// Common response wrapper from ABP backend
export interface ResponseDataDto<T> {
  success: boolean;
  code: number;
  data?: T;
  message?: string;
  warnings?: string[];
  validationErrors?: string[];
}

// Pagination request
export interface PagedAndSortedResultRequestDto {
  skipCount?: number;
  maxResultCount?: number;
  sorting?: string;
}

// Pagination response
export interface PagedResultDto<T> {
  items: T[];
  totalCount: number;
}
