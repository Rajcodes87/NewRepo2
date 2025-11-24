// Request Rescue DTOs and Types

export interface CreateUpdateRequestRescueDto {
  title: string;
  location: string;
  description: string;
  picture?: string;
  contactNo: string;
  contactName?: string;
  isActive?: boolean;
}

export interface RequestRescueDto {
  id: string;
  title: string;
  location: string;
  description: string;
  picture?: string;
  contactNo: string;
  contactName?: string;
  requestDate: string;
  status: RequestRescueStatus;
  isActive: boolean;
  creationTime: string;
  initiationsCount: number;
  hasCompletion: boolean;
}

export interface RequestRescueResponseDto {
  id: string;
  title: string;
  status: string;
}

export interface RequestRescueFilter {
  searchKeyword?: string;
  status?: RequestRescueStatus;
  isActive?: boolean;
}

export type RequestRescueStatus =
  | 'NotInitiated'
  | 'Initiated'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled';

export const REQUEST_RESCUE_STATUS_OPTIONS = [
  { label: 'Not Initiated', value: 'NotInitiated' },
  { label: 'Initiated', value: 'Initiated' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Cancelled', value: 'Cancelled' },
] as const;
