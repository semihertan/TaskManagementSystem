export interface TaskItem {
  id: string;
  title: string;
  description?: string;
  priority: number;
  status: number;
  dueDate?: string;
  completedAt?: string;

  userId: string;
  categoryId?: string;

  createdAt: string;
  updatedAt: string;
}