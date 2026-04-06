import { Component } from '@angular/core';
import {  SidebarComponent } from "../sidebar/sidebar.component";
import { TopbarComponent } from "../topbar/topbar.component";
import { RouterOutlet } from "@angular/router";

@Component({
  selector: 'app-internal-layout',
  imports: [ TopbarComponent, RouterOutlet, SidebarComponent],
  templateUrl: './internal-layout.component.html',
  styleUrls: ['./internal-layout.component.scss'],
})
export class InternalLayoutComponent {
  sidebarCollapsed = false;

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }
}
