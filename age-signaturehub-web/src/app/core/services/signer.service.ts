import { Injectable } from "@angular/core";
import { environment } from "../../../environments/environment.prod";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuditLogDto, AuditLogFilter, CreateSignatureFlowDto, RejectRequest, SignatureFlowDetailDto, SignerDto, SignRequest, UpdateProfileDto, UserDto } from "../models/signer.model";

@Injectable({
  providedIn: "root",
})
export class SignerService {
  private readonly base = `${environment.apiUrl}/signers`;

  constructor(private http: HttpClient) {}

  getPendingByEmail(email: string): Observable<SignerDto[]> {
    return this.http.get<SignerDto[]>(`${this.base}/pending/${encodeURIComponent(email)}`);
  }

  getById(id: string): Observable<SignerDto> {
    return this.http.get<SignerDto>(`${this.base}/${id}`);
  }

  sign(payload: SignRequest): Observable<SignerDto> {
    return this.http.post<SignerDto>(`${this.base}/sign`, payload);
  }

  reject(payload: RejectRequest): Observable<SignerDto> {
    return this.http.post<SignerDto>(`${this.base}/reject`, payload);
  }

  buildDeviceMeta(): Partial<SignRequest> {
    return {
      userAgent: navigator.userAgent,
      deviceInfo: `${navigator.platform} - ${navigator.language}`,
      ipAddress: undefined, // IP address would typically be obtained from the server or a third-party service
    };
  }
}

// Signature Flow Service
@Injectable({
  providedIn: "root",
})
export class SignatureFlowService {
  private readonly base = `${environment.apiUrl}/signatureflow`;

  constructor(private http: HttpClient) {}

  create(payload: CreateSignatureFlowDto): Observable<SignatureFlowDetailDto> {
    return this.http.post<SignatureFlowDetailDto>(`${this.base}`, payload);
  }

  getById(id: string): Observable<SignatureFlowDetailDto> {
    return this.http.get<SignatureFlowDetailDto>(`${this.base}/${id}`);
  }

  GetByDocument(documentId: string): Observable<SignatureFlowDetailDto> {
    return this.http.get<SignatureFlowDetailDto>(`${this.base}/document/${documentId}`);
  }
}

// Audit Log Service

@Injectable({
  providedIn: "root",
})
export class AuditLogService {
  private readonly base = `${environment.apiUrl}/auditlogs`;

  constructor(private http: HttpClient) {}

  GetByDocument(documentId: string): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(`${this.base}/document/${documentId}`);
  }

  GetByDateRange(filter: AuditLogFilter): Observable<AuditLogDto[]> {
    const params = new HttpParams()
    .set("startDate", filter.startDate)
    .set("endDate", filter.endDate);
    return this.http.get<AuditLogDto[]>(`${this.base}/date-range`, { params });
  }

}

// User Service
@Injectable({
  providedIn: "root",
})
export class UserManagementService {
  private readonly base = `${environment.apiUrl}/users`;
  private readonly authBase = `${environment.apiUrl}/auth`;

  constructor(private http: HttpClient) {}

  getMe(): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.authBase}/me`)
    .pipe(
      (source) => new (class  { subscribe = (o: any )=> source.subscribe({
        next: (r: any) => o.next?.(r?.data || r),
        error: (e: any) => o.error?.(e?.error || e),
        complete: () => o.complete?.(),
      })})() as any
    );
  }

  updateProfile(dto: UpdateProfileDto): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.base}/me`, dto)
  }

  listAll(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${this.base}`);
  }

  updateRoles(userId: string, roles: string[]): Observable<UserDto> {
    return this.http.put<UserDto>(`${this.base}/${userId}/roles`, { roles });
  }

  remove(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${userId}`);
  }
}

