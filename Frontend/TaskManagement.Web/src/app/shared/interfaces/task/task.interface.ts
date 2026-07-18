export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  priority: number;
  status: number;
  dueDate?: Date;
  completedAt?: Date;

  userId: string;
  categoryId?: string;

  createdAt: Date;
  updatedAt: Date;
}