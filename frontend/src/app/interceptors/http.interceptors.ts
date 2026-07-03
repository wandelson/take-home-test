import {
  HttpErrorResponse,
  HttpEvent,
  HttpInterceptorFn,
  HttpResponse,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';
import { LoggerService } from '../core/logging/logger.service';
import { AuthService } from '../services/auth.service';
import { environment } from '../../environments/environment';

function isApiRequest(url: string): boolean {
  const apiUrl = environment.apiUrl;
  return apiUrl !== '' && (url === apiUrl || url.startsWith(`${apiUrl}/`));
}

function extractApiPath(url: string): string {
  const apiUrl = environment.apiUrl;
  if (apiUrl && url.startsWith(apiUrl)) {
    const path = url.slice(apiUrl.length);
    return path.startsWith('/') ? path : `/${path}`;
  }

  return url;
}

function shouldSkipHttpLogging(path: string): boolean {
  return path === '/health' || path.startsWith('/health?');
}

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  if (isApiRequest(req.url)) {
    return next(req.clone({ withCredentials: true }));
  }

  return next(req);
};

export const httpLoggingInterceptor: HttpInterceptorFn = (req, next) => {
  if (!environment.logging.enableHttpLogging || !isApiRequest(req.url)) {
    return next(req);
  }

  const path = extractApiPath(req.url);
  if (shouldSkipHttpLogging(path)) {
    return next(req);
  }

  const logger = inject(LoggerService);
  const started = performance.now();

  return next(req).pipe(
    tap({
      next: (event: HttpEvent<unknown>) => {
        if (event instanceof HttpResponse) {
          const durationMs = (performance.now() - started).toFixed(1);
          logger.info(
            `HTTP ${req.method} ${path} responded ${event.status} in ${durationMs} ms`,
            'HTTP'
          );
        }
      },
      error: (error: HttpErrorResponse) => {
        const durationMs = (performance.now() - started).toFixed(1);
        const status = error.status || 0;
        logger.warn(
          `HTTP ${req.method} ${path} failed ${status} in ${durationMs} ms`,
          'HTTP'
        );
      },
    })
  );
};

function extractErrorMessage(error: HttpErrorResponse): string {
  const body = error.error;
  if (typeof body?.detail === 'string' && body.detail.length > 0) {
    return body.detail;
  }

  if (body?.errors && typeof body.errors === 'object') {
    const messages = Object.values(body.errors as Record<string, string[]>)
      .flat()
      .filter((m): m is string => typeof m === 'string' && m.length > 0);
    if (messages.length > 0) {
      return messages.join(' ');
    }
  }

  if (typeof body?.title === 'string' && body.title.length > 0) {
    return body.title;
  }

  return 'An unexpected error occurred.';
}

function shouldLogHttpError(reqUrl: string, status: number): boolean {
  if (status === 401 && (reqUrl.includes('/auth/login') || reqUrl.includes('/auth/me'))) {
    return false;
  }

  return true;
}

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const logger = inject(LoggerService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/login')) {
        if (!req.url.includes('/auth/me')) {
          logger.info('Session expired; redirecting to login', 'Auth');
        }

        authService.clearSession();
        void router.navigate(['/login']);
      }

      let message = extractErrorMessage(error);

      if (error.status === 409) {
        message = 'This loan was updated by another request. Please refresh and try again.';
      }

      if (shouldLogHttpError(req.url, error.status)) {
        const path = extractApiPath(req.url);
        const logMessage = `HTTP ${error.status} ${path}: ${message}`;

        if (error.status >= 500) {
          logger.error(logMessage, 'HTTP', error.error);
        } else {
          logger.warn(logMessage, 'HTTP');
        }
      }

      return throwError(() => new Error(message));
    })
  );
};
