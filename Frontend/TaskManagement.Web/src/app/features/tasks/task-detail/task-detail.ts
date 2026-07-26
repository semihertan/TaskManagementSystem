import { CommonModule } from '@angular/common';
import {
  ChangeDetectorRef,
  Component,
  OnInit,
  inject
} from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';

import { AttachmentService } from '../../../core/services/attachment.service';
import { TaskAttachment } from '../../../shared/interfaces/task/task-attachment.interface';
import { TaskService } from '../../../core/services/task.service';
import { TaskItem } from '../../../shared/interfaces/task/task.interface';
import { PriorityBadge } from '../../../shared/components/priority-badge/priority-badge';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatProgressSpinnerModule,
    PriorityBadge
  ],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.scss'
})
export class TaskDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);
  private attachmentService = inject(AttachmentService);
  private snackBar = inject(MatSnackBar);

  attachments: TaskAttachment[] = [];
  deletingAttachmentId: string | null = null;
  downloadingAttachmentId: string | null = null;
  isUploading = false;
  isAttachmentsLoading = false;

  task: TaskItem | null = null;
  isLoading = false;
  selectedFile: File | null = null;
  
  errorMessage = '';

  ngOnInit(): void {
    const taskId = this.route.snapshot.paramMap.get('id');

    if (!taskId) {
      this.errorMessage = 'Görev kimliği bulunamadı.';
      return;
    }

    this.loadTask(taskId);
  }

  loadTask(taskId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.taskService.getTaskById(taskId).subscribe({
      next: (response) => {
          console.log('Task detail response:', response);

          this.task = response.data;
          this.isLoading = false;

          this.loadAttachments();

          this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Görev detayı alınamadı:', error);

        this.errorMessage =
          error?.error?.message ??
          'Görev detayı yüklenirken bir hata oluştu.';

        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
  }

  getStatusText(): string {
    const statusLabels: Record<number, string> = {
      0: 'Bekliyor',
      1: 'Devam Ediyor',
      2: 'Tamamlandı',
      3: 'İptal Edildi'
    };

    return statusLabels[this.task?.status ?? -1] ?? 'Bilinmiyor';
  }

  getStatusClass(): string {
    const statusClasses: Record<number, string> = {
      0: 'status-pending',
      1: 'status-progress',
      2: 'status-completed',
      3: 'status-cancelled'
    };

    return statusClasses[this.task?.status ?? -1] ?? '';
  }

  loadAttachments(): void {
    if (!this.task) {
      return;
    }

    this.isAttachmentsLoading = true;

    this.attachmentService
      .getAttachments(this.task.id)
      .subscribe({
        next: (attachments) => {
          this.attachments = attachments ?? [];
          this.isAttachmentsLoading = false;
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Dosya ekleri alınamadı:', error);

          this.attachments = [];
          this.isAttachmentsLoading = false;

          this.snackBar.open(
            'Dosya ekleri yüklenemedi.',
            'Kapat',
            {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      this.selectedFile = null;
      return;
    }

    const file = input.files[0];

    const maximumFileSize = 10 * 1024 * 1024;

    if (file.size > maximumFileSize) {
      this.selectedFile = null;
      input.value = '';

      this.snackBar.open(
        'Dosya boyutu en fazla 10 MB olabilir.',
        'Kapat',
        {
          duration: 3500,
          horizontalPosition: 'right',
          verticalPosition: 'top'
        }
      );

      return;
    }

    this.selectedFile = file;
  }

  clearSelectedFile(fileInput?: HTMLInputElement): void {
    this.selectedFile = null;

    if (fileInput) {
      fileInput.value = '';
    }
  }

  uploadFile(fileInput: HTMLInputElement): void {
    if (!this.task || !this.selectedFile || this.isUploading) {
      return;
    }

    this.isUploading = true;

    this.attachmentService
      .uploadAttachment(
        this.task.id,
        this.selectedFile
      )
      .subscribe({
        next: () => {
          this.clearSelectedFile(fileInput);
          this.isUploading = false;

          this.loadAttachments();

          this.snackBar.open(
            'Dosya başarıyla yüklendi.',
            'Kapat',
            {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Dosya yüklenemedi:', error);

          this.isUploading = false;

          this.snackBar.open(
            error?.error?.message ??
              'Dosya yüklenirken bir hata oluştu.',
            'Kapat',
            {
              duration: 4000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  downloadAttachment(attachment: TaskAttachment): void {
    if (!this.task || this.downloadingAttachmentId) {
      return;
    }

    this.downloadingAttachmentId = attachment.id;

    this.attachmentService
      .downloadAttachment(
        this.task.id,
        attachment.id
      )
      .subscribe({
        next: (blob) => {
          const downloadUrl = URL.createObjectURL(blob);

          const link = document.createElement('a');

          link.href = downloadUrl;
          link.download = attachment.fileName;

          document.body.appendChild(link);
          link.click();
          link.remove();

          URL.revokeObjectURL(downloadUrl);

          this.downloadingAttachmentId = null;
          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Dosya indirilemedi:', error);

          this.downloadingAttachmentId = null;

          this.snackBar.open(
            'Dosya indirilemedi.',
            'Kapat',
            {
              duration: 3500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  deleteAttachment(attachment: TaskAttachment): void {
    if (!this.task || this.deletingAttachmentId) {
      return;
    }

    const confirmed = window.confirm(
      `"${attachment.fileName}" dosyasını silmek istediğine emin misin?`
    );

    if (!confirmed) {
      return;
    }

    this.deletingAttachmentId = attachment.id;

    this.attachmentService
      .deleteAttachment(
        this.task.id,
        attachment.id
      )
      .subscribe({
        next: () => {
          this.attachments = this.attachments.filter(
            item => item.id !== attachment.id
          );

          this.deletingAttachmentId = null;

          this.snackBar.open(
            'Dosya başarıyla silindi.',
            'Kapat',
            {
              duration: 3000,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error('Dosya silinemedi:', error);

          this.deletingAttachmentId = null;

          this.snackBar.open(
            error?.error?.message ??
              'Dosya silinirken bir hata oluştu.',
            'Kapat',
            {
              duration: 3500,
              horizontalPosition: 'right',
              verticalPosition: 'top'
            }
          );

          this.cdr.markForCheck();
        }
      });
  }

  formatFileSize(sizeInBytes: number): string {
    if (!sizeInBytes) {
      return '0 B';
    }

    const units = ['B', 'KB', 'MB', 'GB'];

    const unitIndex = Math.floor(
      Math.log(sizeInBytes) / Math.log(1024)
    );

    const size = sizeInBytes / Math.pow(1024, unitIndex);

    return `${size.toFixed(unitIndex === 0 ? 0 : 1)} ${units[unitIndex]}`;
  }

  getFileIcon(attachment: TaskAttachment): string {
    const fileName = attachment.fileName.toLowerCase();
    const contentType = attachment.contentType?.toLowerCase() ?? '';

    if (
      contentType.includes('image') ||
      /\.(png|jpg|jpeg|gif|webp|svg)$/.test(fileName)
    ) {
      return 'image';
    }

    if (
      contentType.includes('pdf') ||
      fileName.endsWith('.pdf')
    ) {
      return 'picture_as_pdf';
    }

    if (
      contentType.includes('word') ||
      /\.(doc|docx)$/.test(fileName)
    ) {
      return 'description';
    }

    if (
      contentType.includes('sheet') ||
      contentType.includes('excel') ||
      /\.(xls|xlsx|csv)$/.test(fileName)
    ) {
      return 'table_chart';
    }

    if (
      contentType.includes('zip') ||
      /\.(zip|rar|7z)$/.test(fileName)
    ) {
      return 'folder_zip';
    }

    return 'insert_drive_file';
  }
}
