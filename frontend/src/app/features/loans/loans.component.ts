import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { catchError, finalize, map, of, startWith, switchMap, BehaviorSubject } from 'rxjs';
import { LoanService } from '../../services/loan.service';
import { AuthService } from '../../services/auth.service';
import { Loan } from '../../models/loan.model';
import { fieldError } from '../../core/forms/form-field-error';
import { requiredNotWhitespace } from '../../core/forms/form-validators';

interface LoansState {
  loading: boolean;
  loans: Loan[];
  error: string | null;
}

@Component({
  selector: 'app-loans',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './loans.component.html',
  styleUrls: ['./loans.component.scss'],
})
export class LoansComponent {
  readonly auth = inject(AuthService);
  private readonly loanService = inject(LoanService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly refresh$ = new BehaviorSubject<void>(undefined);

  readonly createPending = signal(false);
  readonly paymentPending = signal<string | null>(null);

  createForm = this.fb.group({
    amount: [null as number | null, [requiredNotWhitespace, Validators.min(0.01)]],
    applicantName: ['', [requiredNotWhitespace, Validators.maxLength(200)]],
  });

  displayedColumns = ['loanAmount', 'currentBalance', 'applicant', 'status', 'actions'];
  paymentAmounts: Record<string, number | null> = {};
  paymentFieldErrors: Record<string, string | null> = {};
  actionError: string | null = null;
  actionSuccess: string | null = null;

  amountError(): string | null {
    return fieldError(this.createForm.controls.amount, {
      required: 'Amount is required.',
      min: 'Amount must be greater than zero.',
    });
  }

  applicantNameError(): string | null {
    return fieldError(this.createForm.controls.applicantName, {
      required: 'Applicant name is required.',
      maxlength: 'Applicant name cannot exceed 200 characters.',
    });
  }

  readonly loansState$ = this.refresh$.pipe(
    switchMap(() =>
      this.loanService.getLoans().pipe(
        map(
          (loans): LoansState => ({
            loading: false,
            loans,
            error: null,
          })
        ),
        catchError((err: Error) =>
          of({
            loading: false,
            loans: [],
            error: err.message,
          })
        ),
        startWith({ loading: true, loans: [], error: null })
      )
    )
  );

  logout(): void {
    this.auth.logout().subscribe(() => this.router.navigate(['/login']));
  }

  createLoan(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const { amount, applicantName } = this.createForm.getRawValue();
    if (amount == null || !applicantName) {
      return;
    }

    this.createPending.set(true);
    this.actionError = null;
    this.actionSuccess = null;

    this.loanService
      .createLoan({ amount, applicantName: applicantName.trim() })
      .pipe(finalize(() => this.createPending.set(false)))
      .subscribe({
        next: () => {
          this.createForm.reset();
          this.actionSuccess = 'Loan created successfully.';
          this.refresh();
        },
        error: (err: Error) => {
          this.actionError = err.message;
        },
      });
  }

  recordPayment(loan: Loan): void {
    const amount = this.paymentAmounts[loan.id];
    if (amount == null || amount <= 0) {
      this.paymentFieldErrors[loan.id] = 'Payment amount is required.';
      this.actionError = null;
      return;
    }

    this.paymentFieldErrors[loan.id] = null;

    this.paymentPending.set(loan.id);
    this.actionError = null;
    this.actionSuccess = null;

    this.loanService
      .recordPayment(loan.id, { amount })
      .pipe(finalize(() => this.paymentPending.set(null)))
      .subscribe({
        next: () => {
          this.paymentAmounts[loan.id] = null;
          this.actionSuccess = `Payment recorded for ${loan.applicantName}.`;
          this.refresh();
        },
        error: (err: Error) => {
          this.actionError = err.message;
        },
      });
  }

  private refresh(): void {
    this.refresh$.next();
  }
}
