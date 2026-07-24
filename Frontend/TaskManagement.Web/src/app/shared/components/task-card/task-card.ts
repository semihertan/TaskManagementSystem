import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

import { TaskItem } from '../../interfaces/task/task.interface';
import { PriorityBadge } from '../priority-badge/priority-badge';

@Component({
  selector: 'app-task-card',
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    PriorityBadge,
  ],
  templateUrl: './task-card.html',
  styleUrl: './task-card.scss',
})
export class TaskCard {
  @Input({ required: true })
  task!: TaskItem;

  @Output()
  editTask = new EventEmitter<TaskItem>();

  @Output()
  deleteTask = new EventEmitter<TaskItem>();

  @Output()
  viewTask = new EventEmitter<TaskItem>();

  onEdit(): void {
    this.editTask.emit(this.task);
  }

  onDelete(): void {
    this.deleteTask.emit(this.task);
  }

  onView(): void {
    this.viewTask.emit(this.task);
  }

  getStatusText(): string {
    const statusLabels: Record<number, string> = {
      0: 'Bekliyor',
      1: 'Devam Ediyor',
      2: 'Tamamlandı',
      3: 'İptal Edildi',
    };

    return statusLabels[this.task.status] ?? 'Bilinmiyor';
  }

  getStatusClass(): string {
    const statusClasses: Record<number, string> = {
      0: 'status-pending',
      1: 'status-progress',
      2: 'status-completed',
      3: 'status-cancelled',
    };

    return statusClasses[this.task.status] ?? '';
  }
}
