import { inject } from "@angular/core";
import { AuthService } from "../services/auth.service";

export const authInterceptor = (req, next) => {
  const authService = inject(AuthService);
  |
}
