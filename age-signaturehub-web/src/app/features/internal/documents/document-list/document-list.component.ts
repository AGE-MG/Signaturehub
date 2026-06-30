import { ApplicationRef, ChangeDetectorRef, Component, Inject, NgZone, OnInit, PLATFORM_ID, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule,  } from '@angular/material/sort';
import { MatTableDataSource, MatHeaderCell, MatColumnDef, MatTableModule } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { DocumentDto, DocumentStatus, DocumentStatusColor, DocumentStatusLabel, DocumentStatusMatColor, formatFileSize } from '../../../../core/models/document.model';
import { Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from "@angular/material/icon";
import { MatAnchor, MatIconButton } from "@angular/material/button";
import { MatFormField, MatLabel } from "@angular/material/form-field";
import { MatSelect, MatOption } from "@angular/material/select";
import { FormsModule } from "@angular/forms";
import { MatChipSet, MatChip } from "@angular/material/chips";
import { MatCard } from "@angular/material/card";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatInput } from '@angular/material/input';
import { A11yModule } from "@angular/cdk/a11y";
import { DatePipe } from '@angular/common';
import { isPlatformBrowser } from '@angular/common';
import { MatMenuModule } from "@angular/material/menu";
import { MatDividerModule } from '@angular/material/divider';
import { asyncScheduler, observeOn } from 'rxjs';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-document-list',
  imports: [
    MatPaginatorModule,
    MatSortModule, MatAnchor,
    MatIconModule, MatFormField,
    MatLabel,
    MatInput,
    MatSelect,
    FormsModule,
    MatOption,
    MatChipSet,
    MatChip,
    MatCard,
    MatProgressSpinner,
    MatHeaderCell,
    MatColumnDef,
    MatTableModule,
    MatSort,
    A11yModule,
    DatePipe,
    MatIconButton,
    MatMenuModule,
    MatDividerModule
  ],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.scss'],
})
export class DocumentListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<DocumentDto>([]);
  displayedColumns = ['icon', 'title', 'status', 'size', 'updatedAt', 'actions'];
  private allDocuments: DocumentDto[] = [];

  loading = true;
  totalCount = 0;
  visibleDocumentsCount = 0;
  pageSize = 10;
  pageIndex = 0;

  searchQuery = '';
  selectedStatus: DocumentStatus | null = null;

  readonly DocumentStatus = DocumentStatus;
  readonly DocumentStatusLabel = DocumentStatusLabel;
  readonly DocumentStatusMatColor = DocumentStatusMatColor;
  readonly DocumentStatusColor = DocumentStatusColor;
  readonly formatFileSize = formatFileSize;

  readonly statusOptions = Object.entries(DocumentStatusLabel).map(([value, label]) => ({ value: Number(value) as DocumentStatus, label }));
  private isBrowser = false;
  private currentUserEmail: string | null = null;

  private readonly FLOW_TYPE_SEQUENTIAL = 1;
  private readonly FLOW_TYPE_PARALLEL = 2;

  constructor(
    private router: Router,
    private documentService: DocumentService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
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
    this.currentUserEmail = this.normalizeEmail(this.authService.getUserValue()?.email);
    setTimeout(() => this.loadDocuments(), 0);
  }

  loadDocuments(): void {
    this.loading = true;
    this.documentService.getDocuments({
      status: this.selectedStatus ?? undefined,
    }).pipe(
      observeOn(asyncScheduler)
    ).subscribe({
      next: (result) => {
        this.ngZone.run(() => {
          if (Array.isArray(result)) {
            this.allDocuments = result;
          } else {
            this.allDocuments = result.items ?? [];
          }
          this.applyClientFiltersAndPaging();
          this.loading = false;
          this.cdr.detectChanges();
          this.appRef.tick();
        });
      },
      error: (error) => {
        this.ngZone.run(() => {
          console.error('Error loading documents', error);
          this.snackBar.open('Erro ao carregar documentos', 'Fechar', { duration: 3000 });
          this.loading = false;
          this.cdr.detectChanges();
          this.appRef.tick();
        });
      }
    })
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyClientFiltersAndPaging();
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.applyClientFiltersAndPaging();
  }

  onStatusFilter(status: DocumentStatus | null): void {
    this.selectedStatus = status;
    this.pageIndex = 0;
    this.loadDocuments();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedStatus = null;
    this.pageIndex = 0;
    this.loadDocuments();
  }

  get hasActiveFilters(): boolean {
    return !!(this.searchQuery || this.selectedStatus !== null);
  }

  private applyClientFiltersAndPaging(): void {
    const query = this.searchQuery.trim().toLowerCase();

    let filtered = this.allDocuments;
    if (query) {
      filtered = filtered.filter(doc =>
        (doc.title ?? '').toLowerCase().includes(query) ||
        (doc.originalFileName ?? '').toLowerCase().includes(query)
      );
    }

    this.totalCount = filtered.length;

    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    this.dataSource.data = filtered.slice(start, end);
    this.visibleDocumentsCount = this.dataSource.data.length;
  }

  // Navigation

  goToUpload(): void {
    this.router.navigate(['/documents/upload']);
  }

  viewDocument(document: DocumentDto): void {
    this.router.navigate(['/documents', document.id]);
  }

  // Actions

  downloadDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    this.documentService.downloadDocument(doc.id, doc.originalFileName).subscribe({
      next: (blob) => {
        this.documentService.triggerDownload(blob, doc.originalFileName);
        this.snackBar.open('Download iniciado', 'Fechar', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error downloading document', error);
        this.snackBar.open('Erro ao baixar documento', 'Fechar', { duration: 3000 });
      }
    });
  }

  deleteDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    if (!confirm(`Tem certeza que deseja excluir o documento "${doc.title}"?`)) {
      return;
    }
    this.documentService.deleteDocument(doc.id).subscribe({
      next: () => {
        this.snackBar.open('Documento excluído com sucesso', 'Fechar', { duration: 3000 });
        this.loadDocuments();
      },
      error: (error) => {
        console.error('Error deleting document', error);
        this.snackBar.open('Erro ao excluir documento', 'Fechar', { duration: 3000 });
      }
    });
  }

  // Helpers

  getFileIcon(ext: string): string {
    const map: Record<string, string> = {
      '.pdf': 'picture_as_pdf',
      '.doc': 'description',
      '.docx': 'description',
      '.xls': 'grid_on',
      '.xlsx': 'grid_on',
      '.ppt': 'slideshow',
      '.pptx': 'slideshow',
      '.txt': 'notes',
    };
    return map[ext?.toLowerCase()] || 'insert_drive_file';
  }

  getFileIconColor(ext: string): string {
    const map: Record<string, string> = {
      '.pdf': '#E53935',
      '.doc': '#1E88E5',
      '.docx': '#1E88E5',
      '.xls': '#43A047',
      '.xlsx': '#43A047',
      '.ppt': '#FB8C00',
      '.pptx': '#FB8C00',
      '.txt': '#757575',
    };
    return map[ext?.toLowerCase()] || '#757575';
  }

  canSign(doc: DocumentDto): boolean {
    return !!this.getCurrentUserPendingSignerId(doc);
  }

  shouldShowTurnIndicator(doc: DocumentDto): boolean {
    return this.isPendingDocument(doc) && !!this.currentUserEmail;
  }

  isMyTurnToSign(doc: DocumentDto): boolean {
    return !!this.getCurrentUserPendingSignerId(doc);
  }

  isAwaitingOtherSigner(doc: DocumentDto): boolean {
    return this.isPendingDocument(doc) && !this.isMyTurnToSign(doc);
  }

  getTurnIndicatorLabel(doc: DocumentDto): string {
    return this.isMyTurnToSign(doc) ? 'Minha vez de assinar' : 'Aguardando outro signatário';
  }

  getTurnIndicatorIcon(doc: DocumentDto): string {
    return this.isMyTurnToSign(doc) ? 'how_to_reg' : 'schedule';
  }

  signDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.canSign(doc)) {
      this.snackBar.open('Este documento está aguardando assinatura de outro signatário.', 'Fechar', { duration: 3500 });
      return;
    }
    this.router.navigate(['/documents', doc.id], { queryParams: { action: 'sign' } });
  }

  private isPendingDocument(doc: DocumentDto): boolean {
    return (
      doc.status === DocumentStatus.PendingSignatures || doc.status === DocumentStatus.PartiallyCompleted
    )
  }

  private getCurrentUserPendingSignerId(doc: DocumentDto): string | null {
    if (!this.currentUserEmail || !this.isPendingDocument(doc)) {
      return null;
    }

    for (const flow of doc.signatureFlows ?? []) {
      const currentStep = Number(flow.currentStep ?? 0);
      const signers = Array.isArray(flow.signers) ? flow.signers : [];

      const candidate = signers.find((signer) => {
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

      if (candidate) {
        return candidate.id;
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

  canDelete(doc: DocumentDto): boolean {
    return (
      doc.status === DocumentStatus.Draft ||
      doc.status === DocumentStatus.Cancelled
    )
  }

  getStatusColor(status: DocumentDto['status']): string {
    return this.DocumentStatusColor[status];
  }

  getStatusLabel(status: DocumentDto['status']): string {
    return this.DocumentStatusLabel[status];
  }

  trackById(_: number, doc: DocumentDto): string {
    return doc.id;
  }
}
