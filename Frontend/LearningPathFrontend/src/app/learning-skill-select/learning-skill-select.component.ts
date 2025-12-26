import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { LearningPathService, SkillDto } from '../learning-path.service';

@Component({
  selector: 'app-learning-skill-select',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './learning-skill-select.component.html',
  styleUrls: ['./learning-skill-select.component.css']
})
export class LearningSkillSelectComponent implements OnInit {
  private auth = inject(AuthService);
  private api = inject(LearningPathService);
  private router = inject(Router);

  skills: SkillDto[] = [];
  selectedSkillId: number | null = null;

  loading = false;
  error = '';

  ngOnInit(): void {
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    //   return;
    // }
    this.loadSkills();
  }

  private loadSkills() {
    this.loading = true;
    this.error = '';

    this.api.getSkills().subscribe({
      next: (skills) => {
        this.skills = (skills ?? []).filter(s => s.isActive !== false);
        this.loading = false;
        if (this.skills.length > 0) this.selectedSkillId = this.skills[0].skillId;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load skills.';
      }
    });
  }

  generatePath() {
    const userId = this.auth.userId;
    if (!userId) {
      // this.router.navigateByUrl('/register');
      return;
    }
    if (!this.selectedSkillId) return;

    this.loading = true;
    this.error = '';

    this.api.generateLearningPath(userId, this.selectedSkillId).subscribe({
      next: (path) => {
        this.loading = false;
        this.router.navigate(['/learning-path', path.pathId]);
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to generate learning path.';
      }
    });
  }
}
