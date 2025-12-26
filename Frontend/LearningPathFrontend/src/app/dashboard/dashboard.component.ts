import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AssessmentFlowService } from '../assessment-flow.service';
import { AuthService } from '../auth.service';

interface Course {
  id: number;
  title: string;
  instructor: string;
  progress: number;
  duration: string;
  thumbnail: string;
  difficulty: 'Beginner' | 'Intermediate' | 'Advanced';
  category: string;
  rating: number;
  enrolled: number;
}

interface LearningPath {
  id: number;
  name: string;
  description: string;
  courses: number;
  progress: number;
  estimatedTime: string;
  icon: string;
}

interface Stats {
  coursesCompleted: number;
  hoursLearned: number;
  currentStreak: number;
  skillsAcquired: number;
}

@Component({
  selector: 'dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private router = inject(Router);
  private auth = inject(AuthService);
  private flow = inject(AssessmentFlowService);

  userName: string = '';
  
  // Stats
  stats: Stats = {
    coursesCompleted: 12,
    hoursLearned: 148,
    currentStreak: 7,
    skillsAcquired: 24
  };

  // Learning Paths
  learningPaths: LearningPath[] = [
    {
      id: 1,
      name: 'Data Structures & Algorithms',
      description: 'Master fundamental CS concepts with hands-on practice',
      courses: 8,
      progress: 65,
      estimatedTime: '12 weeks',
      icon: '🧩'
    },
    {
      id: 2,
      name: 'Full Stack Development',
      description: 'Build modern web applications from frontend to backend',
      courses: 12,
      progress: 40,
      estimatedTime: '16 weeks',
      icon: '💻'
    },
    {
      id: 3,
      name: 'Machine Learning Foundations',
      description: 'Explore AI and ML with practical implementations',
      courses: 10,
      progress: 20,
      estimatedTime: '14 weeks',
      icon: '🤖'
    }
  ];

  // Active/Recommended Courses
  activeCourses: Course[] = [
    {
      id: 1,
      title: 'Advanced Data Structures',
      instructor: 'Dr. Sarah Johnson',
      progress: 75,
      duration: '8 hours',
      thumbnail: 'https://images.unsplash.com/photo-1516116216624-53e697fedbea?w=400',
      difficulty: 'Advanced',
      category: 'Computer Science',
      rating: 4.8,
      enrolled: 15420
    },
    {
      id: 2,
      title: 'React & TypeScript Masterclass',
      instructor: 'Mike Chen',
      progress: 45,
      duration: '12 hours',
      thumbnail: 'https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=400',
      difficulty: 'Intermediate',
      category: 'Web Development',
      rating: 4.9,
      enrolled: 23100
    },
    {
      id: 3,
      title: 'Python for Data Science',
      instructor: 'Prof. Emily Watson',
      progress: 30,
      duration: '10 hours',
      thumbnail: 'https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?w=400',
      difficulty: 'Beginner',
      category: 'Data Science',
      rating: 4.7,
      enrolled: 31500
    }
  ];

  recommendedCourses: Course[] = [
    {
      id: 4,
      title: 'System Design Fundamentals',
      instructor: 'Alex Turner',
      progress: 0,
      duration: '6 hours',
      thumbnail: 'https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=400',
      difficulty: 'Advanced',
      category: 'Software Engineering',
      rating: 4.9,
      enrolled: 18200
    },
    {
      id: 5,
      title: 'Node.js Backend Development',
      instructor: 'Rachel Kim',
      progress: 0,
      duration: '9 hours',
      thumbnail: 'https://images.unsplash.com/photo-1627398242454-45a1465c2479?w=400',
      difficulty: 'Intermediate',
      category: 'Backend',
      rating: 4.6,
      enrolled: 12800
    },
    {
      id: 6,
      title: 'UI/UX Design Principles',
      instructor: 'David Park',
      progress: 0,
      duration: '7 hours',
      thumbnail: 'https://images.unsplash.com/photo-1581291518633-83b4ebd1d83e?w=400',
      difficulty: 'Beginner',
      category: 'Design',
      rating: 4.8,
      enrolled: 9600
    }
  ];

  // Recent Activity
  recentActivity = [
    { action: 'Completed lesson', course: 'Advanced Data Structures', time: '2 hours ago' },
    { action: 'Started course', course: 'React & TypeScript Masterclass', time: '1 day ago' },
    { action: 'Earned certificate', course: 'JavaScript Fundamentals', time: '3 days ago' }
  ];

  ngOnInit(): void {
    this.userName = this.auth.userName || 'Student';
    
    // Check if user is logged in
    // if (!this.auth.isLoggedIn) {
    //   this.router.navigateByUrl('/register');
    // }
  }

  getDifficultyClass(difficulty: string): string {
    const classes: any = {
      'Beginner': 'difficulty-beginner',
      'Intermediate': 'difficulty-intermediate',
      'Advanced': 'difficulty-advanced'
    };
    return classes[difficulty] || '';
  }

  continueCourse(course: Course): void {
    console.log('Continue course:', course.title);
    // TODO: Navigate to course player
  }

  explorePath(path: LearningPath): void {
    console.log('Explore path:', path.name);
    // TODO: Navigate to learning path details
  }

  startCourse(course: Course): void {
    console.log('Start course:', course.title);
    // TODO: Enroll and start course
  }

  startNewAssessment(): void {
    this.flow.reset();
    this.router.navigateByUrl('/skill-table');
  }

  startLearning(): void {
    this.router.navigateByUrl('/learning-skill-select');
  }

  logout(): void {
    this.auth.clearSession();
    this.router.navigateByUrl('/register');
  }
}
