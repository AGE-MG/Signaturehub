import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { TransferDocumentDepartmentDto } from '../../../core/models/document.model';
import { UserDto } from '../../../core/models/signer.model';
import { DocumentService } from '../../../core/services/document.service';
import { UserManagementService } from '../../../core/services/signer.service';

type TransferDialogData = {
  documentId: string;
  documentTitle: string;
  participantEmails: string[];
  currentUserId?: string | null;
};

@Component({
  selector: 'app-transfer-department-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
  ],
  templateUrl: './transfer-department-dialog.component.html',
  styleUrl: './transfer-department-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TransferDepartmentDialogComponent implements OnInit {
  readonly form: FormGroup;
  loading = false;
  usersLoading = false;
  eligibleUsers: UserDto[] = [];

  constructor(
    public dialogRef: MatDialogRef<TransferDepartmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: TransferDialogData,
    private fb: FormBuilder,
    private userService: UserManagementService,
    private documentService: DocumentService,
    private snackBar: MatSnackBar,
    private cdr: ChangeDetectorRef,
  ) {
    this.form = this.fb.group({
      targetUserId: ['', Validators.required],
      reason: ['', [Validators.required, Validators.maxLength(1000)]],
    });
  }

  ngOnInit(): void {
    this.loadEligibleUsers();
  }

  cancel(): void {
    this.dialogRef.close();
  }

  confirm(): void {
    if (this.form.invalid || !this.data.documentId) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    const payload: TransferDocumentDepartmentDto = {
      targetUserId: this.form.value.targetUserId,
      reason: String(this.form.value.reason ?? '').trim(),
    };

    this.documentService.transferDepartment(this.data.documentId, payload).subscribe({
      next: (updatedDocument) => {
        this.loading = false;
        this.dialogRef.close(updatedDocument);
      },
      error: (err) => {
        this.loading = false;
        this.snackBar.open(err?.error?.message || 'Falha ao movimentar documento entre departamentos.', 'Fechar', { duration: 5000 });
        this.cdr.markForCheck();
      }
    });
  }

  getSelectedUserDepartment(): string {
    const selectedId = this.form.get('targetUserId')?.value;
    const selected = this.eligibleUsers.find((user) => user.id === selectedId);
    return selected?.department || 'Departamento não informado';
  }

  private loadEligibleUsers(): void {
    const participantEmails = new Set(
      (this.data.participantEmails || [])
        .map((email) => email?.trim().toLowerCase())
        .filter((email): email is string => !!email)
    );

    this.usersLoading = true;
    this.userService.listAll().subscribe({
      next: (users) => {
        this.eligibleUsers = (users || [])
          .filter((user) => user.id !== this.data.currentUserId)
          .filter((user) => participantEmails.has((user.email || '').trim().toLowerCase()))
          .sort((a, b) => a.fullName.localeCompare(b.fullName, 'pt-BR'));

        this.usersLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.usersLoading = false;
        this.snackBar.open('Falha ao carregar participantes elegíveis para movimentação.', 'Fechar', { duration: 5000 });
        this.cdr.markForCheck();
      }
    });
  }
}
