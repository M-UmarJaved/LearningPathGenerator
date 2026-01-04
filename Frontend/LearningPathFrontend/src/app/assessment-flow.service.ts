import { Injectable } from '@angular/core';

export type SkillSummary = {
  skillId: number;
  skillName: string;
  correctAnswers: number;
  totalAnswered: number;
  skillLevel: string;
};

export type SelectedSkill = {
  skillId: number;
  skillName: string;
};

@Injectable({ providedIn: 'root' })
export class AssessmentFlowService {
  private _selectedSkills: SelectedSkill[] = [];
  private _currentSkillIndex = 0;
  private _results: SkillSummary[] = [];

  setSelectedSkills(skills: SelectedSkill[]) {
    this._selectedSkills = [...skills];
    this._currentSkillIndex = 0;
    this._results = [];
  }

  get selectedSkills(): SelectedSkill[] {
    return this._selectedSkills;
  }

  get currentSkill(): SelectedSkill | null {
    return this._selectedSkills[this._currentSkillIndex] ?? null;
  }

  get hasNextSkill(): boolean {
    return this._currentSkillIndex + 1 < this._selectedSkills.length;
  }

  advanceToNextSkill(): SelectedSkill | null {
    if (!this.hasNextSkill) return null;
    this._currentSkillIndex++;
    return this.currentSkill;
  }

  addResult(result: SkillSummary) {
    this._results.push(result);
  }

  get results(): SkillSummary[] {
    return this._results;
  }

  reset() {
    this._selectedSkills = [];
    this._currentSkillIndex = 0;
    this._results = [];
  }
}
