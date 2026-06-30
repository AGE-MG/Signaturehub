import { AfterContentChecked, ChangeDetectorRef, Component, NgZone, OnInit } from '@angular/core';
import { DocumentDto, DocumentStatusColor, DocumentStatusLabel, formatFileSize } from '../../../../core/models/document.model';
import { DocumentStatus } from '../../../../core/models/dasboard.model';
import { ActivatedRoute, Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import { MatCardModule } from "@angular/material/card";
import {MatTabChangeEvent, MatTabsModule} from "@angular/material/tabs"
import { MatDivider } from "@angular/material/divider";
import { DatePipe } from '@angular/common';
import { MatProgressBar } from "@angular/material/progress-bar";
import { AuditLogDto } from '../../../../core/models/signer.model';
import { AuditLogService } from '../../../../core/services/signer.service';
import { AuthService } from '../../../../core/services/auth.service';
import { asyncScheduler, observeOn } from 'rxjs';

@Component({
  selector: 'app-document-details',
  imports: [MatProgressSpinner, MatIconModule, MatCardModule, MatTabsModule, MatDivider, DatePipe, MatProgressBar],
  templateUrl: './document-details.component.html',
  styleUrls: ['./document-details.component.scss'],
})
export class DocumentDetailsComponent implements OnInit, AfterContentChecked {
  document: DocumentDto = this.createEmptyDocument();
  auditLogs: AuditLogDto[] = [];
  loadingLogs = false;
  loading = true;
  actionLoading = false;
  documentId = '';
  currentUserEmail: string | null = null;
  totalSignatories = 0;
  signedCount = 0;
  signatureProcess = 0;

  private readonly FLOW_TYPE_SEQUENTIAL = 1;
  private readonly FLOW_TYPE_PARALLEL = 2;

  private readonly ACTION_META: Record<string, { label: string; icon: string; color: string }> = {
    'document_created':    { label: 'Documento criado',    icon: 'add_circle',   color: '#10b981' },
    'document_uploaded':   { label: 'Upload realizado',    icon: 'upload_file',  color: '#3b82f6' },
    'document_signed':     { label: 'Assinado',            icon: 'draw',         color: '#10b981' },
    'document_rejected':   { label: 'Rejeitado',           icon: 'cancel',       color: '#ef4444' },
    'document_expired':    { label: 'Expirado',            icon: 'timer_off',    color: '#8b5cf6' },
    'document_cancelled':  { label: 'Cancelado',           icon: 'block',        color: '#6b7280' },
    'document_downloaded': { label: 'Download',            icon: 'download',     color: '#f59e0b' },
    'document_viewed':     { label: 'Visualizado',         icon: 'visibility',   color: '#64748b' },
    'flow_created':        { label: 'Fluxo criado',        icon: 'account_tree', color: '#3b82f6' },
    'flow_completed':      { label: 'Fluxo concluído',     icon: 'check_circle', color: '#10b981' },
  };

  readonly DocumentStatus = DocumentStatus
  readonly DocumentStatusLabel = DocumentStatusLabel
  readonly DocumentStatusColor = DocumentStatusColor
  readonly formatFileSize = formatFileSize

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private documentService: DocumentService,
    private auditLogService: AuditLogService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private ngZone: NgZone,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.documentId = this.route.snapshot.paramMap.get('id') || '';
    this.currentUserEmail = this.normalizeEmail(this.authService.getUserValue()?.email);

    const action = this.route.snapshot.queryParamMap.get('action');

    setTimeout(() => {
      this.loadDocument(() => {
        if (action === 'sign' && this.canCurrentUserSign) {
          setTimeout(() => this.confirmSign(), 0);
        }
      });
    }, 0);
  }

  loadAuditLogs(): void {
    if (!this.documentId || this.auditLogs.length > 0) return;
    this.loadingLogs = true;
    this.auditLogService.GetByDocument(this.documentId).pipe(
      observeOn(asyncScheduler)
    ).subscribe({
      next: (logs) => {
        this.ngZone.run(() => {
          this.auditLogs = Array.isArray(logs) ? logs : [];
          this.loadingLogs = false;
        });
      },
      error: () => {
        this.ngZone.run(() => {
          this.loadingLogs = false;
          this.snackBar.open('Falha ao carregar o histórico de auditoria', 'Fechar', { duration: 3000 });
        });
      },
    });
  }

  getLogMeta(action: string): { label: string; icon: string; color: string } {
    if (!action) return { label: action ?? 'Ação', icon: 'history', color: '#94a3b8' };
    return this.ACTION_META[action.toLowerCase().replace(/ /g, '_')] ?? { label: action, icon: 'history', color: '#94a3b8' };
  }

  loadDocument(callback?: () => void): void {
    if (!this.documentId) {
      this.snackBar.open('ID de documento inválido', 'Fechar', { duration: 3000 });
      this.router.navigate(['/documents']);
      return;
    }
    this.loading = true;
    this.documentService.getDocumentById(this.documentId).pipe(
      observeOn(asyncScheduler)
    ).subscribe({
      next: (doc) => {
        this.ngZone.run(() => {
          this.document = {
            ...doc,
            signatureFlows: (doc.signatureFlows ?? []).map(flow => ({
              ...flow,
              signers: [...(flow.signers ?? [])]
            }))
          };
          this.updateSignatureStats();
          // Defer final state flip to avoid dev-mode NG0100 on first render/hydration.
          setTimeout(() => {
            this.loading = false;
            callback?.();
          }, 0);
        });
      },
      error: () => {
        this.ngZone.run(() => {
          setTimeout(() => {
            this.loading = false;
            this.snackBar.open('Falha ao carregar os detalhes do documento', 'Fechar', { duration: 3000 });
            this.router.navigate(['/documents']);
          }, 0);
        });
      },
    });
  }

  ngAfterContentChecked(): void {
    this.cdr.detectChanges();
  }

  onTabChange(event: MatTabChangeEvent): void {
    const historyTabIndex = 2;
    const selectedTabIndex = event.index;
    if (selectedTabIndex === historyTabIndex) {
      this.loadAuditLogs();
    }
  }

  get canSign(): boolean {
    return (
      this.document.status === DocumentStatus.PendingSignatures ||
      this.document.status === DocumentStatus.PartiallyCompleted
    )
  }

  get canDelete(): boolean {
    return (
      this.document.status === DocumentStatus.Draft ||
      this.document.status === DocumentStatus.Cancelled
    )
  }

  get canCurrentUserSign(): boolean {
    return !!this.getCurrentUserPendingSigner();
  }

  get signBlockReason(): string | null {
    if (!this.canSign) {
      return null;
    }

    if (!this.currentUserEmail) {
      return 'Não foi possível identificar o usuário logado para assinatura.';
    }

    if (this.canCurrentUserSign) {
      return null;
    }

    return 'Este documento está aguardando assinatura de outro signatário nesta etapa.';
  }

  private updateSignatureStats(): void {
    const flows = this.document?.signatureFlows ?? [];

    const total = flows.reduce((acc, flow) => {
      const signers = Array.isArray(flow.signers) ? flow.signers : [];
      return acc + signers.length;
    }, 0);

    const signed = flows.reduce((acc, flow) => {
      const signers = Array.isArray(flow.signers) ? flow.signers : [];
      return acc + signers.filter(s => !!s.signedAt).length;
    }, 0);

    this.totalSignatories = Math.max(total, signed);
    this.signedCount = signed;
    this.signatureProcess = this.totalSignatories
      ? Math.min(100, Math.round((this.signedCount / this.totalSignatories) * 100))
      : 0;
  }

  download(): void {
    if (!this.document.id) return;
    this.documentService
      .downloadDocument(this.document.id, this.document.originalFileName)
      .subscribe({
        next: (blob) => {
          this.documentService.triggerDownload(blob, this.document?.originalFileName ?? 'documento');
        },
        error: () => {
          this.snackBar.open('Falha ao baixar o documento', 'Fechar', { duration: 3000 });
        }
      })
  }

  confirmSign(): void {
    if (!this.document.id || !this.canSign) return;
    const pendingSigner = this.getCurrentUserPendingSigner();

    if (!pendingSigner) {
      this.snackBar.open(this.signBlockReason ?? 'Você não possui pendência de assinatura neste documento.', 'Fechar', { duration: 4000 });
      return;
    }

    if (!confirm('Tem certeza que deseja assinar este documento?')) return;

    this.actionLoading = true;

    const signPayload = {
      signerId: pendingSigner.id,
      signatureType: 1,
      certificateData: [],
      pin: '',
      deviceInfo: typeof navigator !== 'undefined' ? navigator.userAgent : 'Unknown',
      location: 'WebApp'
    };

    this.documentService.signDocument(signPayload).subscribe({
      next: () => {
        this.actionLoading = false;
        this.snackBar.open('Documento assinado com sucesso', 'Fechar', { duration: 3000 });
        this.loadDocument();
      },
      error: () => {
        this.actionLoading = false;
        this.snackBar.open('Falha ao assinar o documento', 'Fechar', { duration: 3000 });
      }
    })
  }

  private getCurrentUserPendingSigner(): { id: string } | null {
    if (!this.currentUserEmail) {
      return null;
    }

    for (const flow of this.document.signatureFlows ?? []) {
      const currentStep = Number(flow.currentStep ?? 0);
      const signers = Array.isArray(flow.signers) ? flow.signers : [];

      const candidates = signers.filter((signer) => {
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

      if (candidates.length > 0) {
        return { id: candidates[0].id };
      }
    }

    return null;
  }

  private normalizeEmail(email?: string | null): string | null {
    if (!email) {
      return null;
    }
    return email.trim().toLowerCase();
  }

  confirmDelete(): void {
    if (!this.document.id || !this.canDelete) return;
    if (!confirm('Tem certeza que deseja excluir este documento? Esta ação não pode ser desfeita.')) return;

    this.actionLoading = true;
    this.documentService.deleteDocument(this.document.id).subscribe({
      next: () => {
        this.snackBar.open('Documento excluído com sucesso', 'Fechar', { duration: 3000 });
        this.router.navigate(['/documents']);
      },
      error: () => {
        this.actionLoading = false;
        this.snackBar.open('Falha ao excluir o documento', 'Fechar', { duration: 3000 });
      }
    })
  }

  getFileIcon(): string {
    const ext = this.document?.fileExtension?.toLowerCase();
    const map: Record<string, string> = {
      '.pdf': 'picture_as_pdf',
      '.doc': 'description',
      '.docx': 'description',
      '.txt': 'article',
    };
    return map[ext ?? ''] ?? 'insert_drive_file';
  }

  getFileIconColor(): string {
    const ext = this.document?.fileExtension?.toLowerCase();
    const map: Record<string, string> = {
      '.pdf': '#e53935',
      '.doc': '#1565c0',
      '.docx': '#1565c0',
      '.txt': '#757575',
    };
    return map[ext ?? ''] ?? '#94a3b8';
  }

  getSignatoryInitials(name?: string): string {
    if (!name) {
      return '??';
    }

    return name
      .split(' ')
      .slice(0, 2)
      .map((n) => n.charAt(0).toUpperCase())
      .join('');
  }

  goBack(): void {
    this.router.navigate(['/documents']);
  }

  private createEmptyDocument(): DocumentDto {
    return {
      id: '',
      fileName: '',
      originalFileName: '',
      fileExtension: '',
      fileSizeInBytes: 0,
      mimeType: '',
      status: DocumentStatus.Draft,
      title: '',
      description: '',
      createdByUserId: '',
      createdAt: '',
      updatedAt: '',
      signatureFlows: []
    };
  }
}
