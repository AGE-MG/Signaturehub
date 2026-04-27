import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout/public-layout.component';
import { HomeComponent } from './features/public/home/home.component';
import { FaqComponent } from './features/public/faq/faq.component';
import { PrivacyPolicyComponent } from './features/public/privacy-policy/privacy-policy.component';
import { LoginComponent } from './features/auth/login/login.component';
import { InternalLayoutComponent } from './layout/internal-layout/internal-layout.component';
import { authGuard } from './core/guards/auth.guard';
import { DashboardComponent } from './features/internal/dashboard/dashboard.component';

const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      {
        path: '',
        component: HomeComponent
      },
      {
        path: 'faq',
        component: FaqComponent
      },
      {
        path: 'privacy-policy',
        component: PrivacyPolicyComponent
      }
    ]
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: '',
    component: InternalLayoutComponent,
    canActivate: [authGuard],
    children: [
      {
        path: 'dashboard',
        component: DashboardComponent
      },
      {
        path: 'documents',
        component: DashboardComponent
      },
      {
        path: 'pending-signatures',
        component: DashboardComponent
      },
      {
        path: 'history',
        component: DashboardComponent
      },
      {
        path: 'settings',
        component: DashboardComponent
      },
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
