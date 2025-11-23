// Rescue Initiation DTOs and Types

export interface CreateRescueInitiationDto {
  requestRescueId: string;
  notes?: string;
}

export interface UpdateRescueInitiationDto {
  notes?: string;
}

export interface RescueInitiationDto {
  id: string;
  requestRescueId: string;
  requestTitle: string;
  requestLocation: string;
  rescuerId: string;
  rescuerName?: string;
  rescuerEmail?: string;
  initiatedDate: string;
  notes?: string;
  status: RescueInitiationStatus;
  isSelected: boolean;
  acceptedDate?: string;
  acceptedByUserId?: string;
  acceptedByUserName?: string;
  creationTime: string;
}

export interface RescueInitiationResponseDto {
  id: string;
  requestRescueId: string;
  status: string;
}

export interface RescueInitiationFilter {
  searchKeyword?: string;
  status?: RescueInitiationStatus;
  isSelected?: boolean;
  requestRescueId?: string;
  rescuerId?: string;
}

export type RescueInitiationStatus =
  | 'Pending'
  | 'Accepted'
  | 'Rejected'
  | 'Withdrawn';

export const RESCUE_INITIATION_STATUS_OPTIONS = [
  { label: 'Pending', value: 'Pending' },
  { label: 'Accepted', value: 'Accepted' },
  { label: 'Rejected', value: 'Rejected' },
  { label: 'Withdrawn', value: 'Withdrawn' },
] as const;
