import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { DocumentDto, DocumentStatus } from '../../../../../../core/models/document.model';

@Component({
  selector: 'app-document-details-title-card',
  standalone: true,
  imports: [MatCardModule, MatIconModule],
  templateUrl: './document-details-title-card.component.html',
  styleUrls: ['./document-details-title-card.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsTitleCardComponent {
  @Input({ required: true }) document!: DocumentDto;
  @Input({ required: true }) fileIcon = 'insert_drive_file';
  @Input({ required: true }) fileIconColor = '#94a3b8';
  @Input({ required: true }) statusLabel: Record<DocumentStatus, string> = {} as Record<DocumentStatus, string>;
  @Input({ required: true }) statusColor: Record<DocumentStatus, string> = {} as Record<DocumentStatus, string>;
}
