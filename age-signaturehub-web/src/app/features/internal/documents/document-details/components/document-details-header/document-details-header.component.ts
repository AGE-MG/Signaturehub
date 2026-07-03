import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';

@Component({
  selector: 'app-document-details-header',
  standalone: true,
  imports: [MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatTooltipModule],
  templateUrl: './document-details-header.component.html',
  styleUrls: ['./document-details-header.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsHeaderComponent {
  @Input() title = '';
  @Input() canSign = false;
  @Input() canDelete = false;
  @Input() canStartFlow = false;
  @Input() canTransferResponsibility = false;
  @Input() canReject = false;
  @Input() actionLoading = false;
  @Input() canCurrentUserSign = false;
  @Input() signBlockReason: string | null = null;

  @Output() backClicked = new EventEmitter<void>();
  @Output() downloadClicked = new EventEmitter<void>();
  @Output() signClicked = new EventEmitter<void>();
  @Output() rejectClicked = new EventEmitter<void>();
  @Output() deleteClicked = new EventEmitter<void>();
  @Output() startFlowClicked = new EventEmitter<void>();
  @Output() transferResponsibilityClicked = new EventEmitter<void>();
}
