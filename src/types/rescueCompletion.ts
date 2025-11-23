// Rescue Completion DTOs and Types

export interface CreateRescueCompletionDto {
  requestRescueId: string;
  completionProofPicture?: string;
  completionDate: string;
  completionDescription?: string;
}

export interface UpdateRescueCompletionDto {
  completionProofPicture?: string;
  completionDate: string;
  completionDescription?: string;
}

export interface RescueCompletionDto {
  id: string;
  requestRescueId: string;
  requestTitle: string;
  requestLocation: string;
  completionProofPicture?: string;
  completionDate: string;
  completionDescription?: string;
  completedByRescuerId: string;
  completedByRescuerName?: string;
  completedByRescuerEmail?: string;
  isVerified: boolean;
  verifiedByUserId?: string;
  verifiedByUserName?: string;
  verifiedDate?: string;
  verificationNotes?: string;
  creationTime: string;
}

export interface RescueCompletionResponseDto {
  id: string;
  requestRescueId: string;
  isVerified: boolean;
}

export interface VerifyCompletionDto {
  verificationNotes?: string;
}

export interface RescueCompletionFilter {
  searchKeyword?: string;
  isVerified?: boolean;
  requestRescueId?: string;
  completedByRescuerId?: string;
  completionDateFrom?: string;
  completionDateTo?: string;
}
