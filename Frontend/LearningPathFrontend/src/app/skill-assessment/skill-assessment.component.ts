import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AssessmentFlowService } from '../assessment-flow.service';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-skill-assessment',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './skill-assessment.component.html',
  styleUrls: ['./skill-assessment.component.css']
})
export class SkillAssessmentComponent implements OnInit {

  // -----------------------------
  // CONFIG / STATE
  // -----------------------------
  apiUrl = 'https://localhost:7115/api/skill-assessment';

  userId: number = 0;
  skillId: number = 0;
  skillName: string = '';

  question: any = null;
  selectedOption: string = '';

  // These track the user's score/progress on the client side
  correctCount: number = 0;
  totalAnswered: number = 0;

  assessmentCompleted: boolean = false;
  finalSkillLevel: string = '';

  // Backend rule: each skill assessment is exactly 5 answers.
  private readonly questionsPerAssessment = 5;

  get isFinalQuestion(): boolean {
    // totalAnswered = number already answered; UI shows current as totalAnswered + 1
    return (this.totalAnswered + 1) === this.questionsPerAssessment;
  }

  private updateFromResponse(res: any) {
    const total = (typeof res?.totalCount === 'number') ? res.totalCount : res?.TotalCount;
    const correct = (typeof res?.correctCount === 'number') ? res.correctCount : res?.CorrectCount;

    if (typeof total === 'number') this.totalAnswered = total;
    if (typeof correct === 'number') this.correctCount = correct;
  }

  private normalizeCompleted(res: any): boolean {
    return Boolean(res?.completed ?? res?.Completed);
  }

  private normalizeSkillLevel(res: any): string {
    return (res?.skillLevel ?? res?.SkillLevel ?? '') as string;
  }

  private normalizeNextQuestion(res: any): any {
    return res?.nextQuestion ?? res?.NextQuestion ?? null;
  }

  // -----------------------------
  constructor(
    private http: HttpClient,
    private router: Router,
    private flow: AssessmentFlowService,
    private auth: AuthService
  ) {}

  // -----------------------------
  ngOnInit(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.router.navigateByUrl('/register');
      return;
    }

    const current = this.flow.currentSkill;
    if (!current) {
      this.router.navigateByUrl('/skill-table');
      return;
    }

    this.userId = userId;
    this.skillId = current.skillId;
    this.skillName = current.skillName;
    this.startAssessment();
  }

  // -----------------------------
  // STEP 1: START ASSESSMENT
  // -----------------------------
  startAssessment() {
    // reset per-skill state
    this.question = null;
    this.selectedOption = '';
    this.correctCount = 0;
    this.totalAnswered = 0;

    this.http.get<any>(`${this.apiUrl}/start/${this.skillId}`)
      .subscribe({
        next: (res) => {
          this.question = res;
          this.selectedOption = '';
        },
        error: (err) => {
          console.error('Failed to load first question', err);
        }
      });
  }

  // -----------------------------
  // STEP 2: SUBMIT ANSWER
  // -----------------------------
  submitAnswer() {
    this.postAnswerOrSubmit('answer');
  }

  submitFinal() {
    this.postAnswerOrSubmit('submit');
  }

  private postAnswerOrSubmit(endpoint: 'answer' | 'submit') {
    if (!this.selectedOption) {
      alert('Please select an option');
      return;
    }

    // Match backend AnswerDto (camelCase JSON of C# properties)
    const payload = {
      userId: this.userId,
      skillId: this.skillId,
      currentIndex: this.question.treeIndex,
      selectedOption: this.selectedOption,
      correctCount: this.correctCount,
      totalCount: this.totalAnswered
    };

    // Safety: do not allow calling /submit before the 5th question.
    if (endpoint === 'submit' && !this.isFinalQuestion) {
      alert('You can only submit the assessment on Question 5.');
      return;
    }

    this.http.post<any>(`${this.apiUrl}/${endpoint}`, payload)
      .subscribe({
        next: (res) => {

          this.updateFromResponse(res);

          // Backend FinalAssessmentDto exposes: completed/Completed, nextQuestion/NextQuestion, skillLevel/SkillLevel
          if (this.normalizeCompleted(res)) {
            // Store this skill result
            const level = this.normalizeSkillLevel(res);
            this.flow.addResult({
              skillId: this.skillId,
              skillName: this.skillName,
              correctAnswers: this.correctCount,
              totalAnswered: this.totalAnswered,
              skillLevel: level
            });

            // If there are more selected skills, continue immediately.
            if (this.flow.hasNextSkill) {
              const nextSkill = this.flow.advanceToNextSkill();
              if (!nextSkill) {
                this.router.navigateByUrl('/assessment-result');
                return;
              }
              this.skillId = nextSkill.skillId;
              this.skillName = nextSkill.skillName;
              this.startAssessment();
              return;
            }

            // Otherwise, show results on Assessment Result page
            this.question = null;
            this.assessmentCompleted = true;
            this.finalSkillLevel = level;
            this.router.navigateByUrl('/assessment-result');
            return;
          }

          // /submit should never return an in-progress assessment
          if (endpoint === 'submit') {
            alert('Final submit did not complete. Check backend response and counts.');
            return;
          }

          // Load next question from the tree
          this.question = this.normalizeNextQuestion(res);
          this.selectedOption = '';
        },
        error: (err) => {
          console.error(`Error submitting (${endpoint})`, err);
          if (endpoint === 'submit') {
            alert('Could not submit final assessment. Check backend is running and TotalCount is correct (should be 4 before final submit).');
          }
        }
      });
  }

  // -----------------------------
  // OPTIONAL: RESET ASSESSMENT
  // -----------------------------
  restartAssessment() {
    this.flow.reset();
    this.correctCount = 0;
    this.totalAnswered = 0;
    this.assessmentCompleted = false;
    this.finalSkillLevel = '';
    this.router.navigateByUrl('/skill-table');
  }
}
