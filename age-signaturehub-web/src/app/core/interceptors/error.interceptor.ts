import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { AuthService } from "../services/auth.service";
import { catchError, throwError } from "rxjs";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const isWindowsSsoEndpoint = req.url.includes('/auth/windows-sso');

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isWindowsSsoEndpoint) {
        authService.handleUnauthorized();
      }

      if (error.status === 403) {
        console.warn('Access denied. You do not have permission to access this resource.');
      }
      return throwError(() => error);
    })
  )
}
