import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CreateDocumentDto, DocumentDto, DocumentFilterParams, DocumentPagedResult, DocumentStatus, TransferDocumentDepartmentDto } from "../models/document.model";
import { map, Observable } from "rxjs";
import { ApiResponse } from "../models/api-response.model";

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/documents`;

  private unwrapApiResponse<T>(response: T | ApiResponse<T>): T {
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as ApiResponse<T>).data;
    }
    return response as T;
  }

  getDocuments(filters?: DocumentFilterParams): Observable<DocumentPagedResult | DocumentDto[]> {
    let params = new HttpParams();
    if (filters?.status !== undefined) {
      params = params.set('status', filters.status.toString());
    }

    return this.http.get<DocumentPagedResult | DocumentDto[] | ApiResponse<DocumentPagedResult | DocumentDto[]>>(this.baseUrl, { params })
      .pipe(
        map((response) => this.unwrapApiResponse<DocumentPagedResult | DocumentDto[]>(response))
      );
  }

  getDocumentById(documentId: string): Observable<DocumentDto> {
    return this.http
      .get<DocumentDto | ApiResponse<DocumentDto>>(`${this.baseUrl}/${documentId}`)
      .pipe(map((response) => this.unwrapApiResponse<DocumentDto>(response)));
  }

  getDocumentByStatus(status: DocumentStatus): Observable<DocumentDto[]> {
    return this.http
      .get<DocumentDto[] | ApiResponse<DocumentDto[]>>(`${this.baseUrl}/by-status/${status}`)
      .pipe(map((response) => this.unwrapApiResponse<DocumentDto[]>(response)));
  }

  createDocument(file: File, data: CreateDocumentDto): Observable<DocumentDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append('title', data.title);
    if (data.description) {
      formData.append('description', data.description);
    }
    if (data.expiresAt) {
      formData.append('expiresAt', data.expiresAt);
    }
    formData.append('createdByUserId', data.createdByUserId);
    formData.append('isConfidential', String(!!data.isConfidential));

    return this.http
      .post<DocumentDto | ApiResponse<DocumentDto>>(this.baseUrl, formData)
      .pipe(map((response) => this.unwrapApiResponse<DocumentDto>(response)));
  }

  downloadDocument(id: string, filename: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/download`, { responseType: 'blob' });
  }

  triggerDownload(blob: Blob, filename: string): void {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }

  deleteDocument(documentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${documentId}`);
  }

  transferDepartment(documentId: string, payload: TransferDocumentDepartmentDto): Observable<DocumentDto> {
    return this.http
      .post<DocumentDto | ApiResponse<DocumentDto>>(`${this.baseUrl}/${documentId}/transfer-department`, payload)
      .pipe(map((response) => this.unwrapApiResponse<DocumentDto>(response)));
  }

  archiveDocument(documentId: string): Observable<DocumentDto> {
    return this.http.patch<DocumentDto>(`${this.baseUrl}/${documentId}/archive`, {});
  }

  signDocument(signatureData: unknown): Observable<unknown> {
    return this.http.post<unknown>(`${environment.apiUrl}/signers/sign`, signatureData || {});
  }
}
