import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { DocumentDto, DocumentStatus } from '../../../../../../core/models/document.model';

@Component({
  selector: 'app-document-details-info-tab',
  standalone: true,
  imports: [DatePipe, MatDividerModule],
  templateUrl: './document-details-info-tab.component.html',
  styleUrls: ['./document-details-info-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsInfoTabComponent {
  @Input({ required: true }) document!: DocumentDto;
  @Input({ required: true }) statusLabel: Record<DocumentStatus, string> = {} as Record<DocumentStatus, string>;
  @Input({ required: true }) statusColor: Record<DocumentStatus, string> = {} as Record<DocumentStatus, string>;
  @Input({ required: true }) formatFileSize!: (bytes: number) => string;
}
