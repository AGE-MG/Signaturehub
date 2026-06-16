import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { UserDto } from '../../../core/models/signer.model';
import { MatTableDataSource } from '@angular/material/table';
import { AbstractControl, FormBuilder, FormGroup, ValidationErrors, Validators } from '@angular/forms';

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
  ngOnInit(): void {}
}
