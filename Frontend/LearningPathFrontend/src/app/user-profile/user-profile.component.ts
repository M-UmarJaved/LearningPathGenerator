import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, Output, EventEmitter } from '@angular/core';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { ProfileService, UserProfileDto } from '../profile.service';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease-out', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms ease-in', style({ opacity: 0 }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ transform: 'translate(-50%, -50%) scale(0.9)', opacity: 0 }),
        animate('400ms cubic-bezier(0.34, 1.56, 0.64, 1)', 
          style({ transform: 'translate(-50%, -50%) scale(1)', opacity: 1 }))
      ]),
      transition(':leave', [
        animate('250ms ease-in', 
          style({ transform: 'translate(-50%, -50%) scale(0.9)', opacity: 0 }))
      ])
    ])
  ]
})
export class UserProfileComponent implements OnInit {
  private api = inject(ProfileService);
  private auth = inject(AuthService);

  @Output() closeModal = new EventEmitter<void>();

  loading = false;
  error: string | null = null;
  profile: UserProfileDto | null = null;

  ngOnInit(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.close();
      return;
    }
    this.load(userId);
  }

  load(userId: number): void {
    this.loading = true;
    this.error = null;
    this.api.getProfile(userId).subscribe({
      next: (p) => { this.profile = p; this.loading = false; },
      error: (e) => {
        this.loading = false;
        this.error = e?.error?.message ?? e?.message ?? 'Failed to load profile.';
      }
    });
  }

  close(): void {
    this.closeModal.emit();
  }
}
