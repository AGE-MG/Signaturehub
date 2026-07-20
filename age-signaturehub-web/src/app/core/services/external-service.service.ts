import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ExternalServiceConnection, SaveExternalServiceConnection } from '../models/external-service.model';

@Injectable({ providedIn: 'root' })
export class ExternalServiceService {
  private readonly apiUrl = `${environment.apiUrl}/external-services`;
  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<ExternalServiceConnection[]>> { return this.http.get<ApiResponse<ExternalServiceConnection[]>>(this.apiUrl); }
  create(value: SaveExternalServiceConnection): Observable<ApiResponse<ExternalServiceConnection>> { return this.http.post<ApiResponse<ExternalServiceConnection>>(this.apiUrl, value); }
  update(id: string, value: SaveExternalServiceConnection): Observable<ApiResponse<ExternalServiceConnection>> { return this.http.put<ApiResponse<ExternalServiceConnection>>(`${this.apiUrl}/${id}`, value); }
  setActive(id: string, active: boolean): Observable<void> { return this.http.put<void>(`${this.apiUrl}/${id}/active`, { active }); }
  remove(id: string): Observable<void> { return this.http.delete<void>(`${this.apiUrl}/${id}`); }
}
