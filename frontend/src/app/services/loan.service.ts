import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateLoanRequest, Loan, PaymentRequest } from '../models/loan.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoanService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/loans`;

  getLoans(): Observable<Loan[]> {
    return this.http.get<Loan[]>(this.apiUrl);
  }

  createLoan(request: CreateLoanRequest): Observable<Loan> {
    return this.http.post<Loan>(this.apiUrl, request);
  }

  recordPayment(loanId: string, request: PaymentRequest): Observable<Loan> {
    const headers = new HttpHeaders({
      'Idempotency-Key': crypto.randomUUID(),
    });
    return this.http.post<Loan>(`${this.apiUrl}/${loanId}/payment`, request, { headers });
  }
}
