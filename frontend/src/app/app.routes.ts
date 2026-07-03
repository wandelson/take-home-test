import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { LoginComponent } from './features/login/login.component';
import { LoansComponent } from './features/loans/loans.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'loans' },
  { path: 'login', component: LoginComponent },
  { path: 'loans', canActivate: [authGuard], component: LoansComponent },
  { path: '**', redirectTo: 'loans' },
];
