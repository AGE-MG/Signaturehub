import { Component, OnInit } from '@angular/core';
import { DocumentDto, DocumentSource, DocumentStatusColor, DocumentStatusLabel, formatFileSize } from '../../../../core/models/document.model';
import { DocumentStatus } from '../../../../core/models/dasboard.model';
import { ActivatedRoute, Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatIconModule } from "@angular/material/icon";
import { MatCardModule } from "@angular/material/card";
import {MatTabsModule} from "@angular/material/tabs"
import { CdkNoDataRow } from "@angular/cdk/table";
import { MatDivider } from "@angular/material/divider";
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-document-details',
  imports: [MatProgressSpinner, MatIconModule, MatCardModule, MatTabsModule, CdkNoDataRow, MatDivider, DatePipe],
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

    this.documentService.signDocument(this.document.id).subscribe({
      next: (updatedDoc) => {
        this.document = updatedDoc;
        this.actionLoading = false;
        this.snackBar.open('Documento assinado com sucesso', 'Fechar', { duration: 3000 });
      },
      error: () => {
        this.actionLoading = false;
        this.snackBar.open('Falha ao assinar o documento', 'Fechar', { duration: 3000 });
      }
    })
  }

  confirmDelete(): void {
    if (!this.document || !this.canDelete) return;
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

  getSourceClass(source?: DocumentSource): string {
    const map: Record<string, string> = {
      [DocumentSource.internal]: 'source-interno',
      [DocumentSource.Tribunus]: 'source-tribunus',
      [DocumentSource.TJMG]: 'source-tjmg',
      [DocumentSource.external]: 'source-externo',
    };
    return map[source ?? ''] ?? 'source-externo';
  }

  getSignatoryInitials(name: string): string {
    return name
      .split(' ')
      .slice(0, 2)
      .map((n) => n.charAt(0).toUpperCase())
      .join('');
  }

  goBack(): void {
    this.router.navigate(['/documents']);
  }
}
