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

export const routes: Routes = [
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: '',
    component: LayoutComponent, // Layout chính của ứng dụng
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        component: DashboardComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'page',
        component: PageComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'user',
        component: UserComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'role',
        component: RoleComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'config-json',
        component: ConfigJsonComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'student',
        component: StudentListComponent,
        canActivate: [AuthGuard],
      },
      {
        path: 'student/:id',
        component: StudentDetailComponent,
        canActivate: [AuthGuard],
      },
    ],
  },
  { path: 'error/:statusCode', component: PageErrorComponent },
  { path: '**', component: Page404Component }, // Wildcard route for 404 page
];
