import { ComponentFixture, TestBed } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { FormGroupDirective } from '@angular/forms';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { MatSnackBar } from '@angular/material/snack-bar';

import { AuthService } from '../../core/services/auth.service';
import { ErrorHandlingService } from '../../core/services/error-handling.service';
import { UserProfile } from '../../shared/interfaces/auth/profile.interface';
import { Profile } from './profile';

describe('Profile', () => {
  let component: Profile;
  let fixture: ComponentFixture<Profile>;

  const profile: UserProfile = {
    id: '11111111-1111-1111-1111-111111111111',
    username: 'semih',
    email: 'semih@example.com',
    firstName: 'Semih',
    lastName: 'Ertan',
    createdAt: new Date('2026-01-01T00:00:00Z'),
    updatedAt: new Date('2026-01-01T00:00:00Z'),
    isActive: true
  };

  const authService = {
    getProfile: vi.fn(),
    updateProfile: vi.fn(),
    changePassword: vi.fn(),
    saveCurrentUser: vi.fn()
  };

  beforeEach(async () => {
    vi.clearAllMocks();
    authService.getProfile.mockReturnValue(of({
      success: true,
      message: 'Profil başarıyla getirildi.',
      data: profile
    }));
    authService.updateProfile.mockReturnValue(of({
      success: true,
      message: 'Profil bilgileriniz güncellendi.',
      data: { ...profile, firstName: 'Deniz' }
    }));
    authService.changePassword.mockReturnValue(of({
      success: true,
      message: 'Şifreniz başarıyla değiştirildi.',
      data: null
    }));

    await TestBed.configureTestingModule({
      imports: [Profile],
      providers: [
        { provide: AuthService, useValue: authService },
        {
          provide: ErrorHandlingService,
          useValue: { getErrorMessage: () => 'İşlem başarısız oldu.' }
        },
        { provide: MatSnackBar, useValue: { open: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(Profile);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('profil bilgilerini yükleyip formu pristine bırakmalı', () => {
    expect(component.profile).toEqual(profile);
    expect(component.profileForm.getRawValue()).toEqual({
      firstName: 'Semih',
      lastName: 'Ertan',
      username: 'semih',
      email: 'semih@example.com'
    });
    expect(component.profileForm.pristine).toBe(true);
  });

  it('profil formu değişmediyse güncelleme isteği göndermemeli', () => {
    component.updateProfile();

    expect(authService.updateProfile).not.toHaveBeenCalled();
  });

  it('yeni şifreler eşleşmiyorsa istek göndermemeli', () => {
    component.passwordForm.setValue({
      currentPassword: 'mevcut-sifre',
      newPassword: 'yeni-sifre',
      confirmNewPassword: 'farkli-sifre'
    });

    component.changePassword();

    expect(component.passwordForm.hasError('passwordsMismatch')).toBe(true);
    expect(authService.changePassword).not.toHaveBeenCalled();
  });

  it('başarılı şifre değişikliğinden sonra form durumunu sıfırlamalı', () => {
    component.passwordForm.setValue({
      currentPassword: 'mevcut-sifre',
      newPassword: 'yeni-sifre',
      confirmNewPassword: 'yeni-sifre'
    });
    component.passwordForm.markAllAsTouched();
    component.passwordForm.markAsDirty();

    const passwordFormElement = fixture.debugElement.query(
      By.css('.password-form')
    );
    const formDirective = passwordFormElement.injector.get(FormGroupDirective);

    formDirective.onSubmit(new Event('submit'));
    fixture.detectChanges();

    expect(component.passwordForm.getRawValue()).toEqual({
      currentPassword: '',
      newPassword: '',
      confirmNewPassword: ''
    });
    expect(component.passwordForm.pristine).toBe(true);
    expect(component.passwordForm.untouched).toBe(true);
    expect(formDirective.submitted).toBe(false);
    expect(component.isChangingPassword).toBe(false);
    expect(fixture.nativeElement.textContent).not.toContain(
      'Mevcut şifre alanı zorunludur.'
    );
  });
});
