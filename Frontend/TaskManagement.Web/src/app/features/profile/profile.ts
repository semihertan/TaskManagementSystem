import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroupDirective,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

import { finalize } from 'rxjs';

import { AuthService } from '../../core/services/auth.service';
import { ErrorHandlingService } from '../../core/services/error-handling.service';
import {
  ChangePasswordRequest,
  UpdateProfileRequest,
  UserProfile
} from '../../shared/interfaces/auth/profile.interface';

const passwordsMatchValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  const newPassword = control.get('newPassword')?.value as string | undefined;
  const confirmNewPassword = control.get('confirmNewPassword')?.value as
    | string
    | undefined;

  return newPassword === confirmNewPassword
    ? null
    : { passwordsMismatch: true };
};

@Component({
  selector: 'app-profile',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './profile.html',
  styleUrl: './profile.scss'
})
export class Profile implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly errorHandlingService = inject(ErrorHandlingService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly destroyRef = inject(DestroyRef);
  private readonly cdr = inject(ChangeDetectorRef);

  profile: UserProfile | null = null;
  isLoading = true;
  isUpdating = false;
  isChangingPassword = false;
  profileError = '';
  passwordError = '';

  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  readonly profileForm = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(50)]],
    lastName: ['', [Validators.required, Validators.maxLength(50)]],
    username: ['', [Validators.required, Validators.maxLength(50)]],
    email: [
      '',
      [Validators.required, Validators.email, Validators.maxLength(100)]
    ]
  });

  readonly passwordForm = this.fb.nonNullable.group(
    {
      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmNewPassword: ['', Validators.required]
    },
    { validators: passwordsMatchValidator }
  );

  ngOnInit(): void {
    this.loadProfile();
  }

  get initials(): string {
    if (!this.profile) {
      return '';
    }

    const firstInitial = this.profile.firstName.trim().charAt(0);
    const lastInitial = this.profile.lastName.trim().charAt(0);
    const initials = `${firstInitial}${lastInitial}` ||
      this.profile.username.trim().charAt(0);

    return initials.toLocaleUpperCase('tr-TR');
  }

  get fullName(): string {
    if (!this.profile) {
      return '';
    }

    return `${this.profile.firstName} ${this.profile.lastName}`.trim();
  }

  get profileUnchanged(): boolean {
    if (!this.profile) {
      return true;
    }

    const value = this.profileForm.getRawValue();

    return value.firstName.trim() === this.profile.firstName &&
      value.lastName.trim() === this.profile.lastName &&
      value.username.trim() === this.profile.username &&
      value.email.trim() === this.profile.email;
  }

  loadProfile(): void {
    this.isLoading = true;
    this.profileError = '';

    this.authService
      .getProfile()
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (response) => {
          this.applyProfile(response.data);
        },
        error: (error: unknown) => {
          this.profileError = this.errorHandlingService.getErrorMessage(error);
        }
      });
  }

  updateProfile(): void {
    if (this.profileForm.invalid || this.profileUnchanged) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isUpdating = true;
    this.profileError = '';

    const formValue = this.profileForm.getRawValue();
    const request: UpdateProfileRequest = {
      firstName: formValue.firstName.trim(),
      lastName: formValue.lastName.trim(),
      username: formValue.username.trim(),
      email: formValue.email.trim()
    };

    this.authService
      .updateProfile(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isUpdating = false;
        })
      )
      .subscribe({
        next: (response) => {
          this.applyProfile(response.data);
          this.snackBar.open(
            'Profil bilgileriniz güncellendi.',
            'Kapat',
            { duration: 3000 }
          );
        },
        error: (error: unknown) => {
          this.profileError = this.errorHandlingService.getErrorMessage(error);
        }
      });
  }

  changePassword(formDirective?: FormGroupDirective): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isChangingPassword = true;
    this.passwordError = '';

    const request: ChangePasswordRequest = this.passwordForm.getRawValue();

    this.authService
      .changePassword(request)
      .pipe(
        takeUntilDestroyed(this.destroyRef),
        finalize(() => {
          this.isChangingPassword = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: () => {
          const emptyForm = {
            currentPassword: '',
            newPassword: '',
            confirmNewPassword: ''
          };

          if (formDirective) {
            formDirective.resetForm(emptyForm);
          } else {
            this.passwordForm.reset(emptyForm);
          }

          this.passwordForm.markAsPristine();
          this.passwordForm.markAsUntouched();
          this.passwordForm.updateValueAndValidity({ emitEvent: false });
          this.hideCurrentPassword = true;
          this.hideNewPassword = true;
          this.hideConfirmPassword = true;
          this.snackBar.open(
            'Şifreniz başarıyla değiştirildi.',
            'Kapat',
            { duration: 3000 }
          );
        },
        error: (error: unknown) => {
          this.passwordError = this.errorHandlingService.getErrorMessage(error);
        }
      });
  }

  private applyProfile(profile: UserProfile): void {
    this.profile = profile;
    this.authService.saveCurrentUser(profile);
    this.profileForm.reset({
      firstName: profile.firstName,
      lastName: profile.lastName,
      username: profile.username,
      email: profile.email
    });
  }
}
