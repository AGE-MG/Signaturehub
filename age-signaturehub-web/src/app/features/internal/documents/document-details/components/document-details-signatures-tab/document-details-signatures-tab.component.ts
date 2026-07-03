import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { SignatureFlowDto } from '../../../../../../core/models/document.model';
import { SignatureStatus, SignatureType, SignatureTypeLabel } from '../../../../../../core/models/signer.model';

@Component({
  selector: 'app-document-details-signatures-tab',
  standalone: true,
  imports: [DatePipe, MatDividerModule, MatIconModule, MatProgressBarModule],
  templateUrl: './document-details-signatures-tab.component.html',
  styleUrls: ['./document-details-signatures-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsSignaturesTabComponent {
  @Input({ required: true }) signatureFlows: SignatureFlowDto[] = [];
  @Input({ required: true }) totalSignatories = 0;
  @Input({ required: true }) signedCount = 0;
  @Input({ required: true }) signatureProcess = 0;

  readonly SignatureStatus = SignatureStatus;

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

  getSignatureTypeLabel(signatureType?: number): string | null {
    if (!signatureType || !(signatureType in SignatureTypeLabel)) {
      return null;
    }

    return SignatureTypeLabel[signatureType as SignatureType];
  }
}
