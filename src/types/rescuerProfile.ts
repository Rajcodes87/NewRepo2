export interface RescuerProfile {
  id: string;
  tenantId?: string;
  userId: string;
  
  // Location Information
  latitude?: number;
  longitude?: number;
  currentLocation?: string;
  locationUpdatedAt?: string;
  isLocationShared: boolean;
  
  // Notification Preferences
  maxNotificationRadius: number;
  receiveEmailNotifications: boolean;
  receiveSmsNotifications: boolean;
  
  // Audit
  creationTime: string;
  creatorId?: string;
  lastModificationTime?: string;
  lastModifierId?: string;
}

export interface CreateUpdateRescuerProfile {
  latitude?: number;
  longitude?: number;
  currentLocation?: string;
  isLocationShared: boolean;
  maxNotificationRadius: number;
  receiveEmailNotifications: boolean;
  receiveSmsNotifications: boolean;
}

export interface ShareLocationDto {
  token: string;
  requestRescueId: string;
  latitude: number;
  longitude: number;
  currentLocation?: string;
}

export interface RescuerDistanceDto {
  rescuerId: string;
  rescuerName: string;
  email: string;
  latitude?: number;
  longitude?: number;
  distance: number; // in kilometers
  distanceDisplay: string;
}

export interface RescuerLocationDto {
  rescuerId: string;
  rescuerName: string;
  email: string;
  latitude?: number;
  longitude?: number;
}