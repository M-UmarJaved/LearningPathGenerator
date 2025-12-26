import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AssessmentFlowService, SelectedSkill } from '../assessment-flow.service';
import { AuthService } from '../auth.service';

type Skill = {
  skillId: number;
  skillName: string;
};

@Component({
  selector: 'app-skill-table',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './skill-table.component.html',
  styleUrls: ['./skill-table.component.css']
})
export class SkillTableComponent implements OnInit {
  private http = inject(HttpClient);
  private router = inject(Router);
  private flow = inject(AssessmentFlowService);
  private auth = inject(AuthService);

  apiUrl = 'https://localhost:7115/api/skills';

  skills: Skill[] = [];
  selected: SelectedSkill[] = [];
  loading = false;
  error = '';

  ngOnInit(): void {
    if (!this.auth.isLoggedIn) {
      this.router.navigateByUrl('/register');
      return;
    }
    this.loadSkills();
  }

  private loadSkills() {
    this.loading = true;
    this.error = '';
    this.http.get<Skill[]>(this.apiUrl).subscribe({
      next: (res) => {
        this.skills = res ?? [];
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.message ?? 'Failed to load skills.';
      }
    });
  }

  isSelected(skillId: number): boolean {
    return this.selected.some(s => s.skillId === skillId);
  }

  toggle(skill: Skill, checked: boolean) {
    if (checked) {
      if (!this.isSelected(skill.skillId)) {
        this.selected.push({ skillId: skill.skillId, skillName: skill.skillName });
      }
      return;
    }
    this.selected = this.selected.filter(s => s.skillId !== skill.skillId);
  }

  getSelectionOrder(skillId: number): string {
    const idx = this.selected.findIndex(x => x.skillId === skillId);
    return idx >= 0 ? String(idx + 1) : '-';
  }

  get selectedNames(): string {
    return this.selected.map(s => s.skillName).join(', ');
  }

  startAssessment() {
    if (this.selected.length === 0) return;
    this.flow.setSelectedSkills(this.selected);
    this.router.navigateByUrl('/skill-assessment');
  }

}
