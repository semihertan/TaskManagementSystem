import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../shared/interfaces/api-response.interface';
import { TaskAttachment } from '../../shared/interfaces/task/task-attachment.interface';

@Injectable({
  providedIn: 'root'
})
export class AttachmentService {
  private http = inject(HttpClient);

  private apiUrl = `${environment.apiUrl}/tasks`;

  uploadAttachment(
    taskId: string,
    file: File
  ): Observable<ApiResponse<TaskAttachment>> {
    const formData = new FormData();

    formData.append('file', file);

    return this.http.post<ApiResponse<TaskAttachment>>(
      `${this.apiUrl}/${taskId}/attachments`,
      formData
    );
  }

    getAttachments(
    taskId: string
    ): Observable<TaskAttachment[]> {
    return this.http.get<TaskAttachment[]>(
        `${this.apiUrl}/${taskId}/attachments`
    );
    }

  deleteAttachment(
    taskId: string,
    attachmentId: string
  ): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.apiUrl}/${taskId}/attachments/${attachmentId}`
    );
  }

  downloadAttachment(
    taskId: string,
    attachmentId: string
  ): Observable<Blob> {
    return this.http.get(
      `${this.apiUrl}/${taskId}/attachments/${attachmentId}/download`,
      {
        responseType: 'blob'
      }
    );
  }
}