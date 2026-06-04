import { Page404Component } from './shared/components/page-404/page-404.component';
import { LayoutComponent } from './shared/components/layout/layout.component';
import { AuthGuard } from './shared/utils/auth/auth-guard';
import { SuperGuard } from './shared/utils/auth/super-guard';
import { PageErrorComponent } from './shared/components/page-error/page-error.component';
import { Routes } from '@angular/router';
import { PageComponent } from './pages/system/page/page.component';
import { RoleComponent } from './pages/system/role/role.component';
import { UserComponent } from './pages/system/auth/user/user.component';
import { LoginComponent } from './pages/system/auth/login/login.component';
import { SignupComponent } from './pages/system/auth/signup/signup.component';
import { ForgotPasswordComponent } from './pages/system/auth/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/system/auth/reset-password/reset-password.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { ConfigJsonComponent } from './pages/system/config-json/config-json.component';
import { StudentListComponent } from './pages/tutor-domain/student/student-list.component';
import { StudentDetailComponent } from './pages/tutor-domain/student/student-detail/student-detail.component';
import { LessonWeekComponent } from './pages/tutor-domain/lesson/lesson-week.component';
import { NotificationInboxComponent } from './pages/tutor-domain/notification/notification-inbox.component';
import { TuitionListComponent } from './pages/tutor-domain/tuition/tuition-list.component';
import { SettingsComponent } from './pages/tutor-domain/settings/settings.component';
import { AdminLayoutComponent } from './pages/admin/admin-layout/admin-layout.component';
import { AdminDashboardComponent } from './pages/admin/admin-dashboard.component';
import { AdminPaymentsComponent } from './pages/admin/admin-payments.component';
import { BillingComponent } from './pages/tutor-domain/billing/billing.component';
import { ReportComponent } from './pages/tutor-domain/report/report.component';
import { FeedbackComponent } from './pages/tutor-domain/feedback/feedback.component';
import { ClassListComponent } from './pages/tutor-domain/classroom/class-list.component';
import { ClassDetailComponent } from './pages/tutor-domain/classroom/class-detail.component';
import { AdminFeedbackComponent } from './pages/admin/admin-feedback.component';
import { LandingComponent } from './pages/landing/landing.component';
import { DesignGalleryComponent } from './_demo/design-gallery.component';
import { OnboardingDemoComponent } from './_demo/onboarding/onboarding-demo.component';
import { DashboardDemoComponent } from './_demo/dashboard/dashboard-demo.component';
import { TuitionDemoComponent } from './_demo/tuition/tuition-demo.component';
import { StudentTableDemoComponent } from './_demo/student-table/student-table-demo.component';
import { SettingsDemoComponent } from './_demo/settings/settings-demo.component';

export const routes: Routes = [
  { path: 'login',           component: LoginComponent           },
  { path: 'signup',          component: SignupComponent          },
  { path: 'forgot-password', component: ForgotPasswordComponent  },
  { path: 'reset-password',  component: ResetPasswordComponent   },
  { path: 'landing',         component: LandingComponent         },
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
      { path: 'student',      component: StudentListComponent,       canActivate: [AuthGuard] },
      { path: 'student/:id',  component: StudentDetailComponent,     canActivate: [AuthGuard] },
      { path: 'lesson',       component: LessonWeekComponent,        canActivate: [AuthGuard] },
      { path: 'classroom',    component: ClassListComponent,         canActivate: [AuthGuard] },
      { path: 'classroom/:id', component: ClassDetailComponent,      canActivate: [AuthGuard] },
      { path: 'inbox',        component: NotificationInboxComponent, canActivate: [AuthGuard] },
      { path: 'tuition',      component: TuitionListComponent,       canActivate: [AuthGuard] },
      { path: 'settings',     component: SettingsComponent,          canActivate: [AuthGuard] },
      { path: 'billing',      component: BillingComponent,           canActivate: [AuthGuard] },
      { path: 'report',       component: ReportComponent,            canActivate: [AuthGuard] },
      { path: 'feedback',     component: FeedbackComponent,          canActivate: [AuthGuard] },
    ],
  },
  // ─────────── Platform Console (founder, IsSuper) — shell + nav riêng ───────────
  {
    path: 'admin',
    component: AdminLayoutComponent,
    canActivate: [AuthGuard, SuperGuard],
    children: [
      { path: '',          redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'payments',  component: AdminPaymentsComponent },
      { path: 'feedback',  component: AdminFeedbackComponent },
      { path: 'users',     component: UserComponent },
      { path: 'roles',     component: RoleComponent },
      { path: 'pages',     component: PageComponent },
      { path: 'config',    component: ConfigJsonComponent },
    ],
  },
  // Back-compat: link cũ → khu admin mới
  { path: 'user',        redirectTo: 'admin/users',  pathMatch: 'full' },
  { path: 'role',        redirectTo: 'admin/roles',  pathMatch: 'full' },
  { path: 'page',        redirectTo: 'admin/pages',  pathMatch: 'full' },
  { path: 'config-json', redirectTo: 'admin/config', pathMatch: 'full' },
  { path: 'error/:statusCode', component: PageErrorComponent },
  { path: '**', component: Page404Component },
];
