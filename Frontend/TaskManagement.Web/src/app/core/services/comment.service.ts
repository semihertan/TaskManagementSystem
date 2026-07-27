import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreateComment } from '../../shared/interfaces/comment/create-comment.interface';
import { TaskComment } from '../../shared/interfaces/comment/task-comment.interface';
import { UpdateComment } from '../../shared/interfaces/comment/update-comment.interface';

@Injectable({
  providedIn: 'root'
})
export class CommentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/tasks`;

  getComments(taskId: string): Observable<TaskComment[]> {
    return this.http.get<TaskComment[]>(
      `${this.apiUrl}/${taskId}/comments`
    );
  }

  createComment(
    taskId: string,
    request: CreateComment
  ): Observable<TaskComment> {
    return this.http.post<TaskComment>(
      `${this.apiUrl}/${taskId}/comments`,
      request
    );
  }

  updateComment(
    taskId: string,
    commentId: string,
    request: UpdateComment
  ): Observable<TaskComment> {
    return this.http.put<TaskComment>(
      `${this.apiUrl}/${taskId}/comments/${commentId}`,
      request
    );
  }

  deleteComment(
    taskId: string,
    commentId: string
  ): Observable<void> {
    return this.http.delete<void>(
      `${this.apiUrl}/${taskId}/comments/${commentId}`
    );
  }
}