import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';

import { TaskService } from '../../core/services/task.service';
import { TaskItem } from '../../shared/interfaces/task/task.interface';
import { TaskCard } from '../../shared/components/task-card/task-card';

@Component({
  selector: 'app-tasks',
  imports: [
    CommonModule,
    TaskCard
  ],
  templateUrl: './tasks.html',
  styleUrl: './tasks.scss',
})
export class Tasks implements OnInit {
  private taskService = inject(TaskService);
  private cdr = inject(ChangeDetectorRef);

  tasks: TaskItem[] = [];
  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.taskService.getTasks().subscribe({
      next: (response) => {
        console.log('Tam cevap:', response);
        console.log('Items:', response.data.items);
        console.log('Items uzunluğu:', response.data.items.length);

        this.tasks = response.data.items;
        this.isLoading = false;

        this.cdr.detectChanges();

        console.log('Component tasks:', this.tasks);
},

      error: (error) => {
        console.error('Görevler alınamadı:', error);

        this.errorMessage = 'Görevler yüklenirken bir hata oluştu.';
        this.isLoading = false;

        this.cdr.detectChanges();
      }
    });
  }
}