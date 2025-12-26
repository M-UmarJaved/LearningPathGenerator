import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import {
  ConceptDependencyService,
  ConceptDto,
  Skill,
  SkillGraphDto
} from '../concept-dependency.service';

@Component({
  selector: 'concept-dependency',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './concept-dependency.component.html',
  styleUrls: ['./concept-dependency.component.css']
})
export class ConceptDependencyComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);
  private api = inject(ConceptDependencyService);

  skills: Skill[] = [];
  selectedSkillId: number | null = null;

  concepts: ConceptDto[] = [];
  graph: SkillGraphDto | null = null;

  // Add concept form
  conceptName = '';
  conceptDescription = '';

  // Add prerequisite form
  targetConceptId: number | null = null;
  prerequisiteId: number | null = null;

  loading = false;
  error = '';
  message = '';

  ngOnInit(): void {
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    //   return;
    // }

    this.loadSkills();
  }

  get selectedSkillName(): string {
    const s = this.skills.find(x => x.skillId === this.selectedSkillId);
    return s?.skillName ?? '';
  }

  get conceptNameById(): Record<number, string> {
    const map: Record<number, string> = {};
    const list = this.graph?.concepts ?? this.concepts;
    for (const c of list) {
      map[c.conceptId] = c.name;
    }
    return map;
  }

  onSkillChange() {
    this.message = '';
    this.error = '';
    if (!this.selectedSkillId) {
      this.concepts = [];
      this.graph = null;
      return;
    }
    this.refresh();
  }

  refresh() {
    if (!this.selectedSkillId) return;
    this.loading = true;
    this.error = '';

    // Load concepts and graph in sequence to keep code simple.
    this.api.getConceptsBySkill(this.selectedSkillId).subscribe({
      next: (concepts) => {
        this.concepts = concepts ?? [];
        this.api.getSkillGraph(this.selectedSkillId!).subscribe({
          next: (graph) => {
            this.graph = graph;
            this.loading = false;
          },
          error: (err) => {
            this.graph = null;
            this.loading = false;
            this.error = err?.error?.message ?? err?.message ?? 'Failed to load graph.';
          }
        });
      },
      error: (err) => {
        this.concepts = [];
        this.graph = null;
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load concepts.';
      }
    });
  }

  private loadSkills() {
    this.loading = true;
    this.error = '';

    this.api.getSkills().subscribe({
      next: (skills) => {
        this.skills = skills ?? [];
        this.loading = false;

        if (this.skills.length > 0) {
          this.selectedSkillId = this.skills[0].skillId;
          this.refresh();
        }
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load skills.';
      }
    });
  }

  addConcept() {
    if (!this.selectedSkillId) return;
    const name = this.conceptName.trim();
    const description = this.conceptDescription.trim();
    if (!name) {
      this.error = 'Concept name is required.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';

    this.api.createConcept(this.selectedSkillId, name, description || undefined).subscribe({
      next: () => {
        this.conceptName = '';
        this.conceptDescription = '';
        this.message = 'Concept added.';
        this.refresh();
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to add concept.';
      }
    });
  }

  addPrerequisite() {
    if (!this.selectedSkillId) return;
    if (!this.targetConceptId || !this.prerequisiteId) {
      this.error = 'Select both a concept and its prerequisite.';
      return;
    }

    this.loading = true;
    this.error = '';
    this.message = '';

    this.api.createPrerequisite(this.selectedSkillId, this.targetConceptId, this.prerequisiteId).subscribe({
      next: () => {
        this.message = 'Prerequisite added.';
        this.refresh();
      },
      error: (err) => {
        this.loading = false;
        if (err?.status === 409) {
          this.error = err?.error?.message ?? 'That prerequisite would create a cycle.';
          return;
        }
        this.error = err?.error?.message ?? err?.message ?? 'Failed to add prerequisite.';
      }
    });
  }
}
