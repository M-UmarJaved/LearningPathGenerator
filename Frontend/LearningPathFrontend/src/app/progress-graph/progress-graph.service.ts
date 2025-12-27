import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export type ProgressGraphSummaryDto = {
  coursesCompleted: number;
  videosWatched: number;
  skillsLearned: number;
};

export type ProgressGraphNodeDto = {
  id: number;
  label: string;
  type: 'user' | 'metric' | 'skill' | 'course' | string;
  level: number;
  value?: number | null;
  completed?: boolean | null;
  completionPercentage?: number | null;
  watchedVideos?: number | null;
  totalVideos?: number | null;
};

export type ProgressGraphEdgeDto = {
  fromId: number;
  toId: number;
};

export type ProgressGraphResponseDto = {
  summary: ProgressGraphSummaryDto;
  nodes: ProgressGraphNodeDto[];
  edges: ProgressGraphEdgeDto[];
};

@Injectable({ providedIn: 'root' })
export class ProgressGraphService {
  private http = inject(HttpClient);

  private readonly apiBases = ['https://localhost:7115/api', 'http://localhost:5196/api'];

  getGraph(userId: number) {
    return this.getWithFallback<ProgressGraphResponseDto>(`/progress-graph?userId=${userId}`);
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
}
