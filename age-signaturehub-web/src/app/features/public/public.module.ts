import { NgModule } from "@angular/core";
import { HomeComponent } from "./home/home.component";
import { CommonModule } from "@angular/common";
import { RouterModule } from "@angular/router";
import { MatIconModule } from "@angular/material/icon";
import { MatButtonModule } from "@angular/material/button";

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule,
    HomeComponent
  ],
  exports: [
    HomeComponent
  ]
})
export class PublicModule { }
