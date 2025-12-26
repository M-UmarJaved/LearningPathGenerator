import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { AuthService } from '../auth.service';
import { CourseDetailsDto, LearningPathService, ProgressDto } from '../learning-path.service';

@Component({
  selector: 'app-course-player',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-player.component.html',
  styleUrls: ['./course-player.component.css']
})
export class CoursePlayerComponent implements OnInit {
  private auth = inject(AuthService);
  private api = inject(LearningPathService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private sanitizer = inject(DomSanitizer);

  course: CourseDetailsDto | null = null;
  embedUrl: SafeResourceUrl | null = null;

  selectedVideoIndex = 1;

  watched: boolean[] = [];
  coursePct = 0;
  skillPct = 0;
  pathStatus = 'Active';

  loading = false;
  error = '';
  message = '';

  ngOnInit(): void {
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    //   return;
    // }

    const courseId = Number(this.route.snapshot.paramMap.get('courseId'));
    if (!Number.isFinite(courseId) || courseId <= 0) {
      this.error = 'Invalid course id.';
      return;
    }

    this.loadCourse(courseId);
  }

  private loadCourse(courseId: number) {
    const userId = this.auth.userId;
    if (!userId) return;

    this.loading = true;
    this.error = '';
    this.message = '';

    this.api.getCourse(courseId).subscribe({
      next: (c) => {
        this.course = c;
        const firstUrl = c.videos?.[0]?.youtubeVideoUrl;
        this.selectedVideoIndex = c.videos?.[0]?.videoIndex ?? 1;
        this.embedUrl = firstUrl ? this.toEmbedUrl(firstUrl) : null;

        // load watched state from backend
        this.api.getCourseProgress(userId, courseId).subscribe({
          next: (p) => {
            const total = p.totalVideos ?? c.totalVideos;
            this.watched = Array.from({ length: total }, (_, idx) => p.watchedVideoIndexes?.includes(idx + 1) ?? false);
            this.coursePct = p.completionPercentage ?? 0;
            this.loading = false;
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

  selectVideo(videoIndex: number) {
    if (!this.course) return;
    const v = this.course.videos.find(x => x.videoIndex === videoIndex);
    if (!v) return;
    this.selectedVideoIndex = v.videoIndex;
    this.embedUrl = this.toEmbedUrl(v.youtubeVideoUrl);
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

  private toEmbedUrl(url: string): SafeResourceUrl {
    try {
      const u = new URL(url);

      const host = u.hostname.replace('www.', '');

      // If user stored a playlist URL like: https://www.youtube.com/playlist?list=PLxxxx
      // embed as a playlist player.
      const listId = u.searchParams.get('list');
      if (listId && (u.pathname === '/playlist' || u.pathname === '/watch' && !u.searchParams.get('v'))) {
        return this.sanitizer.bypassSecurityTrustResourceUrl(
          `https://www.youtube-nocookie.com/embed/videoseries?list=${encodeURIComponent(listId)}`
        );
      }

      // youtu.be/<id>
      if (host === 'youtu.be') {
        const id = u.pathname.replace('/', '');
        return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube-nocookie.com/embed/${encodeURIComponent(id)}`);
      }

      // youtube.com/watch?v=<id>
      const v = u.searchParams.get('v');
      if (v) {
        // If watch URL includes a playlist, keep the list context.
        const playlistSuffix = listId ? `?list=${encodeURIComponent(listId)}` : '';
        return this.sanitizer.bypassSecurityTrustResourceUrl(
          `https://www.youtube-nocookie.com/embed/${encodeURIComponent(v)}${playlistSuffix}`
        );
      }

      // youtube.com/shorts/<id>
      if (u.pathname.startsWith('/shorts/')) {
        const id = u.pathname.split('/shorts/')[1]?.split('/')[0];
        if (id) {
          return this.sanitizer.bypassSecurityTrustResourceUrl(
            `https://www.youtube-nocookie.com/embed/${encodeURIComponent(id)}`
          );
        }
      }

      // already /embed/<id>
      return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    } catch {
      return this.sanitizer.bypassSecurityTrustResourceUrl(url);
    }
  }
}
