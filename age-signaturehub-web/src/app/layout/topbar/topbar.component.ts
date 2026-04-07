import { Component, OnInit } from '@angular/core';
import { User } from '../../core/models/user.model';
import { AuthService } from '../../core/services/auth.service';
import { MatIconModule } from "@angular/material/icon";
import { MatBadgeModule } from "@angular/material/badge";
import { MatMenuTrigger, MatMenuModule } from "@angular/material/menu";
import { MatDividerModule } from "@angular/material/divider";

@Component({
  selector: 'app-topbar',
  imports: [MatIconModule, MatBadgeModule, MatMenuModule, MatMenuTrigger, MatDividerModule],
  templateUrl: './topbar.component.html',
  styleUrls: ['./topbar.component.scss'],
})
export class TopbarComponent implements OnInit {
  currentUser: User | null = null;

  constructor(private authService: AuthService) {

  }

  ngOnInit(): void {
    this.authService.currentUser$.subscribe(user => {
      this.currentUser = user;
    })
  }

  getUserInitials(): string {
    if (!this.currentUser?.fullName) {
      return '';
    }

    const names = this.currentUser.fullName.split(' ');
    return names.length > 1 ? `${names[0][0]}${names[names.length - 1][0]}`.toUpperCase() : names[0].substring(0, 2).toUpperCase();
  }

}
