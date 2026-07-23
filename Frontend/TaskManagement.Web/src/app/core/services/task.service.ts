import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { TaskItem } from '../../shared/interfaces/task/task.interface';
import { CreateTask } from '../../shared/interfaces/task/create-task.interface';
import { UpdateTask } from '../../shared/interfaces/task/update-task.interface';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';
import { PagedData } from '../../shared/interfaces/paged-data.interface';
import { TaskStatistics } from '../../shared/interfaces/task/task-statistics.interface';
import { TaskFilter } from '../../shared/interfaces/task/task-filter.interface';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tasks`;

  getTasks(
    filter: TaskFilter = {}
  ): Observable<ApiResponse<PagedData<TaskItem>>> {
    let params = new HttpParams();

    if (filter.search?.trim()) {
      params = params.set('search', filter.search.trim());
    }

    if (filter.priority !== undefined) {
      params = params.set(
        'priority',
        filter.priority.toString()
      );
    }

    if (filter.status !== undefined) {
      params = params.set(
        'status',
        filter.status.toString()
      );
    }

    if (filter.categoryId) {
      params = params.set(
        'categoryId',
        filter.categoryId
      );
    }

    if (filter.dueDateFrom) {
      params = params.set(
        'dueDateFrom',
        filter.dueDateFrom
      );
    }

    if (filter.dueDateTo) {
      params = params.set(
        'dueDateTo',
        filter.dueDateTo
      );
    }

    params = params.set(
      'page',
      (filter.page ?? 1).toString()
    );

    params = params.set(
      'pageSize',
      (filter.pageSize ?? 10).toString()
    );

    return this.http.get<
      ApiResponse<PagedData<TaskItem>>
    >(this.apiUrl, { params });
  }

  getTaskById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.apiUrl}/${id}`);
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

  getStatistics(): Observable<ApiResponse<TaskStatistics>> {
    return this.http.get<ApiResponse<TaskStatistics>>(`${this.apiUrl}/statistics`);
  }
}
