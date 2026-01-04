import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AssessmentFlowService } from '../assessment-flow.service';
import { AuthService } from '../auth.service';

@Component({
  selector: 'assessment-result',
  imports: [CommonModule],
  templateUrl: './assessment-result.component.html',
  styleUrl: './assessment-result.component.css'
})
export class AssessmentResultComponent {
  private router = inject(Router);
  private flow = inject(AssessmentFlowService);
  private auth = inject(AuthService);

  ngOnInit(): void {
    if (!this.auth.isLoggedIn) {
      this.router.navigateByUrl('/register');
      return;
    }

    // Reaching this screen means at least one assessment finished and was persisted by backend.
    this.auth.setHasCompletedAssessment(true);
  }

  get userName(): string {
    return this.auth.userName;
  }

  get results() {
    return this.flow.results;
  }

  get hasResults(): boolean {
    return this.results.length > 0;
  }

  startNew() {
    this.flow.reset();
    this.router.navigateByUrl('/skill-table');
  }

  logout() {
    this.flow.reset();
    this.auth.clearSession();
    this.router.navigateByUrl('/register');
  }

  continueToDashboard() {
    this.router.navigateByUrl('/dashboard');
  }

  getLevelColor(level: string): string {
    const levelLower = level.toLowerCase();
    switch (levelLower) {
      case 'beginner':
        return '#22d3ee'; // Cyan
      case 'intermediate':
        return '#fbbf24'; // Amber
      case 'advanced':
        return '#34d399'; // Emerald
      case 'expert':
        return '#a78bfa'; // Purple
      default:
        return '#cbd5e1'; // Gray
    }
  }

  getAccuracyColor(accuracy: number): string {
    if (accuracy >= 80) {
      return '#34d399'; // Green
    } else if (accuracy >= 60) {
      return '#fbbf24'; // Amber
    } else {
      return '#f87171'; // Red
    }
  }
}
