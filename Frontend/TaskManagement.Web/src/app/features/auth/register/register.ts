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
  selector: 'app-register',
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
  templateUrl: './register.html',
  styleUrl: './register.scss'
})

export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private errorHandlingService = inject(ErrorHandlingService);
  private router = inject(Router);

  isLoading = false;
  errorMessage = '';

  registerForm = this.fb.nonNullable.group({
  username: ['', [Validators.required]],
  firstName: ['', [Validators.required]],
  lastName: ['', [Validators.required]],
  email: ['', [Validators.required, Validators.email]],
  password: ['', [Validators.required, Validators.minLength(6)]]
  });

  onSubmit(): void {
  if (this.registerForm.invalid) {
    this.registerForm.markAllAsTouched();
    return;
  }

  this.isLoading = true;
  this.errorMessage = '';

  const registerData = this.registerForm.getRawValue();

  this.authService.register(registerData).subscribe({
    next: (response) => {
      console.log(response);

      this.isLoading = false;
      this.router.navigate(['/login']);
    },

    error: (error) => {
      this.isLoading = false;

      this.errorMessage =
        this.errorHandlingService.getErrorMessage(error);
      console.log('Ekrana yazılacak mesaj:', this.errorMessage);
      console.error(error);
    }
  });
  }
}