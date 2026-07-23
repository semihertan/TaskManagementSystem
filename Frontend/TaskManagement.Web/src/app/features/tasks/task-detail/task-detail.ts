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

import { TaskService } from '../../../core/services/task.service';
import { TaskItem } from '../../../shared/interfaces/task/task.interface';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './task-detail.html',
  styleUrl: './task-detail.scss'
})
export class TaskDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);

  task: TaskItem | null = null;
  isLoading = false;
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

          this.task = response;
          this.isLoading = false;
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
}