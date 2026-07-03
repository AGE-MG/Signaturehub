import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { SignatureType, SignatureTypeIcon, SignatureTypeLabel, SignerDto, SignRequest } from '../../../core/models/signer.model';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { SignerService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-sign-dialog.component',
  imports: [
    ReactiveFormsModule,
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDividerModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './sign-dialog.component.html',
  styleUrl: './sign-dialog.component.scss',
})
export class SignDialogComponent {
  loading = false
  form!: FormGroup
  certificateFileName: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<SignDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { signer: SignerDto },
    private fb: FormBuilder,
    private signerService: SignerService,
    private snackbar: MatSnackBar
  ) {
    this.form = this.fb.group({
      signatureType: [this.data.signer.signatureType ?? SignatureType.Electronic, Validators.required],
      certificateData: [null],
      pin: [''],
    });

    this.updateCertificateValidators(this.form.get('signatureType')?.value as SignatureType);
    this.form.get('signatureType')?.valueChanges.subscribe((value) => {
      this.updateCertificateValidators(value as SignatureType);
    });
  }

  readonly signatureTypeOptions = [
    SignatureType.Electronic,
    SignatureType.DigitalA1,
    SignatureType.DigitalA3,
  ].map((v) => ({
    value: v as SignatureType,
    label: SignatureTypeLabel[v as SignatureType],
    icon: SignatureTypeIcon[v as SignatureType],
  }))

  get requiresCertificate(): boolean {
    const t = this.form.get('signatureType')?.value;
    return t === SignatureType.DigitalA1 || t === SignatureType.DigitalA3;
  }

  async onCertificateSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      this.certificateFileName = null;
      this.form.patchValue({ certificateData: null });
      return;
    }

    const buffer = await file.arrayBuffer();
    const bytes = Array.from(new Uint8Array(buffer));
    this.certificateFileName = file.name;
    this.form.patchValue({ certificateData: bytes });
    this.form.get('certificateData')?.markAsDirty();
    this.form.get('certificateData')?.updateValueAndValidity();
  }

  private updateCertificateValidators(signatureType: SignatureType): void {
    const certificateControl = this.form.get('certificateData');
    const pinControl = this.form.get('pin');
    const requiresCertificate = signatureType === SignatureType.DigitalA1 || signatureType === SignatureType.DigitalA3;

    if (requiresCertificate) {
      certificateControl?.setValidators([Validators.required]);
      pinControl?.setValidators([Validators.required]);
    } else {
      certificateControl?.clearValidators();
      pinControl?.clearValidators();
      this.certificateFileName = null;
      this.form.patchValue({ certificateData: null, pin: '' }, { emitEvent: false });
    }

    certificateControl?.updateValueAndValidity({ emitEvent: false });
    pinControl?.updateValueAndValidity({ emitEvent: false });
  }

  confirm(): void {
    if (this.form.invalid) return;

    this.loading = true;
    const meta = this.signerService.buildDeviceMeta();
    const payload: SignRequest = {
      signerId: this.data.signer.id,
      signatureType: this.form.value.signatureType!,
      certificateData: this.form.value.certificateData || undefined,
      pin: this.form.value.pin || undefined,
      ...meta,
    }
    this.signerService.sign(payload).subscribe({
      next: (res) => {
        this.loading = false;
        this.dialogRef.close(res);
      },
      error: (err) => {
        this.loading = false;
        this.snackbar.open(err?.error?.message || 'Erro ao processar assinatura', 'Fechar', { duration: 5000 });
      }
    })
  }
}
