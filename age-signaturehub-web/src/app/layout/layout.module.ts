import { NgModule } from "@angular/core";
import { HeaderComponent } from "./header/header.component";
import { FooterComponent } from "./footer/footer.component";
import { PublicLayoutComponent } from "./public-layout/public-layout.component";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatIconModule } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    HeaderComponent,
    FooterComponent,
    PublicLayoutComponent
  ],
  exports: [
    HeaderComponent,
    FooterComponent,
    PublicLayoutComponent
  ]
})
export class LayoutModule { }
