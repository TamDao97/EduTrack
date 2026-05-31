import { Page404Component } from './shared/components/page-404/page-404.component';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { AuthGuard } from './shared/utils/auth/auth-guard';
import { PageErrorComponent } from './shared/components/page-error/page-error.component';
import { Routes } from '@angular/router';
import { PageComponent } from './pages/system/page/page.component';
import { RoleComponent } from './pages/system/role/role.component';
import { UserComponent } from './pages/system/auth/user/user.component';
import { LoginComponent } from './pages/system/auth/login/login.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ConfigJsonComponent } from './pages/system/config-json/config-json.component';
import { StudentListComponent } from './pages/tutor-domain/student/student-list.component';
import { StudentDetailComponent } from './pages/tutor-domain/student/student-detail/student-detail.component';
import { LessonWeekComponent } from './pages/tutor-domain/lesson/lesson-week.component';
import { NotificationInboxComponent } from './pages/tutor-domain/notification/notification-inbox.component';
import { TuitionListComponent } from './pages/tutor-domain/tuition/tuition-list.component';
import { SettingsComponent } from './pages/tutor-domain/settings/settings.component';
import { DesignGalleryComponent } from './_demo/design-gallery.component';
import { OnboardingDemoComponent } from './_demo/onboarding/onboarding-demo.component';
import { DashboardDemoComponent } from './_demo/dashboard/dashboard-demo.component';
import { TuitionDemoComponent } from './_demo/tuition/tuition-demo.component';
import { StudentTableDemoComponent } from './_demo/student-table/student-table-demo.component';
import { SettingsDemoComponent } from './_demo/settings/settings-demo.component';

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
  },
  // ─────────────── DESIGN DEMO (mock data, no API) ───────────────
  { path: 'design',                 component: DesignGalleryComponent },
  { path: 'design/onboarding',      component: OnboardingDemoComponent },
  { path: 'design/dashboard',       component: DashboardDemoComponent },
  { path: 'design/tuition',         component: TuitionDemoComponent },
  { path: 'design/student-table',   component: StudentTableDemoComponent },
  { path: 'design/settings',        component: SettingsDemoComponent },
  // ──────────────────────────────────────────────────────────────
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '',             redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard',    component: DashboardComponent,         canActivate: [AuthGuard] },
      { path: 'page',         component: PageComponent,              canActivate: [AuthGuard] },
      { path: 'user',         component: UserComponent,              canActivate: [AuthGuard] },
      { path: 'role',         component: RoleComponent,              canActivate: [AuthGuard] },
      { path: 'config-json',  component: ConfigJsonComponent,        canActivate: [AuthGuard] },
      { path: 'student',      component: StudentListComponent,       canActivate: [AuthGuard] },
      { path: 'student/:id',  component: StudentDetailComponent,     canActivate: [AuthGuard] },
      { path: 'lesson',       component: LessonWeekComponent,        canActivate: [AuthGuard] },
      { path: 'inbox',        component: NotificationInboxComponent, canActivate: [AuthGuard] },
      { path: 'tuition',      component: TuitionListComponent,       canActivate: [AuthGuard] },
      { path: 'settings',     component: SettingsComponent,          canActivate: [AuthGuard] },
    ],
  },
  { path: 'error/:statusCode', component: PageErrorComponent },
  { path: '**', component: Page404Component },
];
