import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

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

@Injectable({ providedIn: 'root' })
export class LearningPathService {
  private http = inject(HttpClient);

  private readonly apiBases = [
    'https://localhost:7115/api',
    'http://localhost:5196/api'
  ];

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

  private getWithFallback<T>(path: string) {
    const [primary, fallback] = this.apiBases;
    return this.http.get<T>(primary + path).pipe(
      catchError((err) => {
        if (err?.status === 0 && fallback) {
          return this.http.get<T>(fallback + path);
        }
        return throwError(() => err);
      })
    );
  }

  private postWithFallback<T>(path: string, body: any) {
    const [primary, fallback] = this.apiBases;
    return this.http.post<T>(primary + path, body).pipe(
      catchError((err) => {
        if (err?.status === 0 && fallback) {
          return this.http.post<T>(fallback + path, body);
        }
        return throwError(() => err);
      })
    );
  }
}
