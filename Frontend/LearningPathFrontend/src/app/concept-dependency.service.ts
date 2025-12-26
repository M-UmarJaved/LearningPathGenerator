import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, throwError } from 'rxjs';

export type Skill = {
  skillId: number;
  skillName: string;
};

export type ConceptDto = {
  conceptId: number;
  skillId: number;
  name: string;
  description?: string | null;
};

export type EdgeDto = {
  fromPrerequisiteId: number;
  toConceptId: number;
};

export type SkillGraphDto = {
  skillId: number;
  skillName: string;
  concepts: ConceptDto[];
  edges: EdgeDto[];
  topologicalOrder: number[];
};

@Injectable({ providedIn: 'root' })
export class ConceptDependencyService {
  private http = inject(HttpClient);

  private readonly apiBases = [
    'https://localhost:7115/api',
    'http://localhost:5196/api'
  ];

  getSkills() {
    return this.getWithFallback<Skill[]>('/skills');
  }

  getConceptsBySkill(skillId: number) {
    return this.getWithFallback<ConceptDto[]>(`/ConceptDependency/skill/${skillId}/concepts`);
  }

  getSkillGraph(skillId: number) {
    return this.getWithFallback<SkillGraphDto>(`/ConceptDependency/skill/${skillId}/graph`);
  }

  createConcept(skillId: number, name: string, description?: string) {
    return this.postWithFallback<ConceptDto>('/ConceptDependency/concept', {
      skillId,
      name,
      description
    });
  }

  createPrerequisite(skillId: number, conceptId: number, prerequisiteId: number) {
    return this.postWithFallback<{ success: boolean }>('/ConceptDependency/prerequisite', {
      skillId,
      conceptId,
      prerequisiteId
    });
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
