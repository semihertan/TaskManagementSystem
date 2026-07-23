import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AuthService } from '../../../core/services/auth.service';
import { ErrorHandlingService } from '../../../core/services/error-handling.service';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,

    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  isLoading = false;
  errorMessage = '';

  private fb = inject(FormBuilder);

  private authService = inject(AuthService);
  private errorHandlingService = inject(ErrorHandlingService);
  
  private router = inject(Router);

  loginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

onSubmit(): void {
  if (this.loginForm.invalid) {
    this.loginForm.markAllAsTouched();
    return;
  }

  this.errorMessage = '';
  this.isLoading = true;

  const loginData = this.loginForm.getRawValue();

  this.authService.login(loginData).subscribe({
    next: (response) => {
      this.authService.saveToken(response.data);
      this.router.navigate(['/dashboard']);
    },

    error: (error) => {
      this.isLoading = false;

      this.errorMessage = this.errorHandlingService.getErrorMessage(error);

      console.error(error);
    },

    complete: () => {
      this.isLoading = false;
    }
  });
  }
}