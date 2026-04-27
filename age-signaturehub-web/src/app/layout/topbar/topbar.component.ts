import { Component, OnDestroy, OnInit } from '@angular/core';
import { User } from '../../core/models/user.model';
import { AuthService } from '../../core/services/auth.service';
import { MatIconModule } from "@angular/material/icon";
import { MatBadgeModule } from "@angular/material/badge";
import { MatMenuTrigger, MatMenuModule } from "@angular/material/menu";
import { MatDividerModule } from "@angular/material/divider";
import { NotificationDto, NotificationType } from '../../core/models/dasboard.model';
import { interval, startWith, Subject, switchMap, takeUntil } from 'rxjs';
import { DashboardService } from '../../core/services/dashboard.service';
import { Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatButtonModule } from "@angular/material/button";
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltip } from "@angular/material/tooltip";

@Component({
  selector: 'app-topbar',
  imports: [MatIconModule, MatBadgeModule, MatMenuModule, MatMenuTrigger, MatDividerModule, MatSnackBarModule, MatButtonModule, MatProgressSpinnerModule, MatTooltip],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit, OnDestroy {
  currentUser: User | null = null;
  notifications: NotificationDto[] = [];
  unreadCount: number = 0;
  loadingNotifications: boolean = false;
  private destroy$ = new Subject<void>();

  constructor(
    private authService: AuthService,
    private dashboardService: DashboardService,
    private router: Router,
    private snackBar: MatSnackBar
  ) {

  }

  ngOnInit(): void {
    this.loadCurrentUser();
    this.loadNotifications();
    this.startNotificationsPolling();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadCurrentUser(): void {
    this.authService.currentUser$.pipe(
      takeUntil(this.destroy$)
    ).subscribe(user => {
      this.currentUser = user;
    });
  }

  private loadNotifications(): void {
    this.loadingNotifications = true;

    this.dashboardService.getNotifications(false).pipe(
      takeUntil(this.destroy$)
    ).subscribe({
      next: (notifications) => {
        this.notifications = notifications;
        this.unreadCount = notifications.filter(n => !n.isRead).length;
        this.loadingNotifications = false;
      },
      error: () => {
        console.error('Failed to load notifications');
        this.loadingNotifications = false;
      }}
    )
  }

  private startNotificationsPolling(): void {
    interval(30000).pipe(
      startWith(0),
      switchMap(() => this.dashboardService.getNotifications(false)),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (notifications) => {
        const previousUnreadCount = this.unreadCount;
        this.notifications = notifications;
        this.unreadCount = notifications.filter(n => !n.isRead).length;

        if (this.unreadCount > previousUnreadCount) {
          this.snackBar.open(
            `Você tem ${this.unreadCount - previousUnreadCount} nova(s) notificação(ões)`,
            'Fechar',
            {
              duration: 5000,
              horizontalPosition: 'end',
              verticalPosition: 'top',
              panelClass: ['info-snackbar']
            }
          )
        }
      },
      error: (err) => {
        console.error('Error polling notifications', err);
      }
    });
  }

  getUserInitials(): string {
    if (!this.currentUser?.fullName) {
      return '??';
    }

    const names = this.currentUser.fullName.split(' ');
    return names.length > 1 ? `${names[0][0]}${names[names.length - 1][0]}`.toUpperCase() : names[0].substring(0, 2).toUpperCase();
  }

  getNotificationIcon(type: NotificationType): string {
    const icons: { [key: number]: string } = {
      [NotificationType.DocumentCreated]: 'description',
      [NotificationType.DocumentSigned]: 'check_circle',
      [NotificationType.DocumentRejected]: 'cancel',
      [NotificationType.DocumentExpired]: 'schedule',
      [NotificationType.DocumentCompleted]: 'verified',
      [NotificationType.SignatureRequested]: 'edit',
      [NotificationType.System]: 'info'
    }
    return icons[type] || 'notifications';
  }

  getNotificationColor(type: NotificationType): string {
    const colors: { [key: number]: string } = {
      [NotificationType.DocumentCreated]: '#1565C0',
      [NotificationType.DocumentSigned]: '#4CAF50',
      [NotificationType.DocumentRejected]: '#F44336',
      [NotificationType.DocumentExpired]: '#FF9800',
      [NotificationType.DocumentCompleted]: '#4CAF50',
      [NotificationType.SignatureRequested]: '#2196F3',
      [NotificationType.System]: '#9E9E9E'
    }
    return colors[type] || '#607D8B';
  }

  getRelativeTime(dateString: string): string {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 1) return 'Agora mesmo';
    if (diffMins < 60) return `${diffMins} min${diffMins > 1 ? 's' : ''} atrás`;
    if (diffHours < 24) return `${diffHours} hora${diffHours > 1 ? 's' : ''} atrás`;
    if (diffDays < 7) return `${diffDays} dia${diffDays > 1 ? 's' : ''} atrás`;
    return date.toLocaleDateString('pt-BR', { day: '2-digit', month: 'short', year: 'numeric' });
  }

  markAsRead(notification: NotificationDto, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    if (notification.isRead) {
      return;
    }

    this.dashboardService.markNotificationAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.unreadCount = this.notifications.filter(n => !n.isRead).length;
      },
      error: (error) => {
        console.error('Failed to mark notification as read', error);
        this.snackBar.open('Falha ao marcar notificação como lida', 'Fechar', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        })
      }
    });
  }

  markAllAsRead(): void {
    this.dashboardService.markAllNotificationsAsRead().subscribe({
      next: () => {
        this.notifications.forEach(n => n.isRead = true);
        this.unreadCount = 0;
        this.snackBar.open('Todas as notificações foram marcadas como lidas', 'Fechar', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['success-snackbar']
        })
      },
      error: (error) => {
        console.error('Failed to mark all notifications as read', error);
        this.snackBar.open('Falha ao marcar todas as notificações como lidas', 'Fechar', {
          duration: 3000,
          horizontalPosition: 'end',
          verticalPosition: 'top',
          panelClass: ['error-snackbar']
        })
      }
    })
  }

  viewNotification(notification: NotificationDto): void {
    this.markAsRead(notification);

    if (notification.relatedDocumentId) {
      this.router.navigate(['/documents', notification.relatedDocumentId]);
    } else {
      this.snackBar.open('Notificação sem documento relacionado', 'Fechar', {
        duration: 3000,
        horizontalPosition: 'end',
        verticalPosition: 'top',
        panelClass: ['info-snackbar']
      })
    }
  }

  viewAllNotifications(): void {
    this.router.navigate(['/notifications']);
  }

  goToProfile(): void {
    this.router.navigate(['/profile']);
  }

  goToSettings(): void {
    this.router.navigate(['/settings']);
  }

  goToHelp(): void {
    this.router.navigate(['/help']);
  }

}
