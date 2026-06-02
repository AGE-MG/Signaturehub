import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { RejectRequest, SignerDto } from '../../../core/models/signer.model';
import { SignerService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-reject-dialog.component',
  imports: [],
  templateUrl: './reject-dialog.component.html',
  styleUrl: './reject-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RejectDialogComponent {
  loading = false
  form!: FormGroup

  constructor(
    public dialogRef: MatDialogRef<RejectDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { signer: SignerDto },
    private fb: FormBuilder,
    private signerService: SignerService,
    private snackbar: MatSnackBar
  ) {
      this.form = this.fb.group({
        reason: ['']
      })
  }

  confirm(): void {
    if (this.form.invalid) return;
    this.loading = true;

    const payload: RejectRequest = {
      signerId: this.data.signer.id,
      reason: this.form.value.reason || undefined,
    }
    this.signerService.reject(payload).subscribe({
      next: (res) => {
        this.loading = false;
        this.dialogRef.close(res);
      },
      error: (err) => {
        this.loading = false;
        this.snackbar.open(err?.error?.message || 'Failed to reject signature', 'Close', { duration: 5000 });
      }
    })
  }
}
