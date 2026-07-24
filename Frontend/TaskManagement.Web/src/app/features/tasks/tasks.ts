import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

import { TaskService } from '../../core/services/task.service';
import { TaskItem } from '../../shared/interfaces/task/task.interface';
import { TaskCard } from '../../shared/components/task-card/task-card';
import { TaskFilter } from '../../shared/interfaces/task/task-filter.interface';
import { TaskForm } from './task-form/task-form';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';

import {
  ConfirmDialog,
  ConfirmDialogData
} from '../../shared/components/confirm-dialog/confirm-dialog';

import {
  CdkDragDrop,
  DragDropModule,
  transferArrayItem
} from '@angular/cdk/drag-drop';

import { UpdateTask } from '../../shared/interfaces/task/update-task.interface';

@Component({
  selector: 'app-tasks',
  imports: [
    CommonModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    TaskCard,
    MatDialogModule,
    MatButtonModule,
    MatSnackBarModule,
    MatPaginatorModule,
    DragDropModule
  ],
  templateUrl: './tasks.html',
  styleUrl: './tasks.scss',
})
export class Tasks implements OnInit {
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  tasks: TaskItem[] = [];
  isLoading = false;
  errorMessage = '';
  searchText = '';
  selectedPriority: number | null = null;
  selectedStatus: number | null = null;
  dueDateFrom: Date | null = null;
  dueDateTo: Date | null = null;
  sortBy = 'createdAt';
  sortDirection: 'asc' | 'desc' = 'desc';
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [5, 10, 20, 50];

  priorities = [
    { value: 1, label: 'Çok Düşük' },
    { value: 2, label: 'Düşük' },
    { value: 3, label: 'Normal' },
    { value: 4, label: 'Yüksek' },
    { value: 5, label: 'Çok Yüksek' }
  ];

  statuses = [
    { value: 0, label: 'Bekliyor' },
    { value: 1, label: 'Devam Ediyor' },
    { value: 2, label: 'Tamamlandı' },
    { value: 3, label: 'İptal Edildi' }
  ];

  pendingTasks: TaskItem[] = [];
  inProgressTasks: TaskItem[] = [];
  completedTasks: TaskItem[] = [];
  cancelledTasks: TaskItem[] = [];

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const filters: TaskFilter = {
      search: this.searchText.trim() || undefined,
      priority: this.selectedPriority ?? undefined,
      status: this.selectedStatus ?? undefined,

      dueDateFrom: this.dueDateFrom
        ? this.toDateString(this.dueDateFrom)
        : undefined,

      dueDateTo: this.dueDateTo
        ? this.toDateString(this.dueDateTo)
        : undefined,

      sortBy: this.sortBy,
      sortDirection: this.sortDirection,

      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.taskService.getTasks(filters).subscribe({
      next: (response) => {
        this.tasks = response.data.items;
        this.totalCount = response.data.totalCount;

        this.distributeTasksByStatus();

        this.isLoading = false;
        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error('Görevler alınamadı:', error);

        this.errorMessage =
          error?.error?.message ??
          'Görevler yüklenirken bir hata oluştu.';

        this.isLoading = false;
        this.cdr.markForCheck();
      }
    });
  }

