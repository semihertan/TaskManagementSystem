import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';

import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';

import { TaskItem } from '../../interfaces/task/task.interface';

@Component({
  selector: 'app-task-card',
  imports: [
    CommonModule,
    MatCardModule,
    MatChipsModule,
    MatIconModule
  ],
  templateUrl: './task-card.html',
  styleUrl: './task-card.scss',
})
export class TaskCard {

  @Input({ required: true })
  task!: TaskItem;

  getPriorityText(): string {
    switch (this.task.priority) {
      case 1:
        return 'Çok Düşük';

      case 2:
        return 'Düşük';

      case 3:
        return 'Normal';

      case 4:
        return 'Yüksek';

      case 5:
        return 'Çok Yüksek';

      default:
        return 'Bilinmiyor';
    }
  }

  getPriorityClass(): string {
    switch (this.task.priority) {
      case 1:
        return 'priority-very-low';

      case 2:
        return 'priority-low';

      case 3:
        return 'priority-normal';

      case 4:
        return 'priority-high';

      case 5:
        return 'priority-very-high';

      default:
        return '';
    }
  }

  getStatusText(): string {
    switch (this.task.status) {
      case 0:
        return 'Bekliyor';

      case 1:
        return 'Devam Ediyor';

      case 2:
        return 'Tamamlandı';

      case 3:
        return 'İptal Edildi';

      default:
        return 'Bilinmiyor';
    }
  }

  getStatusClass(): string {
    switch (this.task.status) {
      case 0:
        return 'status-pending';

      case 1:
        return 'status-progress';

      case 2:
        return 'status-completed';

      case 3:
        return 'status-cancelled';

      default:
        return '';
    }
  }
}