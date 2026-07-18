export interface UpdateTask {
  title: string;
  description?: string;
  priority: number;
  status: number;
  dueDate?: string;
  categoryId?: string;
}