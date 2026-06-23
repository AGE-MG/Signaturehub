import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { UpdateProfileDto, UserDto } from '../../../core/models/signer.model';
import { MatTableDataSource } from '@angular/material/table';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { UserManagementService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { EditRolesDialogComponent } from '../../../shared/components/edit-roles-dialog.component/edit-roles-dialog.component';
import { MatTabGroup, MatTab, MatTabChangeEvent } from "@angular/material/tabs";
import { CdkNoDataRow } from "@angular/cdk/table";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatCard } from "@angular/material/card";
import { MatDivider } from "@angular/material/divider";
import { MatFormField, MatLabel, MatFormFieldModule } from "@angular/material/form-field";

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPass = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPass && confirmPassword && newPass !== confirmPassword ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings.component',
  imports: [MatTabGroup, MatTab, CdkNoDataRow, MatIconModule, MatProgressSpinner, MatCard, MatDivider, MatFormField, MatLabel, MatFormFieldModule, ReactiveFormsModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
  currentUser: UserDto | null = null;
  loadingProfile = false;
  savingProfile = false;
  savingPassword = false;

  usersDataSource = new MatTableDataSource<UserDto>();
  usersDisplayedColumns = ['avatar', 'nome', 'email', 'função', 'ações'];
  loadingUsers = false;

  profileForm!: FormGroup
  passwordForm!: FormGroup

  hideCurrentPwd = true;
  hideNewPwd = true;
  hideConfirmPwd = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private userService: UserManagementService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) {
    this.profileForm = this.fb.group({
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    department: [''],
    position: [''],
    registrationNumber: ['']
  }),
    this.passwordForm = this.fb.group({
    currentPassword: ['', [Validators.required]],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
  }, {validators: passwordMatchValidator})
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  get isAdmin(): boolean {
    return this.authService.hasRole('Admin');
  }

  loadProfile(): void {
    this.loadingProfile = true;

    const user = this.authService.getUserValue();
    if (user) {
      this.currentUser = user as UserDto;
      this.profileForm.patchValue({
        fullName: user.fullName,
        department: (user as UserDto).department ?? '',
        position: (user as UserDto).position ?? '',
        registrationNumber: (user as UserDto).registrationNumber ?? ''
      });
      this.loadingProfile = false;
    } else {
      this.authService.getCurrentUser().subscribe({
        next: (res) => {
          const u = (res as any)?.data ?? res;
          this.currentUser = u;
          this.profileForm.patchValue(u);
          this.loadingProfile = false;
        },
        error: () => {
          this.loadingProfile = false;
        }
      })
    }
  }

  saveProfile(): void {
    if (this.profileForm.invalid) return;
    this.savingProfile = true;
    const dto: UpdateProfileDto = this.profileForm.value as UpdateProfileDto;
    this.userService.updateProfile(dto).subscribe({
      next: (updated) => {
        this.savingProfile = false;
        this.currentUser = updated;
        this.snackBar.open('Perfil atualizado com sucesso!', 'fechar', {
          duration: 3000
        })
      },
      error: (err) => {
        this.savingProfile = false;
        this.snackBar.open(err?.error?.message ?? 'Erro ao atualizar perfil!', 'fechar', {
          duration: 3000
        })
      }
    })
  }

  savePassword(): void {
    if (this.passwordForm.invalid) {
      return;
    }

    this.savingPassword = true;
    const { currentPassword, newPassword, confirmPassword }: { currentPassword: string; newPassword: string; confirmPassword: string } = this.passwordForm.value;
    this.authService.changePassword({ currentPassword: currentPassword, newPassword: newPassword, confirmPassword: confirmPassword }).subscribe({
      next: (res) => {
        this.savingPassword = false;
        this.passwordForm.reset();
        this.snackBar.open('Senha atualizada com sucesso!', 'fechar', {
          duration: 3000
        })
      },
      error: (err) => {
        this.savingPassword = false;
        this.snackBar.open(err?.error?.message ?? 'Erro ao atualizar senha!', 'fechar', {
          duration: 3000
        })
      }
    })
  }

  loadUsers(): void {
    this.loadingUsers = true;
    this.userService.listAll().subscribe({
      next: (users) => {
        this.usersDataSource.data = users;
        this.loadingUsers = false;
      },
      error: () => {
        this.loadingUsers = false;
        this.snackBar.open('Erro ao carregar usuários!', 'fechar', {
          duration: 3000
        })
      }
    })
  }

  onAdminTabSelect(): void {
    if (this.usersDataSource.data.length === 0) {
      this.loadUsers();
    }
  }

  onTabChange(event: MatTabChangeEvent): void {
    const selectedTabIndex = event.index;
    const adminTabIndex = 1; // Índice da aba "Admin" (começando do 0)
    if (selectedTabIndex === adminTabIndex && this.isAdmin) {
      this.onAdminTabSelect();
    }
  }

  editRoles(user: UserDto): void {
    this.dialog.open(EditRolesDialogComponent, {
      data: { user },
      width: '400px'
    })
    .afterClosed().subscribe((result) => {
      if (result) {
        const idx = this.usersDataSource.data.findIndex(u => u.id === result.id);
        if (idx >= 0) {
          const copy = [...this.usersDataSource.data];
          copy[idx] = result;
          this.usersDataSource.data = copy;
        }
      }
    })
  }

  removeUser(user: UserDto): void {
    if (!confirm(`Tem certeza que deseja remover o usuário ${user.fullName}?`)) {
      return;
    }

    this.userService.remove(user.id).subscribe({
      next: () => {
        this.usersDataSource.data = this.usersDataSource.data.filter(u => u.id !== user.id);
        this.snackBar.open('Usuário removido com sucesso!', 'fechar', {
          duration: 3000
        })
      },
      error: (err) => {
        this.snackBar.open(err?.error?.message ?? 'Erro ao remover usuário!', 'fechar', {
          duration: 3000
        })
      }
    })
  }

  getInitials(fullName: string): string {
    return fullName.split(' ').slice(0, 2).map(n => n[0].toUpperCase()).join('');
  }
}
