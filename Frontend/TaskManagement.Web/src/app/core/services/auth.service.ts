import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable } from 'rxjs';
import { jwtDecode } from 'jwt-decode';

import { environment } from '../../../environments/environment';

import { User } from '../../shared/interfaces/auth/user.interface';
import { LoginRequest } from '../../shared/interfaces/auth/login.interface';
import { RegisterRequest } from '../../shared/interfaces/auth/register.interface';
import {
  ChangePasswordRequest,
  UpdateProfileRequest,
  UserProfile
} from '../../shared/interfaces/auth/profile.interface';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';

import { StorageService } from './storage.service';
import { STORAGE_KEYS } from '../constants/storage-keys';

interface JwtPayload {
  exp?: number;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);
  private storageService = inject(StorageService);
  private router = inject(Router);

  private apiUrl = `${environment.apiUrl}/auth`;

  constructor() { }

  login(loginData: LoginRequest): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(
      `${this.apiUrl}/login`,
      loginData
    );
  }

  register(registerData: RegisterRequest): Observable<ApiResponse<User>> {
    return this.http.post<ApiResponse<User>>(
      `${this.apiUrl}/register`,
      registerData
    );
  }

  getProfile(): Observable<ApiResponse<UserProfile>> {
    return this.http.get<ApiResponse<UserProfile>>(
      `${this.apiUrl}/profile`
    );
  }

  updateProfile(
    request: UpdateProfileRequest
  ): Observable<ApiResponse<UserProfile>> {
    return this.http.put<ApiResponse<UserProfile>>(
      `${this.apiUrl}/profile`,
      request
    );
  }

  changePassword(
    request: ChangePasswordRequest
  ): Observable<ApiResponse<null>> {
    return this.http.put<ApiResponse<null>>(
      `${this.apiUrl}/change-password`,
      request
    );
  }

  logout(): void {
    this.storageService.removeItem(STORAGE_KEYS.accessToken);
    this.storageService.removeItem(STORAGE_KEYS.currentUser);

    this.router.navigate(['/login']);
  }

  saveToken(token: string): void {
    this.storageService.setItem(
      STORAGE_KEYS.accessToken,
      token
    );
  }

  getToken(): string | null {
    return this.storageService.getItem<string>(
      STORAGE_KEYS.accessToken
    );
  }

  saveCurrentUser(user: User): void {
    this.storageService.setItem(
      STORAGE_KEYS.currentUser,
      user
    );
  }

  getCurrentUser(): User | null {
    return this.storageService.getItem<User>(
      STORAGE_KEYS.currentUser
    );
  }

  isLoggedIn(): boolean {

    const token = this.getToken();

    if (!token) {
      return false;
    }

    try {

      const decoded = jwtDecode<JwtPayload>(token);

      if (!decoded.exp) {
        this.logout();
        return false;
      }

      const currentTime = Math.floor(Date.now() / 1000);

      if (decoded.exp <= currentTime) {

        this.logout();

        return false;
      }

      return true;

    } catch {
      this.logout();

      return false;
    }
  }
}
