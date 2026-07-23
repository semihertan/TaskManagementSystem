export interface TaskFilter {
  search?: string;
  priority?: number;
  status?: number;
  categoryId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  page?: number;
  pageSize?: number;
}