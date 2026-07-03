import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LoanService } from './loan.service';
import { environment } from '../../environments/environment';

describe('LoanService', () => {
  let service: LoanService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LoanService],
    });

    service = TestBed.inject(LoanService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch loans', () => {
    const mockLoans = [
      {
        id: '1',
        amount: 1000,
        currentBalance: 500,
        applicantName: 'Test',
        status: 'active' as const,
        createdAt: '2026-01-01T00:00:00Z',
      },
    ];

    service.getLoans().subscribe((loans) => {
      expect(loans).toEqual(mockLoans);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/loans`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLoans);
  });

  it('should create a loan', () => {
    const payload = { amount: 2000, applicantName: 'Jane Doe' };
    const mockLoan = { id: '2', ...payload, currentBalance: 2000, status: 'active' as const, createdAt: '2026-01-01T00:00:00Z' };

    service.createLoan(payload).subscribe((loan) => {
      expect(loan.applicantName).toBe('Jane Doe');
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/loans`);
    expect(req.request.method).toBe('POST');
    req.flush(mockLoan);
  });

  it('should record a payment', () => {
    const loanId = 'abc-123';
    const payload = { amount: 100 };
    const mockLoan = {
      id: loanId,
      amount: 1000,
      currentBalance: 400,
      applicantName: 'Jane',
      status: 'active' as const,
      createdAt: '2026-01-01T00:00:00Z',
    };

    service.recordPayment(loanId, payload).subscribe((loan) => {
      expect(loan.currentBalance).toBe(400);
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/loans/${loanId}/payment`);
    expect(req.request.method).toBe('POST');
    req.flush(mockLoan);
  });
});
