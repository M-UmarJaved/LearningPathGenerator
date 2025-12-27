import { Routes } from '@angular/router';
import { LearningSkillSelectComponent } from './learning-skill-select/learning-skill-select.component';
import { LearningPathComponent } from './learning-path/learning-path.component';
import { CoursePlayerComponent } from './course-player/course-player.component';
import { CompletionComponent } from './completion/completion.component';
import { SignupComponent } from './register/register.component';
import { SkillAssessmentComponent } from './skill-assessment/skill-assessment.component';
import { SkillTableComponent } from './skill-table/skill-table.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { AssessmentResultComponent } from './assessment-result/assessment-result.component';
import { ConceptDependencyComponent } from './concept-dependency/concept-dependency.component';
import { LandingComponent } from './landing/landing.component';
import { FeaturesComponent } from './features/features.component';
import { AboutComponent } from './about/about.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { ProgressGraphComponent } from './progress-graph/progress-graph.component';

export const routes: Routes = [
    // Landing Website Routes
    { path: '', redirectTo: 'home', pathMatch: 'full' },
    { path: 'home', component: LandingComponent, title: 'LearnPath - Personalized Learning Paths' },
    { path: 'features', component: FeaturesComponent, title: 'Features - LearnPath' },
    { path: 'about', component: AboutComponent, title: 'About Us - LearnPath' },
    
    // Application Routes
    {
        path: 'register',
        component: SignupComponent,
        title: 'Register'
    },
    {
        path: 'reset-password',
        component: ResetPasswordComponent,
        title: 'Reset Password'
    },
    {
        path: 'skill-table',
        component: SkillTableComponent,
        title: 'Select Skills'
    },
    {
        path: 'skill-assessment',
        component: SkillAssessmentComponent,
        title: 'Skill Assessment'
    },
    {
        path: 'dashboard',
        component: DashboardComponent,
        title: 'Dashboard'
    },
    {
        path: 'progress-graph',
        component: ProgressGraphComponent,
        title: 'Progress Graph'
    },
    {
        path: 'assessment-result',
        component: AssessmentResultComponent,
        title: 'Assessment Result'
    },
    {
        path: 'concept-dependency',
        component: ConceptDependencyComponent,
        title: 'Concept Dependency'
    },
    { path: 'learning-skill-select', component: LearningSkillSelectComponent, title: 'Learning' },
    { path: 'learning-path/:pathId', component: LearningPathComponent, title: 'Learning Path' },
    { path: 'course-player/:courseId', component: CoursePlayerComponent, title: 'Course Player' },
    { path: 'completion', component: CompletionComponent, title: 'Completed' },
    
    // Fallback Route
    { path: '**', redirectTo: 'home' },
];
