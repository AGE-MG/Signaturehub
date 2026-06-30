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
import { DocumentListComponent } from './features/internal/documents/document-list/document-list.component';
import { DocumentUploadComponent } from './features/internal/documents/document-upload/document-upload.component';
import { DocumentDetailsComponent } from './features/internal/documents/document-details/document-details.component';
import { PendingSignaturesComponent } from './features/internal/pending-signatures/pending-signatures.component';
import { HistoryComponent } from './features/internal/history/history.component';
import { SettingsComponent } from './features/internal/settings/settings.component';

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
        children: [
          {
            path: '',
            component: DocumentListComponent
          },
          {
            path: 'upload',
            component: DocumentUploadComponent
          },
          {
            path: ':id',
            component: DocumentDetailsComponent
          }
        ]
      },
      {
        path: 'pending-signatures',
        component: PendingSignaturesComponent
      },
      {
        path: 'history',
        component: HistoryComponent
      },
      {
        path: 'settings',
        component: SettingsComponent
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
