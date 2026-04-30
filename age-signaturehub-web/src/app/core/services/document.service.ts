import { inject, Injectable } from "@angular/core";
import { environment } from "../../../environments/environment";
import { HttpClient, HttpParams } from "@angular/common/http";
import { CreateDocumentDto, DocumentDto, DocumentFilterParams, DocumentPagedResult, DocumentStatus } from "../models/document.model";
import { Observable } from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/documents`;

  getDocuments(filters?: DocumentFilterParams): Observable<DocumentPagedResult> {
    let params = new HttpParams();
    if (filters?.status !== undefined) {
      params = params.set('status', filters.status.toString());
    }

    if (filters?.source) {
      params = params.set('source', filters.source);
    }
    if (filters?.search) {
      params = params.set('search', filters.search);
    }
    if (filters?.pageNumber !== undefined) {
      params = params.set('pageNumber', filters.pageNumber.toString());
    }

    if (filters?.pageSize !== undefined) {
      params = params.set('pageSize', filters.pageSize.toString());
    }
    return this.http.get<DocumentPagedResult>(this.baseUrl, { params });
  }

  getDocumentById(documentId: string): Observable<DocumentDto> {
    return this.http.get<DocumentDto>(`${this.baseUrl}/${documentId}`);
  }

  getDocumentByStatus(status: DocumentStatus): Observable<DocumentDto[]> {
    return this.http.get<DocumentDto[]>(`${this.baseUrl}/status/${status}`);
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
    if (data.source) {
      formData.append('source', data.source);
    }
    return this.http.post<DocumentDto>(this.baseUrl, formData);
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

  archiveDocument(documentId: string): Observable<DocumentDto> {
    return this.http.patch<DocumentDto>(`${this.baseUrl}/${documentId}/archive`, {});
  }

  signDocument(id: string, signatureData?: unknown): Observable<DocumentDto> {
    return this.http.post<DocumentDto>(`${this.baseUrl}/${id}/sign`, signatureData || {});
  }
}
