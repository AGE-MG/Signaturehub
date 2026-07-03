import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { map, Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import { ApiResponse } from "../models/api-response.model";
import { PublicDocumentVerification } from "../models/public-verification.model";

@Injectable({
  providedIn: 'root'
})
export class PublicVerificationService {
  private readonly base = `${environment.apiUrl}/public/verification`;

  constructor(private http: HttpClient) {}

  getDocumentVerification(documentId: string, version?: number | null): Observable<PublicDocumentVerification> {
    const suffix = version ? `?version=${version}` : '';
    return this.http
      .get<PublicDocumentVerification | ApiResponse<PublicDocumentVerification>>(`${this.base}/documents/${documentId}${suffix}`)
      .pipe(map((response) => this.unwrapApiResponse(response)));
  }

  private unwrapApiResponse<T>(response: T | ApiResponse<T>): T {
    if (response && typeof response === 'object' && 'data' in response) {
      return (response as ApiResponse<T>).data;
    }

    return response as T;
  }
}
