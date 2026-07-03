import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { environment } from '../../../environments/environment';
import { fieldError } from '../../core/forms/form-field-error';
import { requiredNotWhitespace } from '../../core/forms/form-validators';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  readonly environment = environment;

  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);

  readonly pending = signal(false);
  loginError: string | null = null;

  loginForm = this.fb.group({
    username: [environment.production ? '' : 'admin', requiredNotWhitespace],
    password: [environment.production ? '' : 'admin123', requiredNotWhitespace],
  });

  usernameError(): string | null {
    return fieldError(this.loginForm.controls.username, {
      required: 'Username is required.',
    });
  }

  passwordError(): string | null {
    return fieldError(this.loginForm.controls.password, {
      required: 'Password is required.',
    });
  }

  login(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const { username, password } = this.loginForm.getRawValue();
    if (!username || !password) {
      return;
    }

    this.pending.set(true);
    this.loginError = null;

    this.auth
      .login({ username: username.trim(), password: password.trim() })
      .pipe(finalize(() => this.pending.set(false)))
      .subscribe({
        next: () => this.router.navigate(['/loans']),
        error: (err: Error) => {
          this.loginError = err.message || 'Invalid username or password.';
        },
      });
  }
}
