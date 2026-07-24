import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
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

  getCategories(): Observable<ApiResponse<Category[]>> {
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl);
  }

  getCategoryById(id: string): Observable<ApiResponse<Category>> {
    return this.http.get<ApiResponse<Category>>(`${this.apiUrl}/${id}`);
  }

  createCategory(categoryData: CreateCategory): Observable<ApiResponse<Category>> {
    return this.http.post<ApiResponse<Category>>(this.apiUrl, categoryData);
  }

  updateCategory(
      id: string,
      categoryData: UpdateCategory
  ): Observable<ApiResponse<Category>> {
      return this.http.put<ApiResponse<Category>>(
          `${this.apiUrl}/${id}`,
          categoryData
      );
  }

  deleteCategory(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
