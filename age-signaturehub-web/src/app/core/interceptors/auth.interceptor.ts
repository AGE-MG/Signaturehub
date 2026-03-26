import { inject } from "@angular/core";
import { AuthService } from "../services/auth.service";
import { HttpInterceptorFn } from "@angular/common/http";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const isAuthEndpoint = req.url.includes('/auth/login') || req.url.includes('/auth/register');

  if (token && !isAuthEndpoint) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  return next(req);

}
