import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, of, throwError } from 'rxjs';

export type SkillDto = {
  skillId: number;
  skillName: string;
  description?: string | null;
  isActive: boolean;
};

export type CourseDto = {
  courseId: number;
  skillId: number;
  courseTitle: string;
  courseLevel: string;
  totalVideos: number;
  sequenceOrder: number;
  isCompleted: boolean;
  completionPercentage: number;
};

export type CourseVideoDto = {
  videoIndex: number;
  videoTitle: string;
  youtubeVideoUrl: string;
};

export type CourseDetailsDto = {
  courseId: number;
  skillId: number;
  courseTitle: string;
  courseLevel: string;
  sequenceOrder: number;
  totalVideos: number;
  videos: CourseVideoDto[];
};

export type LearningPathDto = {
  pathId: number;
  userId: number;
  skillId: number;
  skillName: string;
  createdAt: string;
  status: string;
  skillCompletionPercentage: number;
  activeCourseId?: number | null;
  courses: CourseDto[];
};

export type ProgressDto = {
  pathId: number;
  skillId: number;
  courseId: number;
  courseCompletionPercentage: number;
  courseCompleted: boolean;
  skillCompletionPercentage: number;
  pathStatus: string;
  nextCourseId?: number | null;
};

export type CourseProgressDto = {
  userId: number;
  courseId: number;
  totalVideos: number;
  completionPercentage: number;
  watchedVideoIndexes: number[];
};

export type CourseResumeDto = {
  userId: number;
  courseId: number;
  lastVideoIndex: number;
  lastPositionSeconds: number;
  updatedAt: string;
};

export type InProgressCourseDto = {
  courseId: number;
  skillId: number;
  skillName?: string;
  courseTitle: string;
  courseLevel: string;
  completionPercentage: number;
  lastVideoIndex: number;
  lastPositionSeconds: number;
  lastResumeAt?: string | null;
};

@Injectable({ providedIn: 'root' })
export class LearningPathService {
  private http = inject(HttpClient);

  // Use relative base. In dev, Angular proxy routes /api -> backend.
  private readonly apiBase = '/api';

  getSkills() {
    return this.getWithFallback<SkillDto[]>('/skills');
  }

  generateLearningPath(userId: number, skillId: number) {
    return this.postWithFallback<LearningPathDto>('/learning-path/generate', { userId, skillId });
  }

  getLearningPath(pathId: number) {
    return this.getWithFallback<LearningPathDto>(`/learning-path/${pathId}`);
  }

  getCourse(courseId: number) {
    return this.getWithFallback<CourseDetailsDto>(`/courses/${courseId}`);
  }

  getCourseProgress(userId: number, courseId: number) {
    return this.getWithFallback<CourseProgressDto>(`/progress/course/${courseId}?userId=${userId}`);
  }

  markVideo(userId: number, courseId: number, videoIndex: number, isWatched: boolean) {
    return this.postWithFallback<ProgressDto>('/progress/video', { userId, courseId, videoIndex, isWatched });
  }

  getCourseResume(userId: number, courseId: number) {
    const path = `${this.apiBase}/progress/resume/${courseId}?userId=${userId}`;
    return this.http.get<CourseResumeDto>(path).pipe(
      catchError((err) => {
        // No resume yet is normal; backend may respond 404 or 204.
        if (err?.status === 404) return of(null);
        return throwError(() => err);
      })
    );
  }

  upsertCourseResume(userId: number, courseId: number, lastVideoIndex: number, lastPositionSeconds: number) {
    return this.postWithFallback<CourseResumeDto>('/progress/resume', { userId, courseId, lastVideoIndex, lastPositionSeconds });
  }

  getInProgressCourses(userId: number) {
    return this.getWithFallback<InProgressCourseDto[]>(`/progress/in-progress?userId=${userId}`);
  }

  private getWithFallback<T>(path: string) {
    return this.http.get<T>(this.apiBase + path).pipe(catchError((err) => throwError(() => err)));
  }

  private postWithFallback<T>(path: string, body: any) {
    return this.http.post<T>(this.apiBase + path, body).pipe(catchError((err) => throwError(() => err)));
  }
}
