import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of } from 'rxjs';
import { LoansComponent } from './loans.component';
import { AuthService } from '../../services/auth.service';
import { LoanService } from '../../services/loan.service';

describe('LoansComponent', () => {
  let fixture: ComponentFixture<LoansComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoansComponent, NoopAnimationsModule],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: AuthService,
          useValue: {
            isAuthenticated: () => true,
            logout: jasmine.createSpy('logout').and.returnValue(of(void 0)),
          },
        },
        {
          provide: LoanService,
          useValue: {
            getLoans: () =>
              of([
                {
                  id: '1',
                  amount: 1000,
                  currentBalance: 500,
                  applicantName: 'Maria Silva',
                  status: 'active' as const,
                  createdAt: '2026-01-01T00:00:00Z',
                },
              ]),
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(LoansComponent);
    fixture.detectChanges();
  });

  it('should render loan table with seeded applicant', async () => {
    fixture.detectChanges();
    await fixture.whenStable();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Maria Silva');
    expect(element.querySelector('[data-testid="create-submit"]')).toBeTruthy();
  });
});
