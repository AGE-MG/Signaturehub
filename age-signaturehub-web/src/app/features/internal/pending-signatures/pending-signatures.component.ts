import { Component } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-pending-signatures',
  imports: [
    MatIconModule,
    MatDividerModule,
    ReactiveFormsModule,
    MatFormFieldModule,
  ],
  templateUrl: './pending-signatures.component.html',
  styleUrl: './pending-signatures.component.scss',
})
export class PendingSignaturesComponent {

}
