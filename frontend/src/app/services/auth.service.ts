import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, map, of, tap } from 'rxjs';
import { LoginRequest, LoginResponse, SessionResponse } from '../models/auth.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly authenticatedSignal = signal(false);
  private readonly expiresAtSignal = signal<string | null>(null);

  readonly isAuthenticated = computed(() => this.authenticatedSignal() && !this.isExpired());

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((response) => this.setSession(response.expiresAt))
    );
  }

  restoreSession(): Observable<boolean> {
    if (this.authenticatedSignal()) {
      return of(true);
    }

    return this.http.get<SessionResponse>(`${environment.apiUrl}/auth/me`).pipe(
      tap((response) => this.setSession(response.expiresAt)),
      map(() => true),
      catchError(() => {
        this.clearSession();
        return of(false);
      })
    );
  }

  logout(): Observable<void> {
    return this.http.post<void>(`${environment.apiUrl}/auth/logout`, {}).pipe(
      tap(() => this.clearSession()),
      catchError(() => {
        this.clearSession();
        return of(void 0);
      })
    );
  }

  clearSession(): void {
    this.authenticatedSignal.set(false);
    this.expiresAtSignal.set(null);
  }

  private setSession(expiresAt: string): void {
    this.expiresAtSignal.set(expiresAt);
    this.authenticatedSignal.set(true);
  }

  private isExpired(): boolean {
    const expiresAt = this.expiresAtSignal();
    return !expiresAt || new Date(expiresAt) <= new Date();
  }
}
