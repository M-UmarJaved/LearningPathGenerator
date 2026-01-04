import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Use relative base. In dev, Angular proxy routes /api -> backend.
  private readonly apiBase = '/api/auth';

  private readonly tokenKey = 'authToken';
  private readonly userIdKey = 'userId';
  private readonly userNameKey = 'userName';
  private readonly assessmentCompletedKey = 'hasCompletedAssessment';

  http = inject(HttpClient);

  private get hasStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }

  register(data: any) {
    // Backend may return plain text like: "User Registered" (not JSON)
    return this.postTextWithFallback('/register', data);
  }

  login(data: any) {
    return this.postWithFallback('/login', data);
  }

  requestPasswordReset(email: string) {
    return this.postWithFallback<{ message: string; resetToken?: string; expiresAtUtc?: string }>(
      '/password-reset/request',
      { email }
    );
  }

  confirmPasswordReset(data: { email: string; resetToken: string; newPassword: string }) {
    // Backend returns plain text like: "Password reset successful"
    return this.postTextWithFallback('/password-reset/confirm', data);
  }

  private postWithFallback<T>(path: string, data: any) {
    return this.http.post<T>(this.apiBase + path, data).pipe(catchError((err) => throwError(() => err)));
  }

  private postTextWithFallback(path: string, data: any) {
    return this.http
      .post(this.apiBase + path, data, { responseType: 'text' })
      .pipe(catchError((err) => throwError(() => err)));
  }

  setSession(session: { token: string; userId: number; fullName?: string; hasCompletedAssessment?: boolean }) {
    if (!this.hasStorage) return;
    localStorage.setItem(this.tokenKey, session.token);
    localStorage.setItem(this.userIdKey, String(session.userId));
    if (session.fullName) localStorage.setItem(this.userNameKey, session.fullName);
    if (typeof session.hasCompletedAssessment === 'boolean') {
      localStorage.setItem(this.assessmentCompletedKey, String(session.hasCompletedAssessment));
    }
  }

  clearSession() {
    if (!this.hasStorage) return;
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userIdKey);
    localStorage.removeItem(this.userNameKey);
    localStorage.removeItem(this.assessmentCompletedKey);
  }

  setHasCompletedAssessment(value: boolean) {
    if (!this.hasStorage) return;
    localStorage.setItem(this.assessmentCompletedKey, String(value));
  }

  get hasCompletedAssessment(): boolean {
    if (!this.hasStorage) return false;
    return (localStorage.getItem(this.assessmentCompletedKey) ?? 'false') === 'true';
  }

  get userId(): number | null {
    if (!this.hasStorage) return null;
    const raw = localStorage.getItem(this.userIdKey);
    const id = raw ? Number(raw) : NaN;
    return Number.isFinite(id) ? id : null;
  }

  get userName(): string {
    if (!this.hasStorage) return '';
    return localStorage.getItem(this.userNameKey) ?? '';
  }

  get isLoggedIn(): boolean {
    if (!this.hasStorage) return false;
    return !!this.userId && !!localStorage.getItem(this.tokenKey);
  }
}
