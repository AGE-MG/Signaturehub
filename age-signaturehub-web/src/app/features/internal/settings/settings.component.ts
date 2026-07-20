import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { UpdateProfileDto, UserDto } from '../../../core/models/signer.model';
import { MatTableDataSource, MatColumnDef, MatTableModule } from '@angular/material/table';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { Router } from '@angular/router';
import { UserManagementService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { EditRolesDialogComponent } from '../../../shared/components/edit-roles-dialog.component/edit-roles-dialog.component';
import { MatTabGroup, MatTab, MatTabChangeEvent } from "@angular/material/tabs";
import { MatIconModule } from "@angular/material/icon";
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { MatCard } from "@angular/material/card";
import { MatDivider } from "@angular/material/divider";
import { MatFormField, MatLabel, MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotificationCapabilities } from '../../../core/models/user.model';
import { ExternalServiceConnection } from '../../../core/models/external-service.model';
import { ExternalServiceService } from '../../../core/services/external-service.service';

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPass = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;
  return newPass && confirmPassword && newPass !== confirmPassword ? { mismatch: true } : null;
}

@Component({
  selector: 'app-settings.component',
  imports: [MatTabGroup, MatTab, MatIconModule, MatProgressSpinner, MatCard, MatDivider, MatFormField, MatLabel, MatFormFieldModule, MatInputModule, MatButtonModule, MatTooltipModule, ReactiveFormsModule, MatColumnDef, MatTableModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsComponent implements OnInit {
  currentUser: UserDto | null = null;
  loadingProfile = false;
  savingProfile = false;
  savingPassword = false;
  notificationCapabilities: NotificationCapabilities = { emailConfigured: false, externalServices: [] };
  loadingNotificationCapabilities = true;
  browserNotificationsEnabled = false;
  browserNotificationPermission: NotificationPermission | 'unsupported' = 'unsupported';
  externalConnections: ExternalServiceConnection[] = [];
  showExternalServiceForm = false;
  savingExternalService = false;
  editingExternalServiceId: string | null = null;
  readonly externalServiceEvents = [
    { value: 'document.created', label: 'Documento criado' },
    { value: 'document.updated', label: 'Documento atualizado' },
    { value: 'document.completed', label: 'Documento concluído' },
    { value: 'document.deleted', label: 'Documento removido' },
    { value: 'signature.requested', label: 'Assinatura solicitada' },
    { value: 'signature.signed', label: 'Documento assinado' },
    { value: 'signature.rejected', label: 'Assinatura rejeitada' }
  ];

  usersDataSource = new MatTableDataSource<UserDto>();
  usersDisplayedColumns = ['avatar', 'name', 'email', 'roles', 'actions'];
  loadingUsers = false;

  profileForm!: FormGroup
  passwordForm!: FormGroup
  externalServiceForm!: FormGroup

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
    private cdr: ChangeDetectorRef,
    private externalServiceService: ExternalServiceService,
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
  }, {validators: passwordMatchValidator}),
    this.externalServiceForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      url: ['', [Validators.required, Validators.pattern(/^https:\/\/[^\s]+$/i)]],
      events: [[], Validators.required]
    })
  }

  ngOnInit(): void {
    this.loadProfile();
    this.loadNotificationSettings();
    this.loadExternalConnections();
  }

  get hasAccountEmail(): boolean {
    return !!this.currentUser?.email?.trim();
  }

  get emailChannelAvailable(): boolean {
    return this.hasAccountEmail && this.notificationCapabilities.emailConfigured;
  }

  get browserPermissionLabel(): string {
    switch (this.browserNotificationPermission) {
      case 'granted': return 'Permissão concedida neste navegador';
      case 'denied': return 'Permissão bloqueada nas configurações do navegador';
      case 'default': return 'Aguardando sua autorização';
      default: return 'Este navegador não oferece suporte';
    }
  }

  loadNotificationSettings(): void {
    if (typeof window !== 'undefined') {
      this.browserNotificationsEnabled = localStorage.getItem('browserNotificationsEnabled') === 'true';
      this.browserNotificationPermission = 'Notification' in window ? Notification.permission : 'unsupported';
    }

    this.authService.getNotificationCapabilities().subscribe({
      next: response => {
        if (response.success && response.data) this.notificationCapabilities = response.data;
        this.loadingNotificationCapabilities = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.loadingNotificationCapabilities = false;
        this.cdr.markForCheck();
      }
    });
  }

  async toggleBrowserNotifications(enabled: boolean): Promise<void> {
    if (typeof window === 'undefined' || !('Notification' in window)) {
      this.browserNotificationsEnabled = false;
      return;
    }

    if (enabled && Notification.permission !== 'granted') {
      this.browserNotificationPermission = await Notification.requestPermission();
      enabled = this.browserNotificationPermission === 'granted';
    }

    this.browserNotificationsEnabled = enabled;
    localStorage.setItem('browserNotificationsEnabled', String(enabled));
    this.cdr.markForCheck();
    this.snackBar.open(enabled ? 'Notificações deste navegador ativadas.' : 'Notificações deste navegador desativadas.', 'Fechar', { duration: 3000 });
  }

  loadExternalConnections(): void {
    this.externalServiceService.getAll().subscribe({
      next: response => { this.externalConnections = response.data ?? []; this.cdr.markForCheck(); },
      error: () => this.snackBar.open('Não foi possível carregar as integrações.', 'Fechar', { duration: 3000 })
    });
  }

  toggleExternalEvent(eventName: string, checked: boolean): void {
    const events = new Set<string>(this.externalServiceForm.value.events ?? []);
    checked ? events.add(eventName) : events.delete(eventName);
    this.externalServiceForm.patchValue({ events: [...events] });
  }

  isExternalEventSelected(eventName: string): boolean { return (this.externalServiceForm.value.events ?? []).includes(eventName); }

  openExternalServiceForm(connection?: ExternalServiceConnection): void {
    this.editingExternalServiceId = connection?.id ?? null;
    this.externalServiceForm.reset({ name: connection?.name ?? '', url: connection?.url ?? '', events: connection?.events ?? [] });
    this.showExternalServiceForm = true;
  }

  closeExternalServiceForm(): void { this.showExternalServiceForm = false; this.editingExternalServiceId = null; }

  saveExternalService(): void {
    if (this.externalServiceForm.invalid) { this.externalServiceForm.markAllAsTouched(); return; }
    this.savingExternalService = true;
    const operation = this.editingExternalServiceId
      ? this.externalServiceService.update(this.editingExternalServiceId, this.externalServiceForm.value)
      : this.externalServiceService.create(this.externalServiceForm.value);
    operation.subscribe({
      next: response => {
        this.savingExternalService = false;
        if (response.data?.secret) this.showGeneratedSecret(response.data.secret);
        this.closeExternalServiceForm();
        this.loadExternalConnections();
        this.snackBar.open('Integração salva com sucesso.', 'Fechar', { duration: 3000 });
      },
      error: error => {
        this.savingExternalService = false;
        this.snackBar.open(error?.error?.message ?? 'Não foi possível salvar a integração.', 'Fechar', { duration: 4000 });
      }
    });
  }

  setExternalServiceActive(connection: ExternalServiceConnection): void {
    this.externalServiceService.setActive(connection.id, !connection.isActive).subscribe(() => this.loadExternalConnections());
  }

  removeExternalService(connection: ExternalServiceConnection): void {
    if (!confirm(`Remover a integração ${connection.name}?`)) return;
    this.externalServiceService.remove(connection.id).subscribe(() => this.loadExternalConnections());
  }

  private showGeneratedSecret(secret: string): void {
    navigator.clipboard?.writeText(secret).catch(() => undefined);
    alert(`Segredo HMAC (copiado para a área de transferência):\n\n${secret}\n\nEle não será exibido novamente.`);
  }

  get isAdmin(): boolean {
    return this.authService.hasAnyRole(['Admin', 'Administrator']);
  }

  // ─── Active Directory ─────────────────────────────────────────────────────
  /** true quando o usuário foi provisionado/sincronizado via Active Directory */
  get isFromActiveDirectory(): boolean {
    return !!this.currentUser?.networkUserName;
  }

  /** Login de rede formatado (remove prefixo de domínio "AGEMG\" se presente) */
  get networkLogin(): string {
    const raw = this.currentUser?.networkUserName ?? '';
    const parts = raw.split('\\');
    return parts.length > 1 ? parts[1] : raw;
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
        next: (response) => {
          const user = response.data;
          this.currentUser = user;
          this.profileForm.patchValue(user);
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
