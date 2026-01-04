import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';
import { InProgressCourseDto, LearningPathService } from '../learning-path.service';

@Component({
  selector: 'app-my-courses',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-courses.component.html',
  styleUrls: ['./my-courses.component.css']
})
export class MyCoursesComponent implements OnInit {
  private auth = inject(AuthService);
  private api = inject(LearningPathService);
  private router = inject(Router);

  loading = false;
  error = '';

  courses: InProgressCourseDto[] = [];
  skillSections: { skillId: number; skillName: string; courses: InProgressCourseDto[] }[] = [];

  ngOnInit(): void {
    const userId = this.auth.userId;
    if (!userId) {
      this.error = 'You are not logged in.';
      return;
    }

    this.loading = true;
    this.api.getInProgressCourses(userId).subscribe({
      next: (res) => {
        this.courses = res ?? [];
        this.skillSections = this.buildSkillSections(this.courses);
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message ?? err?.message ?? 'Failed to load courses.';
      }
    });
  }

  resume(course: InProgressCourseDto) {
    this.router.navigate(['/course-player', course.courseId]);
  }

  private buildSkillSections(courses: InProgressCourseDto[]) {
    const map = new Map<number, { skillId: number; skillName: string; courses: InProgressCourseDto[] }>();

    for (const course of courses) {
      const skillId = course.skillId;
      const skillName = (course.skillName && course.skillName.trim()) ? course.skillName.trim() : `Skill ${skillId}`;

      const existing = map.get(skillId);
      if (existing) {
        existing.courses.push(course);
      } else {
        map.set(skillId, { skillId, skillName, courses: [course] });
      }
    }

    return Array.from(map.values()).sort((a, b) => a.skillName.localeCompare(b.skillName));
  }
}
