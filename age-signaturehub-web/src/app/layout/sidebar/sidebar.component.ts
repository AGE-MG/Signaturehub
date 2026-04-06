import { Component, EventEmitter, Output } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { BrowserModule } from "@angular/platform-browser";
import { MatIcon } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";
import { MatTooltipModule } from "@angular/material/tooltip";

interface menuItem {
  icon: string;
  label: string;
  route: string;
  badge?: string;
}

@Component({
  selector: 'app-sidebar',
  imports: [BrowserModule, MatIcon, MatButtonModule, MatTooltipModule, RouterLink],
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
})
export class SidebarComponent {
  @Output() toggleSidebar = new EventEmitter<boolean>();

  loggingOut = false;
  collapsed = false;

  menuItems: menuItem[] = [
    {
      icon: 'dashboard',
      label: 'Dashboard',
      route: '/dashboard',
    },
    {
      icon: 'description',
      label: 'Documentos',
      route: '/documents',
    },
    {
      icon: 'pending_actions',
      label: 'Pendentes',
      route: '/pending-signatures',
      badge: '3',
    },
    {
      icon: 'history',
      label: 'Histórico',
      route: '/history',
    },
    {
      icon: 'settings',
      label: 'Configurações',
      route: '/settings',
    }
  ]

  constructor(
    private router: Router,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}

  toggleCollapse(): void {
    this.collapsed = !this.collapsed;
    this.toggleSidebar.emit(this.collapsed);
  }

  isActiveRoute(route: string): boolean {
    return this.router.url === route;
  }

  Logout(): void {
    if (this.loggingOut) return;

    this.loggingOut = true;

    this.authService.logout().subscribe({
      next: () => {
        this.snackBar.open('Logout realizado com sucesso!', 'Fechar', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['snackbar-success']
        });

        setTimeout(() => {
          this.router.navigate(['/login']);
        }, 500);
      },
      error: (err) => {
        console.error('Erro ao realizar logout:', err);
        this.loggingOut = false;

        let errorMessage = 'Ocorreu um erro ao realizar logout.';

        if (err.status === 0) {
          errorMessage = 'Não foi possível conectar ao servidor. Verifique sua conexão.';
        } else if (err.status === 401) {
          errorMessage += ' Sua sessão expirou. Por favor, faça login novamente.';
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 2000);
        } else {
          errorMessage += 'Tente novamente mais tarde.';
        }

        this.snackBar.open(errorMessage, 'Fechar', {
          duration: 5000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        });
      }
    })
  }
}
