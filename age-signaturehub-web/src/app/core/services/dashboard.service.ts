import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient } from "@angular/common/http";
import { map, Observable } from "rxjs";
import { DashboardStats, NotificationDto, RecentDocument } from "../models/dasboard.model";
import { ApiResponse } from "../models/api-response.model";

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

  getNotifications(unreadOnly: boolean = false): Observable<NotificationDto[]> {
    return this.http.get<ApiResponse<NotificationDto[]>>(`${this.API_URL}/notifications?unreadOnly=${unreadOnly}`)
    .pipe(map(response => response.data as NotificationDto[]));
  }

  markNotificationAsRead(notificationId: string): Observable<void> {
    return this.http.put<ApiResponse<void>>(`${this.API_URL}/notifications/${notificationId}/read`, {})
    .pipe(map(() => undefined) );
  }

  markAllNotificationsAsRead(): Observable<void> {
    return this.http.put<ApiResponse<void>>(`${this.API_URL}/notifications/mark-all-read`, {})
    .pipe(map(() => undefined));
  }
}
