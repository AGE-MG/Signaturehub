import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FooterComponent } from "../footer/footer.component";
import { HeaderComponent } from "../header/header.component";
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  imports: [ HeaderComponent, FooterComponent, RouterModule, CommonModule],
  templateUrl: './public-layout.component.html',
  styleUrls: ['./public-layout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicLayoutComponent { }
