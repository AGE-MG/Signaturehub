import { ChangeDetectionStrategy, ChangeDetectorRef, Component, DestroyRef, OnInit, inject, SecurityContext } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { DocumentDto, DocumentStatusColor, DocumentStatusLabel, formatFileSize, SignatoryDto } from '../../../../core/models/document.model';
import { DocumentStatus } from '../../../../core/models/dasboard.model';
import { ActivatedRoute, Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import {MatTabChangeEvent, MatTabsModule} from "@angular/material/tabs"
import { AuditLogDto } from '../../../../core/models/signer.model';
import { AuditLogService, SignatureFlowService } from '../../../../core/services/signer.service';
import { AuthService } from '../../../../core/services/auth.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CreateSignatureFlowDto, SignerDto, SignerRole, SignatureStatus, SignatureType } from '../../../../core/models/signer.model';
import { DocumentDetailsHeaderComponent } from './components/document-details-header/document-details-header.component';
import { DocumentDetailsTitleCardComponent } from './components/document-details-title-card/document-details-title-card.component';
import { DocumentDetailsInfoTabComponent } from './components/document-details-info-tab/document-details-info-tab.component';
import { DocumentDetailsSignaturesTabComponent } from './components/document-details-signatures-tab/document-details-signatures-tab.component';
import { DocumentDetailsHistoryTabComponent } from './components/document-details-history-tab/document-details-history-tab.component';
import { MatDialog } from '@angular/material/dialog';
import { SignDialogComponent } from '../../../../shared/components/sign-dialog.component/sign-dialog.component';
import { RejectDialogComponent } from '../../../../shared/components/reject-dialog.component/reject-dialog.component';
import { TransferDepartmentDialogComponent } from '../../../../shared/components/transfer-department-dialog.component/transfer-department-dialog.component';

@Component({
  selector: 'app-document-details',
  imports: [
    MatProgressSpinner,
    MatIconModule,
    MatTabsModule,
    FormsModule,
    DocumentDetailsHeaderComponent,
    DocumentDetailsTitleCardComponent,
    DocumentDetailsInfoTabComponent,
    DocumentDetailsSignaturesTabComponent,
    DocumentDetailsHistoryTabComponent,
  ],
  templateUrl: './document-details.component.html',
  styleUrls: ['./document-details.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsComponent implements OnInit {
  document: DocumentDto = this.createEmptyDocument();
  auditLogs: AuditLogDto[] = [];
  loadingLogs = false;
  loading = true;
  actionLoading = false;
  documentId = '';
  currentUserEmail: string | null = null;
  currentUserPendingSignerId: string | null = null;
  canCurrentUserSign = false;
  signBlockReason: string | null = null;
  totalSignatories = 0;
  signedCount = 0;
  signatureProcess = 0;
  flowFormOpen = false;
  flowFormMode: 'start' | 'transfer' = 'start';
  flowFormName = '';
  flowFormEmail = '';
  flowFormDocument = '';
  previewUrl: SafeResourceUrl | null = null;
  previewText: string | null = null;
  previewType: 'pdf' | 'text' | 'unsupported' | null = null;
  previewLoading = false;

  private readonly FLOW_TYPE_SEQUENTIAL = 1;
  private readonly FLOW_TYPE_PARALLEL = 2;

  readonly actionMeta: Record<string, { label: string; icon: string; color: string }> = {
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
    'document_department_transferred': { label: 'Departamento movido', icon: 'swap_horiz', color: '#2563eb' },
  };

  readonly DocumentStatus = DocumentStatus
  readonly DocumentStatusLabel = DocumentStatusLabel
  readonly DocumentStatusColor = DocumentStatusColor
  readonly formatFileSize = formatFileSize
  private readonly destroyRef = inject(DestroyRef);

  constructor(
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer,
    private router: Router,
    private route: ActivatedRoute,
    private documentService: DocumentService,
    private auditLogService: AuditLogService,
    private signatureFlowService: SignatureFlowService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) {}

  ngOnInit(): void {
    this.destroyRef.onDestroy(() => this.clearPreview());
    this.documentId = this.route.snapshot.paramMap.get('id') || '';
    this.currentUserEmail = this.resolveCurrentUserEmail();

    const action = this.route.snapshot.queryParamMap.get('action');

    this.loadDocument(() => {
      if (action === 'sign' && this.canCurrentUserSign) {
        setTimeout(() => this.confirmSign(), 0);
      }
    });
  }

  loadAuditLogs(): void {
    if (!this.documentId || this.auditLogs.length > 0) return;
    this.loadingLogs = true;
    this.auditLogService.GetByDocument(this.documentId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (logs) => {
        this.auditLogs = Array.isArray(logs) ? logs : [];
        this.loadingLogs = false;
      },
      error: () => {
        this.loadingLogs = false;
        this.snackBar.open('Falha ao carregar o histórico de auditoria', 'Fechar', { duration: 3000 });
      },
    });
  }

  loadDocument(callback?: () => void): void {
    if (!this.documentId) {
      this.snackBar.open('ID de documento inválido', 'Fechar', { duration: 3000 });
      this.loading = false;
      this.cdr.markForCheck();
      return;
    }
    this.loading = true;
    this.documentService.getDocumentById(this.documentId).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (doc) => {
        try {
          const signatureFlows = Array.isArray(doc?.signatureFlows)
            ? doc.signatureFlows
            : [];

          this.document = {
            ...doc,
            signatureFlows: signatureFlows.map((flow) => ({
              ...flow,
              signers: Array.isArray(flow?.signers)
                ? flow.signers.filter((signer): signer is NonNullable<typeof signer> => !!signer)
                : []
            }))
          };

          this.refreshSignState();
          this.updateSignatureStats();
          this.loadPreview();
          this.loading = false;
          this.cdr.markForCheck();

          if (callback) {
            setTimeout(() => callback(), 0);
          }
        } catch (error) {
          console.error('Erro ao processar detalhes do documento:', error);
          this.snackBar.open('Falha ao processar os detalhes do documento', 'Fechar', { duration: 3000 });
          this.loading = false;
          this.cdr.markForCheck();
        }
      },
      error: (err) => {
        console.error('Falha ao carregar documento:', err);
        this.loading = false;
        this.cdr.markForCheck();
        this.snackBar.open('Falha ao carregar os detalhes do documento', 'Fechar', { duration: 3000 });
      },
    });
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
      this.document.status === DocumentStatus.PartiallyCompleted ||
      !!this.currentUserPendingSignerId
    )
  }

  get canReject(): boolean {
    return this.canSign;
  }

  get canDelete(): boolean {
    return (
      this.document.status === DocumentStatus.Draft ||
      this.document.status === DocumentStatus.Cancelled
    )
  }

  get canStartFlow(): boolean {
    return !!this.document.id && this.document.status === DocumentStatus.Draft;
  }

  get canTransferResponsibility(): boolean {
    if (!this.document.id || !this.currentUserEmail) {
      return false;
    }

    return (this.document.signatureFlows ?? []).some((flow) => {
      const signers = (flow.signers ?? []).filter((s): s is NonNullable<typeof s> => !!s);
      const bySignOrder = new Map<number, NonNullable<typeof signers[number]>[]>();
      for (const signer of signers) {
        const order = Number(signer.signOrder ?? 0);
        const group = bySignOrder.get(order) ?? [];
        group.push(signer);
        bySignOrder.set(order, group);
      }

      for (const group of bySignOrder.values()) {
        const current = this.resolveCurrentResponsible(group);
        if (
          current &&
          Number(current.status ?? 0) === SignatureStatus.Signed &&
          this.normalizeEmail(current.email) === this.currentUserEmail
        ) {
          return true;
        }
      }
      return false;
    });
  }

  /**
   * Dentro de um mesmo passo (flow + signOrder), uma transferência adiciona um novo
   * signatário sem apagar o histórico do anterior. O "responsável atual" é sempre o
   * mais recente do grupo — só ele pode ver o botão de transferir de novo, e só se já
   * tiver assinado (uma transferência para outra pessoa ainda pendente esconde o botão).
   */
  private resolveCurrentResponsible(signers: SignatoryDto[]): SignatoryDto | null {
    if (signers.length === 0) {
      return null;
    }

    return signers.reduce((latest, signer) => {
      const latestTime = latest.createdAt ? new Date(latest.createdAt).getTime() : 0;
      const signerTime = signer.createdAt ? new Date(signer.createdAt).getTime() : 0;
      return signerTime >= latestTime ? signer : latest;
    });
  }

  private computeSignBlockReason(): string | null {
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

  private refreshSignState(): void {
    this.currentUserPendingSignerId = this.computeCurrentUserPendingSignerId();
    this.canCurrentUserSign = !!this.currentUserPendingSignerId;
    this.signBlockReason = this.computeSignBlockReason();
  }

  private updateSignatureStats(): void {
    const flows = this.document?.signatureFlows ?? [];

    const total = flows.reduce((acc, flow) => {
      const signers = Array.isArray(flow.signers) ? flow.signers.filter((s): s is NonNullable<typeof s> => !!s) : [];
      return acc + signers.length;
    }, 0);

    const signed = flows.reduce((acc, flow) => {
      const signers = Array.isArray(flow.signers) ? flow.signers.filter((s): s is NonNullable<typeof s> => !!s) : [];
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

  get canTransferDepartment(): boolean {
    if (!this.document.id || !this.currentUserEmail) {
      return false;
    }

    return (this.document.signatureFlows ?? []).some((flow) =>
      (flow.signers ?? []).some((signer) =>
        this.normalizeEmail(signer.email) === this.currentUserEmail
      )
    );
  }

  get canPreview(): boolean {
    return this.previewType === 'pdf' || this.previewType === 'text';
  }

  confirmSign(): void {
    if (!this.document.id || !this.canSign) return;
    this.refreshSignState();

    if (!this.currentUserPendingSignerId) {
      this.snackBar.open(this.signBlockReason ?? 'Você não possui pendência de assinatura neste documento.', 'Fechar', { duration: 4000 });
      return;
    }

    const signerForDialog: SignerDto = {
      id: this.currentUserPendingSignerId,
      name: this.authService.getUserValue()?.fullName ?? 'Assinante',
      email: this.currentUserEmail ?? '',
      document: this.document.title,
      role: SignerRole.Signer,
      signOrder: 1,
      status: SignatureStatus.Pending,
      signatureType: SignatureType.Electronic,
    };

    this.dialog.open(SignDialogComponent, {
      data: { signer: signerForDialog },
      width: '560px'
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.snackBar.open('Documento assinado com sucesso', 'Fechar', { duration: 3000 });
        this.loadDocument();
      }
    });
  }

  confirmReject(): void {
    if (!this.document.id || !this.canReject) return;
    this.refreshSignState();

    if (!this.currentUserPendingSignerId) {
      this.snackBar.open(this.signBlockReason ?? 'Você não possui pendência de assinatura neste documento.', 'Fechar', { duration: 4000 });
      return;
    }

    const signerForDialog: SignerDto = {
      id: this.currentUserPendingSignerId,
      name: this.authService.getUserValue()?.fullName ?? 'Assinante',
      email: this.currentUserEmail ?? '',
      document: this.document.title,
      role: SignerRole.Signer,
      signOrder: 1,
      status: SignatureStatus.Pending,
      signatureType: SignatureType.Electronic,
    };

    this.dialog.open(RejectDialogComponent, {
      data: { signer: signerForDialog },
      width: '560px',
      maxWidth: 'calc(100vw - 32px)',
      autoFocus: false,
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.snackBar.open('Documento rejeitado com sucesso', 'Fechar', { duration: 3000 });
        this.loadDocument();
      }
    });
  }

  private computeCurrentUserPendingSignerId(): string | null {
    if (!this.currentUserEmail) {
      return null;
    }

    for (const flow of this.document.signatureFlows ?? []) {
      const currentStep = Number(flow.currentStep ?? 0);
      const signers = Array.isArray(flow.signers) ? flow.signers.filter((s): s is NonNullable<typeof s> => !!s) : [];

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
        return candidates[0].id;
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

  private resolveCurrentUserEmail(): string | null {
    const serviceUser = (this.authService as { getUserValue?: () => { email?: string | null } | null })
      .getUserValue?.();

    const serviceEmail = this.normalizeEmail(serviceUser?.email);
    if (serviceEmail) {
      return serviceEmail;
    }

    try {
      const raw = localStorage.getItem('currentUser');
      if (!raw) {
        return null;
      }

      const parsed = JSON.parse(raw) as { email?: string | null };
      return this.normalizeEmail(parsed?.email);
    } catch {
      return null;
    }
  }

  private getSafeDeviceInfo(): string {
    if (typeof navigator === 'undefined') {
      return 'WebApp';
    }

    const platform = navigator.platform || 'UnknownPlatform';
    const ua = navigator.userAgent || 'UnknownBrowser';
    const safe = `${platform} | ${ua}`;
    return safe.length <= 100 ? safe : safe.slice(0, 100);
  }

  private getSafeUserAgent(): string {
    if (typeof navigator === 'undefined') {
      return 'WebApp';
    }

    const ua = navigator.userAgent || 'WebApp';
    return ua.length <= 500 ? ua : ua.slice(0, 500);
  }

  private getSafeIpAddress(): string {
    return '0.0.0.0';
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

  startSignatureFlow(): void {
    if (!this.document.id || !this.canStartFlow) {
      return;
    }

    this.openFlowForm('start');
  }

  transferResponsibility(): void {
    if (!this.document.id || !this.canTransferResponsibility) {
      return;
    }

    this.openFlowForm('transfer');
  }

  get flowFormTitle(): string {
    return this.flowFormMode === 'start'
      ? 'Iniciar Fluxo de Assinatura'
      : 'Transferir Responsabilidade';
  }

  get flowFormDescription(): string {
    return this.flowFormMode === 'start'
      ? 'Informe os dados do primeiro assinante do fluxo.'
      : 'Informe os dados do novo responsável pela assinatura.';
  }

  closeFlowForm(): void {
    this.flowFormOpen = false;
    this.flowFormName = '';
    this.flowFormEmail = '';
    this.flowFormDocument = '';
    this.cdr.markForCheck();
  }

  submitFlowForm(): void {
    const signerName = this.flowFormName.trim();
    const signerEmail = this.flowFormEmail.trim();
    const signerDocument = this.flowFormDocument.trim();

    if (!this.document.id || !signerName || !signerEmail || !signerDocument) {
      this.snackBar.open('Nome, e-mail e CPF/CNPJ são obrigatórios.', 'Fechar', { duration: 4000 });
      return;
    }

    this.actionLoading = true;

    if (this.flowFormMode === 'transfer') {
      this.documentService.transferResponsibility(this.document.id, {
        newResponsibleName: signerName,
        newResponsibleEmail: signerEmail,
        newResponsibleDocument: signerDocument,
      }).subscribe({
        next: () => {
          this.actionLoading = false;
          this.closeFlowForm();
          this.cdr.markForCheck();
          this.snackBar.open('Responsabilidade transferida com sucesso.', 'Fechar', { duration: 3500 });
          this.loadDocument();
        },
        error: (err) => {
          this.actionLoading = false;
          this.cdr.markForCheck();
          const apiMessage = Array.isArray(err?.error)
            ? err.error.join(' | ')
            : err?.error?.message || err?.error?.title || 'Falha ao transferir responsabilidade.';
          this.snackBar.open(apiMessage, 'Fechar', { duration: 5000 });
        }
      });
      return;
    }

    const payload: CreateSignatureFlowDto = {
      documentId: this.document.id,
      flowName: `Fluxo ${this.document.title}`,
      flowType: 1,
      signers: [
        {
          name: signerName,
          email: signerEmail,
          document: signerDocument,
          role: SignerRole.Signer,
          signOrder: 1,
        },
      ],
    };

    this.signatureFlowService.create(payload).subscribe({
      next: () => {
        this.actionLoading = false;
        this.closeFlowForm();
        this.cdr.markForCheck();
        this.snackBar.open('Fluxo de assinatura iniciado com sucesso.', 'Fechar', { duration: 3500 });
        this.loadDocument();
      },
      error: (err) => {
        this.actionLoading = false;
        this.cdr.markForCheck();
        const apiMessage = Array.isArray(err?.error)
          ? err.error.join(' | ')
          : err?.error?.message || err?.error?.title || 'Falha ao salvar fluxo.';
        this.snackBar.open(apiMessage, 'Fechar', { duration: 5000 });
      }
    });
  }

  private openFlowForm(mode: 'start' | 'transfer'): void {
    this.flowFormMode = mode;
    this.flowFormName = '';
    this.flowFormEmail = '';
    this.flowFormDocument = '';
    this.flowFormOpen = true;
    this.cdr.markForCheck();
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

  goBack(): void {
    this.router.navigate(['/documents']);
  }

  transferDepartment(): void {
    if (!this.document.id || !this.canTransferDepartment) {
      return;
    }

    const currentUserId = this.authService.getUserValue()?.id ?? null;
    const participantEmails = Array.from(new Set(
      (this.document.signatureFlows ?? [])
        .flatMap((flow) => flow.signers ?? [])
        .map((signer) => this.normalizeEmail(signer.email))
        .filter((email): email is string => !!email)
    ));

    this.dialog.open(TransferDepartmentDialogComponent, {
      width: '680px',
      maxWidth: 'calc(100vw - 32px)',
      data: {
        documentId: this.document.id,
        documentTitle: this.document.title,
        participantEmails,
        currentUserId,
      }
    }).afterClosed().subscribe((updatedDocument: DocumentDto | undefined) => {
      if (!updatedDocument) {
        return;
      }

      this.document = {
        ...this.document,
        ...updatedDocument,
        signatureFlows: updatedDocument.signatureFlows ?? this.document.signatureFlows,
      };
      this.auditLogs = [];
      this.loadAuditLogs();
      this.snackBar.open('Documento movimentado com sucesso entre departamentos.', 'Fechar', { duration: 3500 });
      this.loadDocument();
      this.cdr.markForCheck();
    });
  }

  private loadPreview(): void {
    this.clearPreview();

    if (!this.document.id) {
      return;
    }

    const ext = this.document.fileExtension?.toLowerCase();
    const isPdf = ext === '.pdf';
    const isText = ext === '.txt';

    if (!isPdf && !isText) {
      this.previewType = 'unsupported';
      return;
    }

    this.previewLoading = true;
    this.documentService.downloadDocument(this.document.id, this.document.originalFileName).pipe(
      takeUntilDestroyed(this.destroyRef)
    ).subscribe({
      next: (blob) => {
        if (isPdf) {
          const objectUrl = URL.createObjectURL(blob);
          this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
          this.previewType = 'pdf';
          this.previewLoading = false;
          this.cdr.markForCheck();
          return;
        }

        blob.text().then((text) => {
          this.previewText = text;
          this.previewType = 'text';
          this.previewLoading = false;
          this.cdr.markForCheck();
        }).catch(() => {
          this.previewType = 'unsupported';
          this.previewLoading = false;
          this.cdr.markForCheck();
        });
      },
      error: () => {
        this.previewType = 'unsupported';
        this.previewLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  private clearPreview(): void {
    if (this.previewUrl) {
      const unsafeUrl = this.sanitizer.sanitize(SecurityContext.RESOURCE_URL, this.previewUrl);
      if (unsafeUrl) {
        URL.revokeObjectURL(unsafeUrl);
      }
    }

    this.previewUrl = null;
    this.previewText = null;
    this.previewType = null;
    this.previewLoading = false;
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
      owningDepartment: '',
      isConfidential: false,
      createdAt: '',
      updatedAt: '',
      signatureFlows: []
    };
  }
}
