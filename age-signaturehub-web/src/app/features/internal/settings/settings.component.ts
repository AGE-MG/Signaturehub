import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { UpdateProfileDto, UserDto } from '../../../core/models/signer.model';
import { MatTableDataSource } from '@angular/material/table';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { UserManagementService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPass = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPass && confirmPassword && newPass !== confirmPassword ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings.component',
  imports: [],
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

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

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
  
}
