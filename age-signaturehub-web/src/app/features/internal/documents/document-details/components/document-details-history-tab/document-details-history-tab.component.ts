import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuditLogDto } from '../../../../../../core/models/signer.model';

export type AuditActionMeta = { label: string; icon: string; color: string };

@Component({
  selector: 'app-document-details-history-tab',
  standalone: true,
  imports: [DatePipe, MatIconModule, MatProgressSpinnerModule],
  templateUrl: './document-details-history-tab.component.html',
  styleUrls: ['./document-details-history-tab.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DocumentDetailsHistoryTabComponent {
  @Input({ required: true }) loadingLogs = false;
  @Input({ required: true }) auditLogs: AuditLogDto[] = [];
  @Input({ required: true }) actionMeta: Record<string, AuditActionMeta> = {};

  getLogMeta(action: string): AuditActionMeta {
    if (!action) {
      return { label: 'Ação', icon: 'history', color: '#94a3b8' };
    }

    const key = action.toLowerCase().replace(/ /g, '_');
    return this.actionMeta[key] ?? { label: action, icon: 'history', color: '#94a3b8' };
  }
}
