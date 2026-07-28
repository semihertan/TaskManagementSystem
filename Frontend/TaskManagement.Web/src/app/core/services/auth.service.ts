import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { finalize, Observable, tap } from 'rxjs';
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
import { UserRole } from '../../shared/interfaces/auth/user-role.enum';

interface JwtPayload {
  exp?: number;
  role?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
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
    )
    .pipe(
      tap(response => {
        if(!response.data){
          throw new Error("Sunucudan token alınamadı");
        }

        this.saveToken(response.data);
      })
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

  get currentUserRole(): UserRole | null {
    const token = this.getToken();

    if (!token) {
      return null;
    }

    try {
      const decoded = jwtDecode<JwtPayload>(token);
      const role = decoded.role ??
        decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

      if (role === 'Admin' || role === String(UserRole.Admin)) {
        return UserRole.Admin;
      }

      if (role === 'User' || role === String(UserRole.User)) {
        return UserRole.User;
      }

      return null;
    } catch {
      return null;
    }
  }

  isAdmin(): boolean {
    return this.isLoggedIn() && this.currentUserRole === UserRole.Admin;
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
