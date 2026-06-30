import { ApplicationRef, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { SignatureStatus, SignatureStatusColor, SignatureStatusLabel, SignatureType, SignatureTypeIcon, SignatureTypeLabel, SignerDto, SignerRoleLabel } from '../../../core/models/signer.model';
import { AuthService } from '../../../core/services/auth.service';
import { SignerService } from '../../../core/services/signer.service';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { SignDialogComponent } from '../../../shared/components/sign-dialog.component/sign-dialog.component';
import { RejectDialogComponent } from '../../../shared/components/reject-dialog.component/reject-dialog.component';
import { MatTooltip } from "@angular/material/tooltip";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatCard } from "@angular/material/card";
import { DatePipe } from '@angular/common';
import { MatButtonModule } from "@angular/material/button";
import { MatExpansionPanel, MatExpansionPanelHeader, MatExpansionPanelTitle } from '@angular/material/expansion';
import { A11yModule } from "@angular/cdk/a11y";
import { asyncScheduler, observeOn } from 'rxjs';

@Component({
  selector: 'app-pending-signatures',
  imports: [
    MatIconModule,
    MatDividerModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatTooltip,
    MatProgressSpinner,
    MatCard,
    DatePipe,
    MatButtonModule,
    MatExpansionPanel,
    MatExpansionPanelHeader,
    MatExpansionPanelTitle,
    A11yModule
],
  templateUrl: './pending-signatures.component.html',
  styleUrl: './pending-signatures.component.scss',
})
export class PendingSignaturesComponent implements OnInit {

  signers: SignerDto[] = [];
  loading = false;
  pendingCount = 0;

  readonly SignatureStatus = SignatureStatus;
  readonly SignatureStatusLabel = SignatureStatusLabel;
  readonly SignatureStatusColor= SignatureStatusColor;
  readonly SignatureType = SignatureType;
  readonly SignatureTypeLabel = SignatureTypeLabel;
  readonly SignatureTypeIcon = SignatureTypeIcon;
  readonly SignerRoleLabel = SignerRoleLabel;

  constructor(
    private authService: AuthService,
    private signerService: SignerService,
    private dialog: MatDialog,
    private snackbar: MatSnackBar,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private appRef: ApplicationRef
  ) { }

  ngOnInit(): void {
    setTimeout(() => this.load(), 0);
  }

  load(): void {
    const user = this.authService.getUserValue();
    if (!user?.email) return;
    this.loading = true;
    this.signerService.getPendingByEmail(user.email).pipe(
      observeOn(asyncScheduler)
    ).subscribe({
      next: (list) => {
        const safeList = Array.isArray(list) ? list : [];
        this.signers = safeList.map(s => ({
          ...s,
          signOrder: Math.max(0, s.signOrder ?? 0)
        }));
        this.pendingCount = this.signers.filter(s => s.status === SignatureStatus.Pending).length;
        this.loading = false;
        this.cdr.detectChanges();
        this.appRef.tick();
      },
      error: (err) => {
        this.signers = [];
        this.pendingCount = 0;
        this.loading = false;
        this.snackbar.open(err?.error?.message || 'Falhou em carregar as assinaturas pendentes', 'Fechar', { duration: 5000 });
        this.cdr.detectChanges();
        this.appRef.tick();
      }
    })
  }

  openSignDialog(signer: SignerDto): void {
    this.dialog.open(SignDialogComponent, {
      data: { signer },
      width: '520px'
    }).afterClosed().subscribe(result => {
      if (result) {
        this.snackbar.open('Documento assinado com sucesso!', 'Fechar', { duration: 3000 });
        this.load();
      }
    })
  }

  openRejectDialog(signer: SignerDto): void {
    const dialogRef = this.dialog.open(RejectDialogComponent, {
      data: { signer, reject: true },
      width: '520px'
    }).afterClosed().subscribe(result => {
      if (result) {
        this.snackbar.open('Documento rejeitado com sucesso!', 'Fechar', { duration: 3000 });
        this.load();
      }
    })
  }

  viewDocument(signer: SignerDto): void {
    const uuidRegex = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
    if (signer.document && uuidRegex.test(signer.document)) {
      this.router.navigate(['/documents', signer.document]);
    }
  }

  isPending(signer: SignerDto): boolean {
    return signer.status === SignatureStatus.Pending;
  }

  getInitials(name?: string): string {
    if (!name) return '?';
    return name.split(' ').slice(0, 2).map(n => n[0].toUpperCase()).join('');
  }

  getSignatureTypeIcon(signer: SignerDto): string {
    const type = signer.signatureType ?? SignatureType.Electronic;
    return this.SignatureTypeIcon[type];
  }

  getSignatureTypeLabel(signer: SignerDto): string {
    const type = signer.signatureType ?? SignatureType.Electronic;
    return this.SignatureTypeLabel[type];
  }
}
