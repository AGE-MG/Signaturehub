import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { map, Observable } from "rxjs";
import { DashboardStats, RecentDocument } from "../models/dasboard.model";
import { ApiResponse } from "../models/user.model";

@Injectable({
    providedIn: 'root'
})

export class DashboardService {
  private readonly API_URL = `${environment.apiUrl}/dashboard`

  constructor(
    private http: HttpClient
  ) {}

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.API_URL}/stats`)
    .pipe(map(response => response as DashboardStats));
  }

  getRecentDocuments(count: number = 5): Observable<RecentDocument[]> {
    return this.http.get<ApiResponse<RecentDocument[]>>(`${this.API_URL}/recent-documents?count=${count}`)
    .pipe(map(response => response.data as RecentDocument[]));
  }

  getNotifications(count: number = 5): Observable<Notification[]> {
    return this.http.get<ApiResponse<Notification[]>>(`${this.API_URL}/notifications?count=${count}`)
    .pipe(map(response => response.data as Notification[]));
  }

  markNotificationAsRead(notificationId: string): Observable<boolean> {
    return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/notifications/${notificationId}/read`, {})
    .pipe(map(response => response.data));
  }

  markAllNotificationsAsRead(): Observable<boolean> {
    return this.http.put<ApiResponse<boolean>>(`${this.API_URL}/notifications/mark-all-read`, {})
    .pipe(map(response => response.data));
  }
}
