import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { TaskService } from '../../core/services/task.service';
import { TaskStatistics } from '../../shared/interfaces/task/task-statistics.interface';

@Component({
  selector: 'app-dashboard',
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);

  statistics: TaskStatistics | null = null;

  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.taskService.getStatistics().subscribe({
      next: (response) => {
        this.statistics = response.data;
        this.isLoading = false;

        this.cdr.detectChanges();
      },

      error: (error) => {
        console.error('İstatistikler alınamadı:', error);

        this.errorMessage = 'İstatistikler yüklenirken bir hata oluştu.';
        this.isLoading = false;

        this.cdr.detectChanges();
      }
    });
  }
}