import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../../core/services/auth.service';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  loginForm: FormGroup;
  hidePassword = true;
  loading = false;
  errorMessage: string | null = null;
  returnUrl: string;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar
  ) {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.loading = true;
      this.errorMessage = null;

      const loginRequest = this.loginForm.value;

      this.authService.login(loginRequest).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('Login realizado com sucesso!', 'Fechar', {
              duration: 3000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
              panelClass: ['success-snackbar']
            });
            this.router.navigate([this.returnUrl]);
          } else {
            this.errorMessage = response.message || 'Falha no login. Verifique suas credenciais.';
            this.loading = false;
            this.snackBar.open(this.errorMessage, 'Fechar', {
              duration: 3000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
              panelClass: ['error-snackbar']
            });
          }
        },
        error: (error) => {
          console.error('Login error', error);

          if (error.error?.message) {
            this.errorMessage = error.error.message;
          }else if (error.status === 401) {
            this.errorMessage = 'Credenciais inválidas. Por favor, tente novamente.';
          } else if (error.status === 0) {
            this.errorMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';
          } else {
            this.errorMessage = 'Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.';
          }

          this.loading = false;
        }
      })
    } else {
      Object.keys(this.loginForm.controls).forEach(key => {
        this.loginForm.get(key)?.markAsTouched();
      });
    }
  }

  togglePasswordVisibility(): void {
    this.hidePassword = !this.hidePassword;
  }
}
