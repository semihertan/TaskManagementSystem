export interface TaskFilter {
  search?: string;
  priority?: number;
  status?: number;
  categoryId?: string;
  dueDateFrom?: string;
  dueDateTo?: string;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
  page?: number;
  pageSize?: number;
}