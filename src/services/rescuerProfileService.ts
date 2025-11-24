import httpClient, { extractData } from '../config/httpClient';
import { ResponseDataDto } from '../types/common';
import { 
  RescuerProfile, 
  CreateUpdateRescuerProfile, 
  ShareLocationDto, 
  RescuerDistanceDto 
} from '../types/rescuerProfile';

const BASE_URL = '/app/rescuer-profile';

export class RescuerProfileService {
  // Get rescuer profile by user ID
  static async getByUserId(userId: string): Promise<RescuerProfile> {
    const response = await httpClient.get<ResponseDataDto<RescuerProfile>>(
      `${BASE_URL}/by-user/${userId}`
    );
    return extractData(response.data);
  }

  // Get current user's rescuer profile
  static async getMyProfile(): Promise<RescuerProfile> {
    const response = await httpClient.get<ResponseDataDto<RescuerProfile>>(
      `${BASE_URL}/my-profile`
    );
    return extractData(response.data);
  }

  // Create or update rescuer profile
  static async createOrUpdate(data: CreateUpdateRescuerProfile): Promise<RescuerProfile> {
    const response = await httpClient.post<ResponseDataDto<RescuerProfile>>(
      BASE_URL,
      data
    );
    return extractData(response.data);
  }

  // Share location via email link
  static async shareLocation(data: ShareLocationDto): Promise<{ distance: number; distanceDisplay: string }> {
    const response = await httpClient.post<ResponseDataDto<{ distance: number; distanceDisplay: string }>>(
      `${BASE_URL}/share-location`,
      data
    );
    return extractData(response.data);
  }

  // Get nearest rescuers to a rescue request
  static async getNearestRescuers(
    requestRescueId: string,
    maxResults: number = 10
  ): Promise<RescuerDistanceDto[]> {
    const response = await httpClient.get<ResponseDataDto<RescuerDistanceDto[]>>(
      `${BASE_URL}/nearest-rescuers/${requestRescueId}`,
      { params: { maxResults } }
    );
    return extractData(response.data);
  }
}

export default RescuerProfileService;