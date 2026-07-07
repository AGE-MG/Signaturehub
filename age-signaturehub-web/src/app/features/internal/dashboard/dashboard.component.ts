import { ApplicationRef, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatAnchor } from "@angular/material/button";
import { MatIcon } from "@angular/material/icon";
import { MatCard } from "@angular/material/card";
import { MatTableModule } from "@angular/material/table";
import { MatChip } from "@angular/material/chips";
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { DatePipe, isPlatformBrowser } from '@angular/common';
import { Inject, NgZone, PLATFORM_ID } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { DashboardService } from '../../../core/services/dashboard.service';
import { asyncScheduler, forkJoin, observeOn } from 'rxjs';
import { DashboardStats, DocumentStatus, RecentDocument } from '../../../core/models/dasboard.model';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentDto } from '../../../core/models/document.model';

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
  displayedColumns: string[] = ['title', 'status', 'date', 'actions'];
  private currentUserEmail: string | null = null;
  private readonly turnByDocumentId: Record<string, 'mine' | 'waiting' | 'none'> = {};
  private readonly FLOW_TYPE_SEQUENTIAL = 1;
  private readonly FLOW_TYPE_PARALLEL = 2;
  private readonly isBrowser: boolean;

  constructor(
    private router: Router,
    private authService: AuthService,
    private dashboardService: DashboardService,
    private documentService: DocumentService,
    private cdr: ChangeDetectorRef,
    private appRef: ApplicationRef,
    private ngZone: NgZone,
    @Inject(PLATFORM_ID) private platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) {
      return;
    }
    this.loadUserName();
    this.currentUserEmail = this.normalizeEmail(this.authService.getUserValue()?.email);
    setTimeout(() => this.loadDashboardData(), 0);
  }

  private loadUserName(): void {
    const user = this.authService.getUserValue();
    const fullName = user?.fullName?.trim();
    const networkUserName = user?.networkUserName?.trim();

    if (fullName) {
      this.userName = fullName.split(' ')[0];
      return;
    }

    if (networkUserName) {
      this.userName = this.extractFriendlyLogin(networkUserName);
      return;
    }

    const email = user?.email?.trim();
    if (email) {
      this.userName = email.split('@')[0];
    }
  }

  private extractFriendlyLogin(value: string): string {
    const normalized = value.trim();

    if (normalized.includes('\\')) {
      return normalized.substring(normalized.lastIndexOf('\\') + 1);
    }

    if (normalized.includes('@')) {
      return normalized.substring(0, normalized.indexOf('@'));
    }

    return normalized;
  }

  private loadDashboardData(): void {
    this.loading = true;

    forkJoin({
    stats: this.dashboardService.getStats(),
    documents: this.dashboardService.getRecentDocuments(5)
    }).pipe(
      observeOn(asyncScheduler)
    ).subscribe({
      next: ({ stats, documents }) => {
        this.ngZone.run(() => {
          const safeStats: DashboardStats = stats ?? {
            totalDocuments: 0,
            draftDocuments: 0,
            pendingDocuments: 0,
            completedDocuments: 0,
            rejectedDocuments: 0,
            expiredDocuments: 0,
            unreadNotifications: 0
          };

          this.buildStatsCards(safeStats);
          this.recentDocuments = Array.isArray(documents) ? documents : [];
          this.updateTurnIndicators(this.recentDocuments);
          this.loading = false;
          this.cdr.detectChanges();
          this.appRef.tick();
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          console.error('Error loading dashboard data', error);
          this.recentDocuments = [];
          this.clearTurnIndicators();
          this.loading = false;
          this.cdr.detectChanges();
          this.appRef.tick();
        });
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

  shouldShowTurnIndicator(doc: RecentDocument): boolean {
    return this.isPendingDocument(doc.status) && !!this.currentUserEmail && this.getTurnState(doc.id) !== 'none';
  }

  isMyTurnToSign(doc: RecentDocument): boolean {
    return this.getTurnState(doc.id) === 'mine';
  }

  isAwaitingOtherSigner(doc: RecentDocument): boolean {
    return this.getTurnState(doc.id) === 'waiting';
  }

  getTurnIndicatorLabel(doc: RecentDocument): string {
    return this.isMyTurnToSign(doc) ? 'Minha vez de assinar' : 'Aguardando outro signatário';
  }

  getTurnIndicatorIcon(doc: RecentDocument): string {
    return this.isMyTurnToSign(doc) ? 'how_to_reg' : 'schedule';
  }

  private updateTurnIndicators(docs: RecentDocument[]): void {
    this.clearTurnIndicators();

    if (!this.currentUserEmail || docs.length === 0) {
      return;
    }

    const pendingDocs = docs.filter((doc) => this.isPendingDocument(doc.status));
    if (pendingDocs.length === 0) {
      return;
    }

    const requests = pendingDocs.map((doc) => this.documentService.getDocumentById(doc.id));
    forkJoin(requests).pipe(observeOn(asyncScheduler)).subscribe({
      next: (fullDocs) => {
        this.ngZone.run(() => {
          for (const fullDoc of fullDocs) {
            this.turnByDocumentId[fullDoc.id] = this.resolveTurnState(fullDoc);
          }
          this.cdr.detectChanges();
          this.appRef.tick();
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.cdr.detectChanges();
          this.appRef.tick();
        });
      }
    });
  }

  private resolveTurnState(doc: DocumentDto): 'mine' | 'waiting' | 'none' {
    if (!this.currentUserEmail || !this.isPendingDocument(doc.status)) {
      return 'none';
    }

    let hasPendingStep = false;

    for (const flow of doc.signatureFlows ?? []) {
      const currentStep = Number(flow.currentStep ?? 0);
      const signers = Array.isArray(flow.signers) ? flow.signers : [];

      const myEligible = signers.some((signer) => {
        const signerEmail = this.normalizeEmail(signer.email);
        const signerStatus = Number(signer.status ?? 0);
        const signerOrder = Number(signer.signOrder ?? 0);
        const isPending = signerStatus === 1 && !signer.signedAt;

        if (!isPending || signerEmail !== this.currentUserEmail) {
          return false;
        }

        if (flow.flowType === this.FLOW_TYPE_PARALLEL) {
          return true;
        }

        if (flow.flowType === this.FLOW_TYPE_SEQUENTIAL) {
          return signerOrder === currentStep;
        }

        return signerOrder <= currentStep;
      });

      if (myEligible) {
        return 'mine';
      }

      const hasFlowPending = signers.some((signer) => Number(signer.status ?? 0) === 1 && !signer.signedAt);
      if (hasFlowPending) {
        hasPendingStep = true;
      }
    }

    return hasPendingStep ? 'waiting' : 'none';
  }

  private isPendingDocument(status: DocumentStatus): boolean {
    return status === DocumentStatus.PendingSignatures || status === DocumentStatus.PartiallyCompleted;
  }

  private getTurnState(documentId: string): 'mine' | 'waiting' | 'none' {
    return this.turnByDocumentId[documentId] ?? 'none';
  }

  private clearTurnIndicators(): void {
    const keys = Object.keys(this.turnByDocumentId);
    for (const key of keys) {
      delete this.turnByDocumentId[key];
    }
  }

  private normalizeEmail(email?: string | null): string | null {
    if (!email) {
      return null;
    }
    return email.trim().toLowerCase();
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
