import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';

import { User } from '../../shared/interfaces/user.interface';
import { LoginRequest } from '../../shared/interfaces/login.interface';
import { RegisterRequest } from '../../shared/interfaces/register.interface';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private http = inject(HttpClient);

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

  getProfile(): Observable<ApiResponse<User>> {
  return this.http.get<ApiResponse<User>>(
    `${this.apiUrl}/profile`
  );
  }

  logout(): void {
  localStorage.removeItem('token');
  }

  saveToken(token: string): void {
  localStorage.setItem('token', token);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }
}