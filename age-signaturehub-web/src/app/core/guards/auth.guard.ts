import { inject } from "@angular/core";
import { CanActivateChildFn, Router } from "@angular/router";

export const authGuard: CanActivateChildFn = (route, state) => {



  return true;

}
