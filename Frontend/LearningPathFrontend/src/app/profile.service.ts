import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface UserProfileDto {
  userId: number;
  fullName: string;
  email: string;
  learningPaths: number;
  skillsAssessed: number;
  coursesInPaths: number;
  completedCourses: number;
  videosWatched: number;
  hoursLearned: number;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private http = inject(HttpClient);

  // Use relative base. In dev, Angular proxy routes /api -> backend.
  private readonly apiBase = '/api';

  getProfile(userId: number): Observable<UserProfileDto> {
    const path = `${this.apiBase}/profile/me?userId=${userId}`;
    return this.http.get<UserProfileDto>(path);
  }
}
