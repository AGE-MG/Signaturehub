import { Component, OnInit } from '@angular/core';
import { DocumentDto, DocumentStatusColor, DocumentStatusLabel, formatFileSize } from '../../../../core/models/document.model';
import { DocumentStatus } from '../../../../core/models/dasboard.model';
import { ActivatedRoute, Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'app-document-details',
  imports: [],
  templateUrl: './document-details.component.html',
  styleUrls: ['./document-details.component.scss'],
})
export class DocumentDetailsComponent implements OnInit {
  document: DocumentDto | null = null;
  loading = true;
  actionLoading = false;
  documentId = '';

  readonly DocumentStatus = DocumentStatus
  readonly DocumentStatusLabel = DocumentStatusLabel
  readonly DocumentStatusColor = DocumentStatusColor
  readonly formatFileSize = formatFileSize

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private documentService: DocumentService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.documentId = this.route.snapshot.paramMap.get('id') || '';

    const action = this.route.snapshot.queryParamMap.get('action');

    this.loadDocument(() => {
      if (action === 'sign' && this.canSign) {
        this.confirmSign();
      }
    });
  }

  loadDocument(callback?: () => void): void {
    if (!this.documentId) {
      this.snackBar.open('ID de documento inválido', 'Fechar', { duration: 3000 });
      this.router.navigate(['/documents']);
      return;
    }
    this.loading = true;
    this.documentService.getDocumentById(this.documentId).subscribe({
      next: (doc) => {
        this.document = doc;
        this.loading = false;
        callback?.();
      },
      error: () => {
        this.snackBar.open('Falha ao carregar os detalhes do documento', 'Fechar', { duration: 3000 });
        this.router.navigate(['/documents']);
      },
    });
  }

  get canSign(): boolean {
    return (
      !!this.document &&
      (
        this.document.status === DocumentStatus.PendingSignatures ||
        this.document.status === DocumentStatus.PartiallyCompleted
      )
    )
  }

  get canDelete(): boolean {
    return (
      !!this.document &&
      (
        this.document.status === DocumentStatus.Draft ||
        this.document.status === DocumentStatus.Cancelled
      )
    )
  }

  get totalSignatories(): number {
    return (
      this.document?.signatureFlows.reduce(
        (acc, flow) => acc + (flow.signatories.length ?? 0),
        0
      ) ?? 0
    )
  }

  get signedCount(): number {
    return (
      this.document?.signatureFlows.reduce(
        (acc, flow) => acc + (flow.signatories?.filter(s => !!s.signedAt).length ?? 0),
        0
      ) ?? 0
    )
  }

  get signatureProcess(): number {
    if (!this.totalSignatories) return 0;
    return Math.round((this.signedCount / this.totalSignatories) * 100);
  }

  download(): void {
    if (!this.document) return;
    this.documentService
      .downloadDocument(this.document.id, this.document.originalFilename)
      .subscribe({
        next: (blob) => {
          this.documentService.triggerDownload(blob, this.document?.originalFilename ?? 'documento');
        },
        error: () => {
          this.snackBar.open('Falha ao baixar o documento', 'Fechar', { duration: 3000 });
        }
      })
  }

  confirmSign(): void {
    if (!this.document || !this.canSign) return;
    if (!confirm('Tem certeza que deseja assinar este documento?')) return;

    this.actionLoading = true;
  }
}
