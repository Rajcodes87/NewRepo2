export interface RescuerApplication {
    id: string;
    userId: string;
    userName: string;
    email: string;
    name: string;
    surname: string;
    phoneNumber: string;
    identityCardPicture?: string; // Base64 encoded image
    status: 'Pending' | 'Approved' | 'Rejected';
    applicationDate: string;
    reviewedByUserId?: string;
    reviewedDate?: string;
    reviewNotes?: string;
  }
  
  export interface CreateRescuerApplicationDto {
    identityCardPicture: string; // Base64 encoded image
  }
  
  export interface ReviewRescuerApplicationDto {
    applicationId: string;
    status: 'Approved' | 'Rejected';
    reviewNotes?: string;
  }
  
  export interface RescuerApplicationFilter {
    searchKeyword?: string;
    status?: 'Pending' | 'Approved' | 'Rejected';
  }