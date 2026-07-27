import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, shareReplay, tap } from 'rxjs';

import { environment } from '../../../environments/environment';

import { Category } from '../../shared/interfaces/category/category.interface';
import { CreateCategory } from '../../shared/interfaces/category/create-category.interface';
import { UpdateCategory } from '../../shared/interfaces/category/update-category.interface';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';

@Injectable({
  providedIn: 'root'
})
export class CategoryService {

  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/categories`;

  private categoriesCache$:
    Observable<ApiResponse<Category[]>> | null = null;

  getCategories(
    forceRefresh = false
  ): Observable<ApiResponse<Category[]>> {

    if (forceRefresh) {
      this.clearCache();
    }

    if (!this.categoriesCache$) {
      this.categoriesCache$ = this.http
        .get<ApiResponse<Category[]>>(this.apiUrl)
        .pipe(
          shareReplay({
            bufferSize: 1,
            refCount: false
          })
        );
    }

    return this.categoriesCache$;
  }

  getCategoryById(
    id: string
  ): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(
      `${this.apiUrl}/${id}`
    );
  }

  createCategory(
    category: CreateCategory
  ): Observable<ApiResponse<Category>> {
    return this.http
      .post<ApiResponse<Category>>(
        this.apiUrl,
        category
      )
      .pipe(
        tap(() => this.clearCache())
      );
  }

  updateCategory(
    id: string,
    category: UpdateCategory
  ): Observable<ApiResponse<Category>> {
    return this.http
      .put<ApiResponse<Category>>(
        `${this.apiUrl}/${id}`,
        category
      )
      .pipe(
        tap(() => this.clearCache())
      );
  }

  deleteCategory(
    id: string
  ): Observable<ApiResponse<null>> {
    return this.http
      .delete<ApiResponse<null>>(
        `${this.apiUrl}/${id}`
      )
      .pipe(
        tap(() => this.clearCache())
      );
  }

  clearCache(): void {
    this.categoriesCache$ = null;
  }
}