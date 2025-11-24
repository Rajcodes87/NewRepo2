import httpClient from '../config/httpClient';
import { ResponseDataDto, PagedResultDto, PagedAndSortedResultRequestDto } from '../types/common';
import { 
  RescuerApplication, 
  CreateRescuerApplicationDto,
  ReviewRescuerApplicationDto,
  RescuerApplicationFilter 
} from '../types/rescuerApplication';

class RescuerApplicationService {
  private baseUrl = '/app/rescuer-application';

  // Submit identity card for verification
  async submitApplication(data: CreateRescuerApplicationDto): Promise<ResponseDataDto<RescuerApplication>> {
    const response = await httpClient.post<ResponseDataDto<RescuerApplication>>(
      `${this.baseUrl}`,
      data
    );
    return response.data;
  }

  // Get application list (Admin only)
  async getList(
    params: PagedAndSortedResultRequestDto,
    filter?: RescuerApplicationFilter
  ): Promise<ResponseDataDto<PagedResultDto<RescuerApplication>>> {
    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescuerApplication>>>(
      `${this.baseUrl}`,
      {
        params: {
          ...params,
          ...filter,
        },
      }
    );
    return response.data;
  }

  // Get application by ID
  async get(id: string): Promise<ResponseDataDto<RescuerApplication>> {
    const response = await httpClient.get<ResponseDataDto<RescuerApplication>>(
      `${this.baseUrl}/${id}`
    );
    return response.data;
  }

  // Get application by user ID (check own application status)
  async getByUserId(userId: string): Promise<ResponseDataDto<RescuerApplication>> {
    const response = await httpClient.get<ResponseDataDto<RescuerApplication>>(
      `${this.baseUrl}/by-user/${userId}`
    );
    return response.data;
  }

  // Review application (Admin only)
  async reviewApplication(data: ReviewRescuerApplicationDto): Promise<ResponseDataDto<object>> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${this.baseUrl}/review`,
      data
    );
    return response.data;
  }
}

export default new RescuerApplicationService();