import { Component } from "@angular/core";
import { NgIcon, provideIcons } from "@ng-icons/core";
import { boxInstagramLogo, boxYoutubeLogo } from "@ng-icons/boxicons/logos";
import { iconoirSearchWindow } from "@ng-icons/iconoir";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";

@Component({
  selector: "app-footer",
  imports: [NgIcon, CommonModule, RouterModule],
  templateUrl: "./footer.component.html",
  styleUrls: ["./footer.component.scss"],
  viewProviders: [provideIcons({ boxYoutubeLogo, boxInstagramLogo, iconoirSearchWindow})]
})
export class FooterComponent {
  currentYear: number = new Date().getFullYear();
}
