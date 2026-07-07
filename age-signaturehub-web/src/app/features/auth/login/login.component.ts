import { Component, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
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
import { LoginMode } from '../../../core/models/user.model';

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
  readonly LoginMode = LoginMode;
  loginForm: FormGroup;
  hidePassword = true;
  loading = false;
  errorMessage: string | null = null;
  returnUrl: string;
  private isBrowser: boolean;
  selectedLoginMode: LoginMode = LoginMode.ActiveDirectory;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private snackBar: MatSnackBar,
    @Inject(PLATFORM_ID) private platformId: Object

  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);

    if (this.isBrowser && this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
    }

    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      rememberMe: [false]
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/dashboard';
  }

  onSubmit(): void {
    if (this.isActiveDirectoryMode) {
      this.loginWithActiveDirectorySession();
      return;
    }

    if (this.loginForm.valid) {
      this.loading = true;
      this.errorMessage = null;

      const loginRequest = {
        ...this.loginForm.value,
        email: String(this.loginForm.value.email ?? '').trim(),
        loginMode: this.selectedLoginMode,
      };

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

  loginWithCertificate(): void {
    this.snackBar.open('Funcionalidade de login com certificado digital ainda não implementada.', 'Fechar', {
      duration: 3000,
      horizontalPosition: 'end',
      verticalPosition: 'top',
      panelClass: ['info-snackbar']
    });
  }

  loginWithActiveDirectorySession(): void {
    this.loading = true;
    this.errorMessage = null;

    this.authService.loginWithWindowsSession().subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Login AD realizado com sucesso!', 'Fechar', {
            duration: 3000,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: ['success-snackbar']
          });
          this.router.navigate([this.returnUrl]);
          return;
        }

        this.errorMessage = response.message || 'Falha no login via Active Directory.';
        this.loading = false;
      },
      error: (error) => {
        console.error('Windows SSO login error', error);

        if (error.error?.message) {
          this.errorMessage = error.error.message;
        } else if (error.status === 401) {
          this.errorMessage = 'O navegador não conseguiu autenticar com o usuário da máquina. Verifique se a API está na intranet/zona confiável e com Windows Authentication habilitado.';
        } else if (error.status === 0) {
          this.errorMessage = 'Não foi possível conectar ao servidor para o login AD.';
        } else {
          this.errorMessage = 'Falha no login automático com Active Directory.';
        }

        this.loading = false;
      }
    });
  }

  selectLoginMode(mode: LoginMode): void {
    this.selectedLoginMode = mode;
    this.errorMessage = null;
  }

  get isActiveDirectoryMode(): boolean {
    return this.selectedLoginMode === LoginMode.ActiveDirectory;
  }

  get loginFieldLabel(): string {
    return this.isActiveDirectoryMode
      ? 'Usuário de rede AD'
      : 'Usuário interno ou e-mail';
  }

  get loginTitle(): string {
    return this.isActiveDirectoryMode
      ? 'Entrar com Active Directory'
      : 'Entrar com conta interna';
  }

  get loginSubtitle(): string {
    return this.isActiveDirectoryMode
      ? 'Use a mesma credencial de rede da organização.'
      : 'Use a conta interna cadastrada no SignatureHub.';
  }

  get loginPlaceholder(): string {
    return this.isActiveDirectoryMode
      ? 'm10700379'
      : 'usuario.interno@age.mg.gov.br';
  }

  get loginHintText(): string {
    return this.isActiveDirectoryMode
      ? 'Este modo autentica diretamente no domínio/AD da organização.'
      : 'Este modo usa usuário e senha internos da aplicação.';
  }
}
