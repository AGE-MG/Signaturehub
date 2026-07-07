import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from '../../../core/services/auth.service';
import { User } from '../../../core/models/user.model';
import { finalize, timeout } from 'rxjs';

export interface ProfileDialogData {
  user: User | null;
}

@Component({
  selector: 'app-profile-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatDividerModule,
  ],
  templateUrl: './profile-dialog.component.html',
  styleUrl: './profile-dialog.component.scss',
})
export class ProfileDialogComponent implements OnInit {
  user: User | null;
  loading = false;
  initialLoading = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: ProfileDialogData,
    private dialogRef: MatDialogRef<ProfileDialogComponent>,
    private authService: AuthService,
  ) {
    this.user = data.user;
  }

  ngOnInit(): void {
    this.refreshUser(!this.user);
  }

  refreshUser(forceFullLoading = false): void {
    this.loading = true;
    this.initialLoading = forceFullLoading;

    this.authService.getCurrentUser().pipe(
      timeout(10000),
      finalize(() => {
        this.loading = false;
        this.initialLoading = false;
      })
    ).subscribe({
      next: (response) => {
        this.user = response.data;
      },
      error: () => {
      }
    });
  }

  close(): void {
    this.dialogRef.close();
  }

  getInitials(): string {
    const name = this.user?.fullName?.trim();
    if (!name) {
      return '??';
    }

    const parts = name.split(' ').filter(Boolean);
    if (parts.length === 1) {
      return parts[0].slice(0, 2).toUpperCase();
    }

    return `${parts[0][0]}${parts[parts.length - 1][0]}`.toUpperCase();
  }

  getAuthSource(): string {
    return this.user?.networkUserName
      ? 'Active Directory / sessão Windows'
      : 'Conta interna da aplicação';
  }

  getRoleLabel(): string {
    const roles = this.user?.roles ?? [];
    return roles.length > 0 ? roles.join(', ') : 'Sem roles atribuídas';
  }

  getDisplayValue(value?: string): string {
    return value?.trim() || 'Não informado';
  }
}
