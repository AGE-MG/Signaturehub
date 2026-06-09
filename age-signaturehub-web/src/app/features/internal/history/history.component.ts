import { Component } from '@angular/core';


interface ActionMeta {
  label: string;
  icon: string;
  color: string;
}

const ACTION_META: Record<string, ActionMeta> = {
  
}

@Component({
  selector: 'app-history',
  imports: [],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss',
})
export class HistoryComponent {

}
