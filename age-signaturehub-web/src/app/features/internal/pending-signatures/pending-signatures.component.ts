import { Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { SignatureStatus, SignatureStatusColor, SignatureStatusLabel, SignatureTypeIcon, SignatureTypeLabel, SignerDto, SignerRoleLabel } from '../../../core/models/signer.model';
import { AuthService } from '../../../core/services/auth.service';
import { SignerService } from '../../../core/services/signer.service';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

@Component({
  selector: 'app-pending-signatures',
  imports: [
    MatIconModule,
    MatDividerModule,
    ReactiveFormsModule,
    MatFormFieldModule,
  ],
  templateUrl: './pending-signatures.component.html',
  styleUrl: './pending-signatures.component.scss',
})
export class PendingSignaturesComponent implements OnInit {

  signers: SignerDto[] = [];
  loading = false;

  readonly SignatureStatus = SignatureStatus;
  readonly SignatureStatusLabel = SignatureStatusLabel;
  readonly SignatureStatusColor= SignatureStatusColor;
  readonly SignatureTypeLabel = SignatureTypeLabel;
  readonly SignatureTypeIcon = SignatureTypeIcon;
  readonly SignerRoleLabel = SignerRoleLabel;

  constructor(
    private authService: AuthService,
    private signerService: SignerService,
    private dialog: MatDialog,
    private snackbar: MatSnackBar,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.load()
  }

  load(): void {
    const user = this.authService.getUserValue();
    if (!user?.email) return;
    this.loading = true;
    this.signerService.getPendingByEmail(user.email).subscribe({
      next: (list) => {
        this;getPend
      },
      error: (err) => {
        this.loading = false;
        this.snackbar.open(err?.error?.message || 'Failed to load pending signatures', 'Close', { duration: 5000 });
      }
    })
  }
}
