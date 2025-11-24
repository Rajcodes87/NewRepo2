// Request Rescue DTOs and Types
export interface RequestRescue {
  id: string;
  title: string;
  location: string;
  description: string;
  picture?: string;
  contactNo: string;
  contactName?: string;
  requestDate: string;
  status: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  isActive: boolean;
  
  // ✅ GPS Coordinates
  latitude?: number;
  longitude?: number;
  mapUrl?: string;
  
  // Navigation
  initiationsCount: number;
  hasCompletion: boolean;
  
  // Audit
  creationTime: string;
  creatorId?: string;
  lastModificationTime?: string;
  lastModifierId?: string;
}

export interface CreateUpdateRequestRescue {
  title: string;
  location: string;
  description: string;
  picture?: string;
  contactNo: string;
  contactName?: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  
  // ✅ GPS Coordinates
  latitude?: number;
  longitude?: number;
  mapUrl?: string;
}

export interface CreateUpdateRequestRescueDto {
  title: string;
  location: string;
  description: string;
  picture?: string;
  contactNo: string;
  contactName?: string;
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  isActive?: boolean;
  
  // ✅ GPS Coordinates
  latitude?: number;
  longitude?: number;
  mapUrl?: string;
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
  severity: 'Low' | 'Medium' | 'High' | 'Critical';
  isActive: boolean;
  creationTime: string;
  initiationsCount: number;
  hasCompletion: boolean;
  
  // ✅ GPS Coordinates
  latitude?: number;
  longitude?: number;
  mapUrl?: string;
}

export interface RequestRescueResponseDto {
  id: string;
  title: string;
  status: string;
}

export interface RequestRescueFilter {
  searchKeyword?: string;
  status?: RequestRescueStatus;
  severity?: 'Low' | 'Medium' | 'High' | 'Critical';
  isActive?: boolean;
}

export type RequestRescueStatus =
  | 'NotInitiated'
  | 'Initiated'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled';

export type RequestRescueSeverity = 'Low' | 'Medium' | 'High' | 'Critical';

// ✅ Status Options for UI
export const REQUEST_RESCUE_STATUS_OPTIONS = [
  { label: 'Not Initiated', value: 'NotInitiated' },
  { label: 'Initiated', value: 'Initiated' },
  { label: 'In Progress', value: 'InProgress' },
  { label: 'Completed', value: 'Completed' },
  { label: 'Cancelled', value: 'Cancelled' },
] as const;

// ✅ NEW: Severity Options for UI
export const REQUEST_RESCUE_SEVERITY_OPTIONS = [
  { label: '🟢 Low', value: 'Low', color: '#52c41a' },
  { label: '🟡 Medium', value: 'Medium', color: '#faad14' },
  { label: '🟠 High', value: 'High', color: '#fa8c16' },
  { label: '🔴 Critical', value: 'Critical', color: '#ff4d4f' },
] as const;

// Helper function to get severity badge color
export const getSeverityColor = (severity: string): string => {
  switch (severity) {
    case 'Low':
      return 'success';
    case 'Medium':
      return 'warning';
    case 'High':
      return 'orange';
    case 'Critical':
      return 'error';
    default:
      return 'default';
  }
};

// Helper function to get severity tag color (Ant Design color)
export const getSeverityTagColor = (severity: string): string => {
  switch (severity) {
    case 'Low':
      return 'green';
    case 'Medium':
      return 'gold';
    case 'High':
      return 'orange';
    case 'Critical':
      return 'red';
    default:
      return 'default';
  }
};