import { Component, OnInit } from '@angular/core';
import { MatAnchor } from "@angular/material/button";
import { MatIcon } from "@angular/material/icon";
import { MatCard } from "@angular/material/card";
import { MatTableModule } from "@angular/material/table";
import { MatChip } from "@angular/material/chips";
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

interface StatCard {
  title: string;
  value: string | number;
  icon: string;
  color: string;
  trend?: {
    value: string;
    isPositive: boolean;
  }
}

interface RecentDocument {
  id: string;
  title: string;
  status: 'pending' | 'signed' | 'rejected' | 'expired';
  createdAt: Date;
  signers: number;
  signedCount: number;
}


@Component({
  selector: 'app-dashboard',
  imports: [MatAnchor, MatIcon, MatCard, MatTableModule, MatChip],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {

  userName = '';

  stats: StatCard[] = [
    {
      title: 'Documentos Pendentes',
      value: 5,
      icon: 'pending_actions',
      color: '#FFA000',
      trend: { value: '+2', isPositive: false }
    },
    {
      title: 'Assinados Hoje',
      value: 12,
      icon: 'check_circle',
      color: '#4CAF50',
      trend: { value: '+5', isPositive: true }
    },
    {
      title: 'Total de Documentos',
      value: 156,
      icon: 'description',
      color: '#1565C0',
      trend: { value: '+18', isPositive: true }
    },
    {
      title: 'Aguardando Outros',
      value: 8,
      icon: 'hourglass_empty',
      color: '#757575'
    }
  ]

  recentDocuments: RecentDocument[] = [
    {
      id: '1',
      title: 'Contrato de Prestação de Serviços - Empresa X',
      status: 'pending',
      createdAt: new Date(2024, 2, 10),
      signers: 3,
      signedCount: 1
    },
    {
      id: '2',
      title: 'Termo de Confidencialidade - Projeto Y',
      status: 'signed',
      createdAt: new Date(2024, 2, 9),
      signers: 2,
      signedCount: 2
    },
    {
      id: '3',
      title: 'Relatório Mensal - Março/2024',
      status: 'pending',
      createdAt: new Date(2024, 2, 8),
      signers: 5,
      signedCount: 3
    }
  ]

  displayedColumns: string[] = ['title', 'status', 'progress', 'date', 'actions'];

  constructor(
    private router: Router,
    private authService: AuthService
  ) { }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.userName = user.fullName.split(' ')[0]; //
      }
    });
  }

  getStatusColor(status: string): string {
    const colors: { [key: string]: string } = {
      pending: 'warn',
      signed: 'primary',
      rejected: 'accent',
      expired: ''
    };
    return colors[status] || '';
  }

  getStatusLabel(status: string): string {
    const labels: { [key: string]: string } = {
      pending: 'Pendente',
      signed: 'Assinado',
      rejected: 'Rejeitado',
      expired: 'Expirado'
    };
    return labels[status] || status;
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
