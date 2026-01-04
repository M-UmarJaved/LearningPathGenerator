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
  summary?: ProgressGraphSummaryDto | null;
  nodes: ProgressGraphNodeDto[];
  edges: ProgressGraphEdgeDto[];
};

@Injectable({ providedIn: 'root' })
export class ProgressGraphService {
  private http = inject(HttpClient);

  private readonly apiBase = '/api';

  getGraph(userId: number) {
    return this.getWithFallback<ProgressGraphResponseDto>(`/progress-graph?userId=${userId}`);
  }

  private getWithFallback<T>(path: string) {
    return this.http.get<T>(this.apiBase + path).pipe(catchError((err) => throwError(() => err)));
  }
}
