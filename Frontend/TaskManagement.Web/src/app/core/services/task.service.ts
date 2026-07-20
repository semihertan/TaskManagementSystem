import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskItem } from '../../shared/interfaces/task/task.interface';
import { CreateTask } from '../../shared/interfaces/task/create-task.interface';
import { UpdateTask } from '../../shared/interfaces/task/update-task.interface';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';
import { PagedData } from '../../shared/interfaces/paged-data.interface';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tasks`;

  getTasks(): Observable<ApiResponse<PagedData<TaskItem>>> {
    return this.http.get<ApiResponse<PagedData<TaskItem>>>(this.apiUrl);
  }

  getTaskById(id: string): Observable<ApiResponse<TaskItem>> {
    return this.http.get<ApiResponse<TaskItem>>(`${this.apiUrl}/${id}`);
  }

  createTask(taskData: CreateTask): Observable<ApiResponse<TaskItem>> {
    return this.http.post<ApiResponse<TaskItem>>(this.apiUrl, taskData);
  }

  updateTask(id: string, taskData: UpdateTask): Observable<ApiResponse<TaskItem>> {
    return this.http.put<ApiResponse<TaskItem>>(`${this.apiUrl}/${id}`, taskData);
  }

  deleteTask(id: string): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`);
  }
}
