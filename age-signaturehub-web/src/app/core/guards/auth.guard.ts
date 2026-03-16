import { inject } from "@angular/core";
import { CanActivateChildFn, Router } from "@angular/router";

export const authGuard: CanActivateChildFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }


  return true;

}
