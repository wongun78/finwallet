import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { StorageService } from '../core/storage.service';

export const authGuard: CanActivateFn = () => {
  const storage = inject(StorageService);
  const router = inject(Router);

  if (storage.getAccessToken()) {
    return true;
  }

  router.navigateByUrl('/login');
  return false;
};
