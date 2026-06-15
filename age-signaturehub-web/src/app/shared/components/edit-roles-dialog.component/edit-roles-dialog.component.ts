import { ChangeDetectionStrategy, Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UserDto } from '../../../core/models/signer.model';
import { UserManagementService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { CommonModule } from '@angular/common';
import { MatIcon } from "@angular/material/icon";
import { MatDivider } from "@angular/material/divider";
import { MatFormField, MatLabel } from "@angular/material/form-field";
import { MatSelect } from "@angular/material/select";

@Component({
  selector: 'app-edit-roles-dialog',
  imports: [
    CommonModule,
    MatIcon,
    MatDivider,
    MatFormField,
    MatLabel,
    MatSelect
],
  templateUrl: './edit-roles-dialog.component.html',
  styleUrl: './edit-roles-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EditRolesDialogComponent {
  loading = false
  selectedRoles: string[] = []
  readonly availableRoles = ['Signer', 'Approver', 'Witness', 'Observer', 'CarbonCopy']

  constructor(
    public dialogRef: MatDialogRef<EditRolesDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { user: UserDto},
    private userService: UserManagementService,
    private snackBar: MatSnackBar,
  ) {
    this.selectedRoles = [...(this.data.user.roles ?? [])]
  }

  save(): void {
    this.loading = true
    this.userService.updateRoles(this.data.user.id, this.selectedRoles)
      .subscribe({
        next: (updated) => {
          this.loading = false
          this.dialogRef.close(updated)
          this.snackBar.open('Responsabilidades atualizadas com sucesso!', 'Fechar', { duration: 3000 })
        },
        error: (error) => {
        this.loading = false
        this.snackBar.open(error?.error?.message ?? 'Falha ao atualizar as responsabilidades', 'Fechar', { duration: 3000 })
      }
    })
  }
}

