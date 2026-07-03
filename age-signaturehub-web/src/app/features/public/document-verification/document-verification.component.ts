import { CommonModule, DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { PublicVerificationService } from '../../../core/services/public-verification.service';
import { PublicDocumentVerification } from '../../../core/models/public-verification.model';
import { DocumentStatus, DocumentStatusColor, DocumentStatusLabel, formatFileSize } from '../../../core/models/document.model';
import { SignatureType, SignatureTypeLabel } from '../../../core/models/signer.model';

@Component({
  selector: 'app-document-verification',
  imports: [
    CommonModule,
    RouterModule,
    DatePipe,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './document-verification.component.html',
  styleUrl: './document-verification.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentVerificationComponent implements OnInit {
  loading = true;
  error = false;
  documentVerification: PublicDocumentVerification | null = null;

  readonly statusLabel = DocumentStatusLabel;
  readonly statusColor = DocumentStatusColor;
  readonly signatureTypeLabel = SignatureTypeLabel;

  constructor(
    private route: ActivatedRoute,
    private publicVerificationService: PublicVerificationService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit(): void {
    const documentId = this.route.snapshot.paramMap.get('id');
    const versionParam = this.route.snapshot.queryParamMap.get('version');
    const version = versionParam ? Number(versionParam) : null;

    if (!documentId) {
      this.error = true;
      this.loading = false;
      this.cdr.markForCheck();
      return;
    }

    this.publicVerificationService.getDocumentVerification(documentId, version).subscribe({
      next: (verification) => {
        this.documentVerification = verification;
        this.loading = false;
        this.error = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = true;
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  getSignatureTypeLabel(type?: SignatureType): string {
    if (!type) {
      return 'Não informado';
    }

    return this.signatureTypeLabel[type];
  }

  getStatusChipColor(status: DocumentStatus): string {
    return this.statusColor[status] ?? '#64748b';
  }
}
