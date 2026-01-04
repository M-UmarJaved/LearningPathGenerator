import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-completion',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './completion.component.html',
  styleUrls: ['./completion.component.css']
})
export class CompletionComponent {
  private router = inject(Router);

  goDashboard() {
    this.router.navigateByUrl('/dashboard');
  }

  startNew() {
    this.router.navigateByUrl('/learning-skill-select');
  }
}
