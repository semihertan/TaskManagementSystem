import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { TaskItem } from '../../interfaces/task/task.interface';
import { PriorityBadge } from '../priority-badge/priority-badge';

@Component({
  selector: 'app-task-card',
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
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

  getDueDateWarning(): {
    text: string;
    type: 'overdue' | 'today' | 'tomorrow' | 'soon' | 'none';
  } {
    if (!this.task.dueDate) {
      return { text: '', type: 'none' };
    }

    // Tamamlandı veya iptal edildi görevlerde uyarı gösterme
    if (this.task.status === 2 || this.task.status === 3) {
      return { text: '', type: 'none' };
    }

    const today = new Date();
    const dueDate = new Date(this.task.dueDate);

    today.setHours(0, 0, 0, 0);
    dueDate.setHours(0, 0, 0, 0);

    const differenceInMilliseconds = dueDate.getTime() - today.getTime();
    const differenceInDays = Math.round(
      differenceInMilliseconds / (1000 * 60 * 60 * 24)
    );

    if (differenceInDays < 0) {
      return {
        text: 'Vadesi geçti',
        type: 'overdue'
      };
    }

    if (differenceInDays === 0) {
      return {
        text: 'Bugün son gün',
        type: 'today'
      };
    }

    if (differenceInDays === 1) {
      return {
        text: 'Yarın son gün',
        type: 'tomorrow'
      };
    }

    if (differenceInDays <= 3) {
      return {
        text: `Son tarihe ${differenceInDays} gün kaldı`,
        type: 'soon'
      };
    }

    return {
      text: '',
      type: 'none'
    };
  }

  getProgressValue(): number {
    const progressValues: Record<number, number> = {
      0: 0,
      1: 50,
      2: 100,
      3: 0,
    };

    return progressValues[this.task.status] ?? 0;
  }

  getProgressText(): string {
    const progressTexts: Record<number, string> = {
      0: 'Başlanmadı',
      1: 'Devam ediyor',
      2: 'Tamamlandı',
      3: 'İptal edildi',
    };

    return progressTexts[this.task.status] ?? 'Bilinmiyor';
  }
}
