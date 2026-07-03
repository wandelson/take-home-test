import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { environment } from '../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService],
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should mark session authenticated after login', () => {
    const expiresAt = new Date(Date.now() + 3600000).toISOString();

    service.login({ username: 'admin', password: 'admin123' }).subscribe((response) => {
      expect(response.expiresAt).toBe(expiresAt);
      expect(service.isAuthenticated()).toBeTrue();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/auth/login`);
    req.flush({ expiresAt });
  });

  it('should clear session on logout', () => {
    const expiresAt = new Date(Date.now() + 3600000).toISOString();

    service.login({ username: 'admin', password: 'admin123' }).subscribe();
    httpMock.expectOne(`${environment.apiUrl}/auth/login`).flush({ expiresAt });

    service.logout().subscribe();
    const req = httpMock.expectOne(`${environment.apiUrl}/auth/logout`);
    req.flush(null);

    expect(service.isAuthenticated()).toBeFalse();
  });
});
