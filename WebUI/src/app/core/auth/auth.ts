import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest } from './models/login-request.model';
import { LoginResponse } from './models/login-response.model';

const TOKEN_STORAGE_KEY = 'pma.auth.token';
const DISPLAY_NAME_STORAGE_KEY = 'pma.auth.displayName';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly http = inject(HttpClient);

  private readonly tokenSignal = signal<string | null>(localStorage.getItem(TOKEN_STORAGE_KEY));
  private readonly displayNameSignal = signal<string | null>(
    localStorage.getItem(DISPLAY_NAME_STORAGE_KEY),
  );

  readonly isAuthenticated = computed(() => this.tokenSignal() !== null);
  readonly displayName = computed(() => this.displayNameSignal());

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${environment.apiBaseUrl}/auth/login`, request)
      .pipe(
        tap((response) => {
          localStorage.setItem(TOKEN_STORAGE_KEY, response.token);
          localStorage.setItem(DISPLAY_NAME_STORAGE_KEY, response.displayName);
          this.tokenSignal.set(response.token);
          this.displayNameSignal.set(response.displayName);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    localStorage.removeItem(DISPLAY_NAME_STORAGE_KEY);
    this.tokenSignal.set(null);
    this.displayNameSignal.set(null);
  }

  getToken(): string | null {
    return this.tokenSignal();
  }
}
