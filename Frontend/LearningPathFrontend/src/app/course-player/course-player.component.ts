import { CommonModule } from '@angular/common';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Subscription } from 'rxjs';
import { AuthService } from '../auth.service';
import { CourseDetailsDto, LearningPathService, ProgressDto } from '../learning-path.service';

declare global {
  interface Window {
    YT?: any;
    onYouTubeIframeAPIReady?: () => void;
  }
}

@Component({
  selector: 'app-course-player',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-player.component.html',
  styleUrls: ['./course-player.component.css']
})
export class CoursePlayerComponent implements OnInit, OnDestroy {
  private auth = inject(AuthService);
  private api = inject(LearningPathService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  @ViewChild('ytIframe') ytIframe?: ElementRef<HTMLIFrameElement>;

  course: CourseDetailsDto | null = null;
  embedUrl: SafeResourceUrl | null = null;

  renderIframe = true;

  selectedVideoIndex = 1;

  watched: boolean[] = [];
  coursePct = 0;
  skillPct = 0;
  pathStatus = 'Active';

  loading = false;
  error = '';
  message = '';

  private ytPlayer: any | null = null;
  private ytPlayerReady = false;
  private ytApiPromise: Promise<void> | null = null;
  private pendingSeekSeconds: number | null = null;
  private resumeApplied = false;
  private resumeIntervalId: any;
  private routeSub?: Subscription;

  ngOnInit(): void {
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    //   return;
    // }

    // Subscribe so the player also refreshes when only :courseId changes.
    this.routeSub = this.route.paramMap.subscribe((pm) => {
      const courseId = Number(pm.get('courseId'));
      if (!Number.isFinite(courseId) || courseId <= 0) {
        this.error = 'Invalid course id.';
        return;
      }
      this.loadCourse(courseId);
    });
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    this.stopResumeTracking();
    this.pushResume(true);
    this.destroyPlayer();
  }

  private loadCourse(courseId: number) {
    const userId = this.auth.userId;
    if (!userId) {
      this.error = 'Please register/login first.';
      this.router.navigateByUrl('/register');
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';

    // If the component instance gets reused, make sure player state is reset.
    this.stopResumeTracking();
    this.resumeApplied = false;
    this.pendingSeekSeconds = null;
    this.destroyPlayer();
    this.renderIframe = true;

    console.debug('[CoursePlayer] loadCourse', { courseId, userId });

    this.api.getCourse(courseId).subscribe({
      next: (c) => {
        this.course = c;
        console.debug('[CoursePlayer] course loaded', {
          courseId: c.courseId,
          videos: c.videos?.length ?? 0
        });
        const firstUrl = c.videos?.[0]?.youtubeVideoUrl;
        this.selectedVideoIndex = c.videos?.[0]?.videoIndex ?? 1;

        if (firstUrl) {
          this.switchToVideoUrl(firstUrl, null);
        } else {
          console.debug('[CoursePlayer] missing first video URL; hiding iframe');
          this.embedUrl = null;
        }

        // load watched state from backend
        this.api.getCourseProgress(userId, courseId).subscribe({
          next: (p) => {
            const total = p.totalVideos ?? c.totalVideos;
            this.watched = Array.from({ length: total }, (_, idx) => p.watchedVideoIndexes?.includes(idx + 1) ?? false);
            this.coursePct = p.completionPercentage ?? 0;
            this.loading = false;

            // try resume position (non-fatal if not found)
            this.loadResume(userId, courseId);
          },
          error: (err) => {
            this.loading = false;
            this.error = err?.error?.message ?? err?.message ?? 'Failed to load course progress.';
          }
        });
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load course.';
      }
    });
  }

  private loadResume(userId: number, courseId: number) {
    this.api.getCourseResume(userId, courseId).subscribe({
      next: (r) => {
        if (this.resumeApplied || !this.course) return;

        console.debug('[CoursePlayer] resume response', r);

        const idx = r?.lastVideoIndex;
        const secs = r?.lastPositionSeconds;
        if (!idx || idx <= 0) {
          // No saved position yet: keep current video as-is.
          return;
        }

        const v = this.course.videos.find(x => x.videoIndex === idx);
        if (!v?.youtubeVideoUrl) {
          return;
        }

        this.selectedVideoIndex = v.videoIndex;
        const seekSeconds = typeof secs === 'number' && secs > 0 ? secs : null;
        this.switchToVideoUrl(v.youtubeVideoUrl, seekSeconds);
        this.resumeApplied = true;
      },
      error: () => {
        // ignore missing resume
      }
    });
  }

  selectVideo(videoIndex: number) {
    if (!this.course) return;
    const v = this.course.videos.find(x => x.videoIndex === videoIndex);
    if (!v) return;
    this.selectedVideoIndex = v.videoIndex;

    this.switchToVideoUrl(v.youtubeVideoUrl, 0);
    this.pushResume(true);
  }

  private switchToVideoUrl(youtubeUrl: string, seekSeconds: number | null) {
    this.pendingSeekSeconds = seekSeconds;

    console.debug('[CoursePlayer] switchToVideoUrl', { youtubeUrl, seekSeconds });

    // The YT IFrame API can get "stuck" to the first iframe element.
    // For reliable switching, destroy the player and recreate the iframe node.
    this.destroyPlayer();
    this.renderIframe = false;

    // Defer so Angular removes the iframe from the DOM first.
    Promise.resolve().then(() => {
      try {
        this.embedUrl = this.toEmbedUrl(youtubeUrl, seekSeconds);
      } catch (e) {
        console.error('[CoursePlayer] toEmbedUrl failed', e);
        this.embedUrl = null;
      } finally {
        this.renderIframe = true;
      }
      // Keep URL simple: do not initialize the YouTube IFrame JS API.
    });
  }

  toggleVideo(index1Based: number, checked: boolean) {
    if (!this.course) return;
    const userId = this.auth.userId;
    if (!userId) return;

    this.loading = true;
    this.error = '';
    this.message = '';

    this.api.markVideo(userId, this.course.courseId, index1Based, checked).subscribe({
      next: (res: ProgressDto) => {
        this.loading = false;
        this.watched[index1Based - 1] = checked;
        this.coursePct = res.courseCompletionPercentage;
        this.skillPct = res.skillCompletionPercentage;
        this.pathStatus = res.pathStatus;

        if (res.pathStatus === 'Completed') {
          this.router.navigateByUrl('/completion');
          return;
        }

        if (res.nextCourseId && res.courseCompleted) {
          this.message = 'Course completed. Next course unlocked.';
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to update progress.';
      }
    });
  }

  onVideoCheckboxChange(index1Based: number, evt: Event) {
    const target = evt.target as HTMLInputElement | null;
    const checked = !!target?.checked;
    this.toggleVideo(index1Based, checked);
  }

  private toEmbedUrl(url: string, startSeconds: number | null): SafeResourceUrl {
    const raw = (url ?? '').trim();

    // Support storing just the YouTube video id (11 chars) in the DB.
    // Example: dQw4w9WgXcQ
    if (/^[a-zA-Z0-9_-]{11}$/.test(raw)) {
      const embed = this.withStartParam(`https://www.youtube.com/embed/${encodeURIComponent(raw)}`, startSeconds);
      return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
    }

    const u = this.tryParseUrl(raw);
    if (!u) {
      // Fall back: treat as already-embed URL, but ensure it's absolute.
      return this.sanitizer.bypassSecurityTrustResourceUrl(raw);
    }

    try {
      const host = u.hostname.replace(/^www\./, '').replace(/^m\./, '');

      // If user stored a playlist URL like: https://www.youtube.com/playlist?list=PLxxxx
      // embed as a playlist player.
      const listId = u.searchParams.get('list');
      if (listId && (u.pathname === '/playlist' || u.pathname === '/watch' && !u.searchParams.get('v'))) {
        const embed = this.withStartParam(
          `https://www.youtube.com/embed/videoseries?list=${encodeURIComponent(listId)}`,
          startSeconds
        );
        return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
      }

      // youtu.be/<id>
      if (host === 'youtu.be') {
        const id = u.pathname.replace('/', '');
        const embed = this.withStartParam(`https://www.youtube.com/embed/${encodeURIComponent(id)}`, startSeconds);
        return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
      }

      // youtube.com/watch?v=<id>
      const v = u.searchParams.get('v');
      if (v) {
        // If watch URL includes a playlist, keep the list context.
        const playlistSuffix = listId ? `?list=${encodeURIComponent(listId)}` : '';
        const embed = this.withStartParam(`https://www.youtube.com/embed/${encodeURIComponent(v)}${playlistSuffix}`, startSeconds);
        return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
      }

      // youtube.com/shorts/<id>
      if (u.pathname.startsWith('/shorts/')) {
        const id = u.pathname.split('/shorts/')[1]?.split('/')[0];
        if (id) {
          const embed = this.withStartParam(`https://www.youtube.com/embed/${encodeURIComponent(id)}`, startSeconds);
          return this.sanitizer.bypassSecurityTrustResourceUrl(embed);
        }
      }

      // already /embed/<id>
      return this.sanitizer.bypassSecurityTrustResourceUrl(this.withStartParam(u.toString(), startSeconds));
    } catch {
      return this.sanitizer.bypassSecurityTrustResourceUrl(this.withStartParam(u.toString(), startSeconds));
    }
  }

  private withStartParam(embedUrl: string, startSeconds: number | null): string {
    const start = typeof startSeconds === 'number' ? Math.floor(startSeconds) : 0;
    if (!start || start < 0) return embedUrl;

    const parsed = this.tryParseUrl((embedUrl ?? '').trim());
    try {
      const u = parsed ?? new URL(embedUrl);
      // YouTube embed supports start=<seconds>
      if (!u.searchParams.has('start')) u.searchParams.set('start', String(start));
      return u.toString();
    } catch {
      const sep = embedUrl.includes('?') ? '&' : '?';
      return `${embedUrl}${sep}start=${encodeURIComponent(String(start))}`;
    }
  }

  private tryParseUrl(raw: string): URL | null {
    if (!raw) return null;
    try {
      return new URL(raw);
    } catch {
      // Common DB values: "www.youtube.com/watch?v=..." or "youtube.com/watch?v=..."
      // Make them absolute.
      try {
        if (raw.startsWith('//')) return new URL(`https:${raw}`);
        if (/^[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}(\/|\?|#|$)/.test(raw)) {
          return new URL(`https://${raw}`);
        }
        return null;
      } catch {
        return null;
      }
    }
  }

  private withJsApiParams(embedUrl: string): string {
    const parsed = this.tryParseUrl((embedUrl ?? '').trim());
    try {
      const u = parsed ?? new URL(embedUrl);
      u.searchParams.set('enablejsapi', '1');
      u.searchParams.set('origin', window.location.origin);
      return u.toString();
    } catch {
      const sep = embedUrl.includes('?') ? '&' : '?';
      return `${embedUrl}${sep}enablejsapi=1&origin=${encodeURIComponent(window.location.origin)}`;
    }
  }

  private ensureYouTubeApi(): Promise<void> {
    if (this.ytApiPromise) return this.ytApiPromise;
    if (window.YT?.Player) {
      this.ytApiPromise = Promise.resolve();
      return this.ytApiPromise;
    }

    this.ytApiPromise = new Promise<void>((resolve) => {
      const existing = document.querySelector('script[data-yt-iframe-api]') as HTMLScriptElement | null;

      const resolveWhenReady = () => {
        if (window.YT?.Player) {
          resolve();
          return;
        }

        // Poll for a short time. This avoids a stuck promise when the script tag exists
        // but onYouTubeIframeAPIReady already fired earlier.
        let tries = 0;
        const timer = setInterval(() => {
          tries++;
          if (window.YT?.Player) {
            clearInterval(timer);
            resolve();
            return;
          }
          if (tries >= 80) {
            // ~4s timeout: allow retry later instead of being stuck forever.
            clearInterval(timer);
            this.ytApiPromise = null;
            resolve();
          }
        }, 50);
      };

      if (existing) {
        const prev = window.onYouTubeIframeAPIReady;
        window.onYouTubeIframeAPIReady = () => {
          prev?.();
          resolveWhenReady();
        };
        resolveWhenReady();
        return;
      }

      window.onYouTubeIframeAPIReady = () => resolveWhenReady();
      const tag = document.createElement('script');
      tag.src = 'https://www.youtube.com/iframe_api';
      tag.async = true;
      tag.defer = true;
      tag.setAttribute('data-yt-iframe-api', 'true');
      document.head.appendChild(tag);
    });

    return this.ytApiPromise;
  }

  private async initPlayerIfPossible() {
    if (!this.course || !this.embedUrl) return;
    await this.ensureYouTubeApi();

    // If the API still isn't ready (timeout/pending), retry shortly.
    if (!window.YT?.Player) {
      setTimeout(() => this.initPlayerIfPossible(), 100);
      return;
    }

    const iframe = this.ytIframe?.nativeElement;
    if (!iframe) return;

    this.destroyPlayer();
    this.ytPlayerReady = false;

    this.ytPlayer = new window.YT.Player(iframe, {
      events: {
        onReady: () => {
          this.ytPlayerReady = true;
          if (this.pendingSeekSeconds != null && this.pendingSeekSeconds > 0) {
            try {
              this.ytPlayer.seekTo(this.pendingSeekSeconds, true);
            } catch {
              // ignore
            }
          }
          this.startResumeTracking();
        }
      }
    });
  }

  private destroyPlayer() {
    try {
      this.ytPlayer?.destroy?.();
    } catch {
      // ignore
    }
    this.ytPlayer = null;
    this.ytPlayerReady = false;
  }

  private startResumeTracking() {
    this.stopResumeTracking();
    this.resumeIntervalId = setInterval(() => this.pushResume(false), 5000);
  }

  private stopResumeTracking() {
    if (this.resumeIntervalId) {
      clearInterval(this.resumeIntervalId);
      this.resumeIntervalId = undefined;
    }
  }

  private pushResume(force: boolean) {
    const userId = this.auth.userId;
    const courseId = this.course?.courseId;
    if (!userId || !courseId) return;

    let seconds = 0;
    if (this.ytPlayerReady && this.ytPlayer?.getCurrentTime) {
      try {
        seconds = Math.max(0, Math.floor(Number(this.ytPlayer.getCurrentTime()) || 0));
      } catch {
        seconds = 0;
      }
    }

    if (!force && seconds <= 0) return;

    this.api.upsertCourseResume(userId, courseId, this.selectedVideoIndex, seconds).subscribe({
      next: () => {},
      error: () => {}
    });
  }
}
