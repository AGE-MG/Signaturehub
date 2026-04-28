import { Component, OnInit } from '@angular/core';
import { MatAnchor } from "@angular/material/button";
import { MatIcon } from "@angular/material/icon";
import { MatCard } from "@angular/material/card";
import { MatTableModule } from "@angular/material/table";
import { MatChip } from "@angular/material/chips";
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { forkJoin } from 'rxjs';
import { DashboardStats, DocumentStatus, RecentDocument } from '../../../core/models/dasboard.model';

interface StatCard {
  title: string;
  value: number;
  icon: string;
  color: string;
  trend?: {
    value: string;
    isPositive: boolean;
  }
}
@Component({
  selector: 'app-dashboard',
  imports: [MatAnchor, MatIcon, MatCard, MatTableModule, MatChip, MatProgressSpinner, DatePipe],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {


  userName: string = '';
  loading = true;
  stats: StatCard[] = [];
  recentDocuments: RecentDocument[] = [];
  displayedColumns: string[] = ['title', 'status', 'progress', 'date', 'actions'];

  constructor(
    private router: Router,
    private authService: AuthService,
    private dashboardService: DashboardService
  ) { }

  ngOnInit(): void {
    this.loadUserName();
    this.loadDashboardData();
  }

  private loadUserName(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        const firstName = user.fullName.split(' ')[0];
        this.userName = firstName;
      }
    })
  }

  private loadDashboardData(): void {
    this.loading = true;

    forkJoin({
    stats: this.dashboardService.getStats(),
    documents: this.dashboardService.getRecentDocuments(5)
    }).subscribe({
      next: ({ stats, documents }) => {
        this.buildStatsCards(stats);
        this.recentDocuments = documents;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard data', error);
        this.loading = false;
      }
    });
  }

  private buildStatsCards(stats: DashboardStats): void {
    this.stats = [
      {
        title: 'Documentos Pendentes',
        value: stats.pendingDocuments,
        icon: 'pending_actions',
        color: '#FFA000'
      },
      {
        title: 'Documentos Concluídos',
        value: stats.completedDocuments,
        icon: 'check_circle',
        color: '#4CAF50'
      },
      {
        title: 'Total de Documentos',
        value: stats.totalDocuments,
        icon: 'description',
        color: '#1565C0'
      },
      {
        title: 'Notificações Não Lidas',
        value: stats.unreadNotifications,
        icon: 'notifications',
        color: '#F44336'
      }
    ]
  }

  getStatusColor(status: DocumentStatus): 'warn' | 'primary' | 'accent' | undefined {
    switch (status) {
      case DocumentStatus.Completed:
        return 'primary';
      case DocumentStatus.PendingSignatures:
      case DocumentStatus.PartiallyCompleted:
        return 'warn';
      case DocumentStatus.Rejected:
      case DocumentStatus.Expired:
        return 'accent';
      default:
        return undefined;
    }
  }

  getStatusLabel(status: DocumentStatus): string {
    const labels: { [key: string]: string } = {
      [DocumentStatus.Draft]: 'Rascunho',
      [DocumentStatus.PendingSignatures]: 'Pendente',
      [DocumentStatus.PartiallyCompleted]: 'Parcialmente Concluído',
      [DocumentStatus.Completed]: 'Concluído',
      [DocumentStatus.Rejected]: 'Rejeitado',
      [DocumentStatus.Expired]: 'Expirado',
      [DocumentStatus.Cancelled]: 'Cancelado'
    };
    return labels[status] || 'Desconhecido';
  }

  viewDocument(doc: RecentDocument): void {
    this.router.navigate(['/documents', doc.id]);
  }

  goToPendingSignatures(): void {
    this.router.navigate(['/pending-signatures']);
  }

  goToDocuments(): void {
    this.router.navigate(['/documents']);
  }
}
