import { Component, input } from '@angular/core';

import { getPriorityDefinition } from '../../constants/priority.constants';

@Component({
  selector: 'app-priority-badge',
  standalone: true,
  templateUrl: './priority-badge.html',
  styleUrl: './priority-badge.scss',
})
export class PriorityBadge {
  readonly priority = input<number>(0);

  getPriorityLabel(): string {
    return getPriorityDefinition(this.priority()).label;
  }

  getPriorityClass(): string {
    return getPriorityDefinition(this.priority()).className;
  }
}
