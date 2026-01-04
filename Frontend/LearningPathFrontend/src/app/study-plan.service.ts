import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export interface StudyPlanEventDto {
  studyPlanEventId: number;
  userId: number;
  title: string;
  category: string;
  notes?: string | null;
  startAtUtc: string; // ISO
  endAtUtc: string;   // ISO
  skillId?: number | null;
  courseId?: number | null;
  isCompleted: boolean;
}

export interface CreateStudyPlanEventDto {
  userId: number;
  title: string;
  category: string;
  notes?: string | null;
  startAtUtc: string; // ISO
  endAtUtc: string;   // ISO
  skillId?: number | null;
  courseId?: number | null;
}

export interface UpdateStudyPlanEventDto {
  title: string;
  category: string;
  notes?: string | null;
  startAtUtc: string; // ISO
  endAtUtc: string;   // ISO
  skillId?: number | null;
  courseId?: number | null;
  isCompleted: boolean;
}

@Injectable({ providedIn: 'root' })
export class StudyPlanService {
  private readonly apiBase = '/api/study-plan';

  private http = inject(HttpClient);

  getEvents(userId: number, fromUtc?: string, toUtc?: string) {
    const params = new URLSearchParams({ userId: String(userId) });
    if (fromUtc) params.set('from', fromUtc);
    if (toUtc) params.set('to', toUtc);

    return this.getWithFallback<StudyPlanEventDto[]>(`/events?${params.toString()}`);
  }

  createEvent(dto: CreateStudyPlanEventDto) {
    return this.postWithFallback<StudyPlanEventDto>('/events', dto);
  }

  updateEvent(userId: number, id: number, dto: UpdateStudyPlanEventDto) {
    const qs = new URLSearchParams({ userId: String(userId) }).toString();
    return this.putWithFallback<StudyPlanEventDto>(`/events/${id}?${qs}`, dto);
  }

  deleteEvent(userId: number, id: number) {
    const qs = new URLSearchParams({ userId: String(userId) }).toString();
    return this.deleteWithFallback(`/events/${id}?${qs}`);
  }

  private getWithFallback<T>(path: string) {
    return this.http.get<T>(this.apiBase + path).pipe(catchError((err) => throwError(() => err)));
  }

  private postWithFallback<T>(path: string, body: any) {
    return this.http.post<T>(this.apiBase + path, body).pipe(catchError((err) => throwError(() => err)));
  }

  private putWithFallback<T>(path: string, body: any) {
    return this.http.put<T>(this.apiBase + path, body).pipe(catchError((err) => throwError(() => err)));
  }

  private deleteWithFallback<T>(path: string) {
    return this.http.delete<T>(this.apiBase + path).pipe(catchError((err) => throwError(() => err)));
  }
}
