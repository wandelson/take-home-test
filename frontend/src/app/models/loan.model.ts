export interface Loan {
  id: string;
  amount: number;
  currentBalance: number;
  applicantName: string;
  status: 'active' | 'paid';
  createdAt: string;
}

export interface CreateLoanRequest {
  amount: number;
  applicantName: string;
}

export interface PaymentRequest {
  amount: number;
}