  openCreateTaskDialog(): void {
    const dialogRef = this.dialog.open(TaskForm, {
      width: '600px',
      maxWidth: '95vw',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe((createdTask: TaskItem | undefined) => {
      if(!createdTask)
        return;

      this.tasks = [createdTask, ...this.tasks];

      this.showSuccess('Görev başarıyla oluşturuldu.');

      this.cdr.markForCheck();
    });
  }

  openEditTaskDialog(task: TaskItem): void {
    const dialogRef = this.dialog.open<
      TaskForm,
      TaskItem,
      TaskItem
    >(TaskForm, {
      width: '600px',
      maxWidth: '95vw',
      disableClose: true,
      data: task
    });

    dialogRef.afterClosed().subscribe(
      (updatedTask: TaskItem | undefined) => {
        if (!updatedTask) {
          return;
        }

        this.tasks = this.tasks.map((currentTask) =>
          currentTask.id === updatedTask.id
            ? updatedTask
            : currentTask
        );

        this.showSuccess('Görev başarıyla düzenlendi.');

        this.cdr.markForCheck();
      }
    );
  }

  deleteTask(task: TaskItem): void {
    const dialogRef = this.dialog.open<
      ConfirmDialog,
      ConfirmDialogData,
      boolean
    >(ConfirmDialog, {
      width: '420px',
      maxWidth: '95vw',
      disableClose: true,
      data: {
        title: 'Görevi Sil',
        message:
          `"${task.title}" görevini silmek istediğinize emin misiniz?`,
        confirmText: 'Sil',
        cancelText: 'Vazgeç'
      }
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (!confirmed) {
        return;
      }

      this.taskService.deleteTask(task.id).subscribe({
        next: () => {
          this.tasks = this.tasks.filter(
            (currentTask) =>
              currentTask.id !== task.id
          );

          this.showSuccess(
            'Görev başarıyla silindi.'
          );

          this.cdr.markForCheck();
        },

        error: (error) => {
          console.error(
            'Görev silinemedi:',
            error
          );

          this.errorMessage =
            error?.error?.message ??
            'Görev silinirken bir hata oluştu.';

          this.showError(
            this.errorMessage
          );

          this.cdr.markForCheck();
        }
      });
    });
  }

  openTaskDetail(task: TaskItem): void {
    this.router.navigate(['/tasks', task.id]);
  }

  private showSuccess(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }

  private showError(message: string): void {
    this.snackBar.open(message, 'Kapat', {
      duration: 4000,
      horizontalPosition: 'right',
      verticalPosition: 'top',
      panelClass: ['error-snackbar']
    });
  }

  private toDateString(date: Date): string {
    const year = date.getFullYear();

    const month = String(
      date.getMonth() + 1
    ).padStart(2, '0');

    const day = String(
      date.getDate()
    ).padStart(2, '0');

    return `${year}-${month}-${day}`;
  }

  applyFilters(): void {
    this.currentPage = 1;
    this.loadTasks();
  }

  clearFilters(): void {
    this.searchText = '';
    this.selectedPriority = null;
    this.selectedStatus = null;
    this.dueDateFrom = null;
    this.dueDateTo = null;
    this.sortBy = 'createdAt';
    this.sortDirection = 'desc';
    this.currentPage = 1;

    this.loadTasks();
  }

  onSortingChanged(): void {
    this.currentPage = 1;
    this.loadTasks();
  }

  onPageChange(event: PageEvent): void {
    this.currentPage = event.pageIndex + 1;
    this.pageSize = event.pageSize;

    this.loadTasks();
  }

  getTasksByStatus(status: number): TaskItem[] {
    return this.tasks.filter((task) => task.status === status);
  }

  private distributeTasksByStatus(): void {
    this.pendingTasks = this.tasks.filter(
      (task) => task.status === 0
    );

    this.inProgressTasks = this.tasks.filter(
      (task) => task.status === 1
    );

    this.completedTasks = this.tasks.filter(
      (task) => task.status === 2
    );

    this.cancelledTasks = this.tasks.filter(
      (task) => task.status === 3
    );
  }

  dropTask(
    event: CdkDragDrop<TaskItem[]>,
    newStatus: number
  ): void {
    const task = event.previousContainer.data[event.previousIndex];

    if (task.status === newStatus) {
      return;
    }

    const oldStatus = task.status;

    transferArrayItem(
      event.previousContainer.data,
      event.container.data,
      event.previousIndex,
      event.currentIndex
    );

    task.status = newStatus;

    const request: UpdateTask = {
      title: task.title,
      description: task.description,
      priority: task.priority,
      status: newStatus,
      dueDate: task.dueDate
        ? new Date(task.dueDate).toISOString()
        : undefined,
      categoryId: task.categoryId
    };

    this.taskService.updateTask(task.id, request).subscribe({
      next: (response) => {
        const updatedTask = response.data;

        this.tasks = this.tasks.map((currentTask) =>
          currentTask.id === updatedTask.id
            ? updatedTask
            : currentTask
        );

        this.distributeTasksByStatus();

        this.showSuccess(
          'Görev durumu başarıyla güncellendi.'
        );

        this.cdr.markForCheck();
      },

      error: (error) => {
        console.error(
          'Görev durumu güncellenemedi:',
          error
        );

        task.status = oldStatus;

        this.distributeTasksByStatus();

        this.showError(
          error?.error?.message ??
          'Görev durumu güncellenemedi.'
        );

        this.cdr.markForCheck();
      }
    });
  }
}