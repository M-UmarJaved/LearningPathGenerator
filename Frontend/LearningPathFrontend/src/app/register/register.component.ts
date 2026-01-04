import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'register',
  standalone: true,
  imports:[FormsModule, CommonModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class SignupComponent implements OnInit {
  
 
  isRightPanelActive: boolean = false;

  onSignUpClick(): void {
    this.isRightPanelActive = true;
  }

  onSignInClick(): void {
    this.isRightPanelActive = false;
  }
  
  // Signup Model
  userObj: any = {
    fullName: '',
    email: '',
    password: ''
  };

  private getSignupEmailError(email: string, requireValue: boolean): string | null {
    const value = (email ?? '').toString().trim();

    if (!value) return requireValue ? 'Email is required.' : null;
    if (!value.toLowerCase().endsWith('@gmail.com')) return 'Email must end with @gmail.com.';
    return null;
  }

  private getSignupPasswordError(password: string): string | null {
    const value = (password ?? '').toString();

    if (value.length < 8) return 'Password must be at least 8 characters.';
    if (!/[A-Z]/.test(value)) return 'Password must include at least 1 uppercase letter.';
    if (!/\d/.test(value)) return 'Password must include at least 1 number.';
    if (!/[^A-Za-z0-9]/.test(value)) return 'Password must include at least 1 special character.';
    return null;
  }

  get signupPasswordError(): string | null {
    return this.getSignupPasswordError(this.userObj?.password);
  }

  get signupEmailError(): string | null {
    // UI hint: only show after user starts typing
    return this.getSignupEmailError(this.userObj?.email, false);
  }

  get isSignupPasswordValid(): boolean {
    return this.signupPasswordError === null;
  }

  get isSignupEmailValid(): boolean {
    // Actual gating: email must be present and valid
    return this.getSignupEmailError(this.userObj?.email, true) === null;
  }

  get isSignupFormValid(): boolean {
    return this.isSignupEmailValid && this.isSignupPasswordValid;
  }

  // Login Model
  loginObj: any = {
    email: '',
    password: ''
  };

  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  ngOnInit(): void {
    const fromLanding = this.route.snapshot.queryParamMap.get('fromLanding') === '1';

    // If already logged in, route based on whether initial assessment was completed.
    // if (this.auth.isLoggedIn && !fromLanding) {
    //   this.router.navigateByUrl(this.auth.hasCompletedAssessment ? '/dashboard' : '/skill-table');
    // }
  }

  onSave() {
    const emailError = this.getSignupEmailError(this.userObj?.email, true);
    if (emailError) {
      alert(emailError);
      return;
    }

    const passwordError = this.getSignupPasswordError(this.userObj?.password);
    if (passwordError) {
      alert(passwordError);
      return;
    }

    this.auth.register({
      fullName: this.userObj.fullName,
      email: this.userObj.email,
      password: this.userObj.password
    }).subscribe({
      next: (res: any) => {
        alert(typeof res === 'string' ? res : 'User Registered');
        this.userObj = { fullName: '', email: '', password: '' };
        this.isRightPanelActive = false; // Switch to login panel
      },
      error: (err) => {
        alert('Signup Error: ' + (err?.error ?? err?.message ?? 'Unknown error'));
      }
    });
  }

  onLogin() {
    if (!this.loginObj.email || !this.loginObj.password) {
      alert('Please enter email and password');
      return;
    }

    this.auth.login(this.loginObj).subscribe({
      next: (res: any) => {
        // Be tolerant to casing and types: ASP.NET might return Token/UserId or userId as a string.
        const token = res?.token ?? res?.Token;
        const rawUserId = res?.userId ?? res?.UserId;
        const userId = Number(rawUserId);
        const fullName = res?.fullName ?? res?.FullName ?? res?.userName ?? res?.UserName;
        const hasCompletedAssessment = Boolean(res?.hasCompletedAssessment ?? res?.HasCompletedAssessment);

        if (!token || !Number.isFinite(userId)) {
          console.error('Unexpected login response:', res);
          alert(
            'Login response is invalid. Backend must return token + userId (number).\n' +
              'Expected: { token, userId, fullName, email }'
          );
          return;
        }

        this.auth.setSession({ token, userId, fullName, hasCompletedAssessment });
        this.loginObj = { email: '', password: '' }; // reset

        // New users must complete initial assessment (skill-table -> skill-assessment).
        // Returning users go directly to dashboard.
        this.router.navigateByUrl(hasCompletedAssessment ? '/dashboard' : '/skill-table');
      },
      error: (err) => {
        alert('Login Error: ' + (err?.error ?? err?.message ?? 'Invalid credentials'));
      }
    });
  }
}
