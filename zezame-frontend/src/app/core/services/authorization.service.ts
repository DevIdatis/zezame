import { Injectable, inject, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';

import { AuthService } from './auth.service';
import { OrganizationRole } from '../enum/organization-role.enum';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private readonly authService = inject(AuthService);

  private readonly currentUser = toSignal(
    this.authService.currentUser$,
    { initialValue: null }
  );

  private readonly currentRole = computed(
    () => this.currentUser()?.role ?? null
  );

  getRole(): OrganizationRole | null {
    return this.currentRole();
  }

  hasRole(role: OrganizationRole): boolean {
    return this.currentRole() === role;
  }

  hasAnyRole(roles: OrganizationRole[]): boolean {
    const role = this.currentRole();

    return role !== null && roles.includes(role);
  }

  hasMinimumRole(requiredRole: OrganizationRole): boolean {
    const role = this.currentRole();

    return role !== null && role >= requiredRole;
  }

  isOwner(): boolean {
    return this.hasRole(OrganizationRole.Owner);
  }

  canManageUsers(): boolean {
    return this.hasMinimumRole(OrganizationRole.Admin);
  }
}