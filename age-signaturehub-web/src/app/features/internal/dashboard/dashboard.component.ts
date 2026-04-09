import { Component, OnInit } from '@angular/core';
import { MatAnchor } from "@angular/material/button";
import { MatIcon } from "@angular/material/icon";
import { MatCard } from "@angular/material/card";

interface StatCard {
  title: string;
  value: string | number;
  icon: string;
  color: string;
  trend?: {
    value: string;
    isPositive: boolean;
  }
}

interface RecentDocument {
  id: string;
  title: string;
  status: 'pending' | 'signed' | 'rejected' | 'expired';
  createdAt: Date;
  signers: number;
  signedcount: number;
}


@Component({
  selector: 'app-dashboard',
  imports: [MatAnchor, MatIcon, MatCard],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {

  userName = '';

  stats: StatCard[] = [
    {
      
    }
  ]
}
