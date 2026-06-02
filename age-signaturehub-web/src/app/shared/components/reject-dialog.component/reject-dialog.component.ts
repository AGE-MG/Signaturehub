import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-reject-dialog.component',
  imports: [],
  template: `<p>reject-dialog.component works!</p>`,
  styleUrl: './reject-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RejectDialogComponent {}
