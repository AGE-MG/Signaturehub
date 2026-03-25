import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { BehaviorSubject, catchError, Observable, tap, throwError } from "rxjs";
import { environment } from "../../../environments/environment";
import { ApiResponse, ChangePasswordRequest, LoginRequest, LoginResponse, RegisterRequest, User } from "../models/user.model";
import { HttpClient } from "@angular/common/http";
import { error } from "console";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly API_URL = `${environment.apiUrl}/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.LoadUserFromStorage();
  }

  private LoadUserFromStorage(): void {
    const savedUser = localStorage.getItem('currentUser');
    const token = localStorage.getItem('token');
    if (savedUser && token) {
      try {
        const user: User = JSON.parse(savedUser);
        this.currentUserSubject.next(user);
      } catch (error) {
        console.error('Error parsing user from storage', error);
        this.ClearStorage();
      }
    }
  }

  login(request: LoginRequest): Observable<ApiResponse<LoginResponse>> {
    return this.http.post<ApiResponse<LoginResponse>>(`${this.API_URL}/login`, request)
    .pipe(
      tap(response => {
        if (response.success && response.data) {
          this.setSession(response.data);
        }
      }),
      catchError(error => {
        console.error('Login error', error);
        return throwError(() => error);
      })
    )
  }

  register(request: RegisterRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(`${this.API_URL}/register`, request)
    .pipe(
      catchError(error => {
        console.error('Register error', error);
        return throwError(() => error);
      })
    )
  }

  logout(): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/logout`, {})
    .pipe(
      tap(() => {
        this.clearSession();
      }),
      catchError(error => {
        this.clearSession();
        return throwError(() => error);
      })
    )
  }

  refreshToken(): Observable<ApiResponse<LoginResponse>> {
    const token = this.getToken();
    const refreshToken = this.getRefreshToken();

    if (!token || !refreshToken) {
      return throwError(() => new Error('No token or refresh token available'));
    }

    return this.http.post<ApiResponse<LoginResponse>>(`${this.API_URL}/refresh-token`, { token, refreshToken })
    .pipe(
      tap(response => {
        if (response.success && response.data) {
          this.setSession(response.data);
        }
      }),
      catchError(error => {
        console.error('Token refresh error', error);
        this.clearSession();
        return throwError(() => error);
      })
    )
  }

  getCurrentUser(): Observable<ApiResponse<User>> {
    return this.http.get<ApiResponse<User>>(`${this.API_URL}/me`)
    .pipe(
      tap(response => {
        if (response.success && response.data) {
          this.currentUserSubject.next(response.data);
          localStorage.setItem('currentUser', JSON.stringify(response.data));
        }
      }),
      catchError(error => {
        console.error('Get current user error', error);
        return throwError(() => error);
      })
    )
  }

  changePassword(request: ChangePasswordRequest): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.API_URL}/change-password`, request)
    .pipe(
      catchError(error => {
        console.error('Change password error', error);
        return throwError(() => error);
      })
    )
  }

  private setSession(authResult: LoginResponse): void {
    localStorage.setItem('token', authResult.token);
    localStorage.setItem('refreshToken', authResult.refreshToken);
    localStorage.setItem('tokenExpiration', authResult.tokenExpiration.toString());
    localStorage.setItem('currentUser', JSON.stringify(authResult.user));
    this.currentUserSubject.next(authResult.user);
  }

  private clearSession(): void {
    this.ClearStorage();
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  private ClearStorage(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('tokenExpiration');
    localStorage.removeItem('currentUser');
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    const expiration = localStorage.getTokenExpiration();

    if (!token || !expiration) {
      return false;
    }

    return new Date(expiration) > new Date();
  }
}

