export interface CreateTask {
  title: string;
  description?: string;
  priority: number;
  dueDate?: string;
  categoryId?: string;
}