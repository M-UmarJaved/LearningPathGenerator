import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './reset-password.component.html',
  styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = '';
  resetToken = '';
  newPassword = '';
  confirmPassword = '';

  isRequesting = false;
  isResetting = false;

  infoMessage = '';
  errorMessage = '';

  requestToken() {
    this.clearMessages();
    if (!this.email) {
      this.errorMessage = 'Please enter your email.';
      return;
    }

    this.isRequesting = true;
    this.auth.requestPasswordReset(this.email).subscribe({
      next: (res) => {
        this.infoMessage = res?.message ?? 'Reset token generated. Please check below.';
        if (res?.resetToken) {
          this.resetToken = res.resetToken;
        }
        this.isRequesting = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to request reset token.';
        console.error(err);
        this.isRequesting = false;
      }
    });
  }

  resetPassword() {
    this.clearMessages();

    if (!this.email || !this.resetToken || !this.newPassword || !this.confirmPassword) {
      this.errorMessage = 'Please fill all fields.';
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isResetting = true;
    this.auth
      .confirmPasswordReset({ email: this.email, resetToken: this.resetToken, newPassword: this.newPassword })
      .subscribe({
        next: () => {
          this.infoMessage = 'Password updated successfully. Please sign in.';
          this.isResetting = false;
          // Take user back to register/sign-in page.
          this.router.navigate(['/register'], { queryParams: { fromLanding: 1 } });
        },
        error: (err) => {
          this.errorMessage = err?.error ?? 'Invalid or expired reset token.';
          console.error(err);
          this.isResetting = false;
        }
      });
  }

  private clearMessages() {
    this.infoMessage = '';
    this.errorMessage = '';
  }
}
