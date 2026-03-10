import { ChangeDetectionStrategy, Component } from '@angular/core';
import { FooterComponent } from "../footer/footer.component";
import { HeaderComponent } from "../header/header.component";
import { AppRoutingModule } from "../../app-routing-module";

@Component({
  selector: 'app-public-layout',
  imports: [ HeaderComponent, FooterComponent, AppRoutingModule],
  templateUrl: './public-layout.component.html',
  styleUrls: ['./public-layout.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PublicLayoutComponent { }
