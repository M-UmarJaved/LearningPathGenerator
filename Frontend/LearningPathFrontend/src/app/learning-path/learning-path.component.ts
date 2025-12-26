import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { LearningPathService, LearningPathDto } from '../learning-path.service';

@Component({
  selector: 'app-learning-path',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './learning-path.component.html',
  styleUrls: ['./learning-path.component.css']
})
export class LearningPathComponent implements OnInit {
  private auth = inject(AuthService);
  private api = inject(LearningPathService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  path: LearningPathDto | null = null;
  loading = false;
  error = '';

  ngOnInit(): void {
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    //   return;
    // }

    const pathId = Number(this.route.snapshot.paramMap.get('pathId'));
    if (!Number.isFinite(pathId) || pathId <= 0) {
      this.error = 'Invalid path id.';
      return;
    }
    this.loadPath(pathId);
  }

  private loadPath(pathId: number) {
    this.loading = true;
    this.error = '';
    this.api.getLearningPath(pathId).subscribe({
      next: (p) => {
        this.path = p;
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load learning path.';
      }
    });
  }

  startLearning() {
    if (!this.path?.activeCourseId) return;
    this.router.navigate(['/course-player', this.path.activeCourseId]);
  }
}
