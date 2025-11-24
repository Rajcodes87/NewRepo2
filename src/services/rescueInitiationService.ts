import httpClient, { extractData } from '../config/httpClient';
import {
  CreateRescueInitiationDto,
  UpdateRescueInitiationDto,
  RescueInitiationDto,
  RescueInitiationResponseDto,
  RescueInitiationFilter,
} from '../types/rescueInitiation';
import {
  ResponseDataDto,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '../types/common';

const BASE_URL = '/app/rescue-initiation';

export class RescueInitiationService {
  // Get paginated list of rescue initiations
  static async getList(
    pagination: PagedAndSortedResultRequestDto,
    filter?: RescueInitiationFilter
  ): Promise<PagedResultDto<RescueInitiationDto>> {
    const params = {
      ...pagination,
      ...filter,
    };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescueInitiationDto>>>(
      BASE_URL,
      { params }
    );

    return extractData(response.data);
  }

  // Get single rescue initiation by ID
  static async get(id: string): Promise<RescueInitiationDto> {
    const response = await httpClient.get<ResponseDataDto<RescueInitiationDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Create new rescue initiation
  static async create(
    input: CreateRescueInitiationDto
  ): Promise<RescueInitiationResponseDto> {
    const response = await httpClient.post<ResponseDataDto<RescueInitiationResponseDto>>(
      BASE_URL,
      input
    );

    return extractData(response.data);
  }

  // Update existing rescue initiation
  static async update(
    id: string,
    input: UpdateRescueInitiationDto
  ): Promise<RescueInitiationResponseDto> {
    const response = await httpClient.put<ResponseDataDto<RescueInitiationResponseDto>>(
      `${BASE_URL}/${id}`,
      input
    );

    return extractData(response.data);
  }

  // Delete rescue initiation
  static async delete(id: string): Promise<RescueInitiationResponseDto> {
    const response = await httpClient.delete<ResponseDataDto<RescueInitiationResponseDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Accept initiation (Admin operation)
  static async accept(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/accept-initiation`
    );

    extractData(response.data);
  }

  // Reject initiation (Admin operation)
  static async reject(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/reject-initiation`
    );

    extractData(response.data);
  }

  // Withdraw initiation (Rescuer operation)
  static async withdraw(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/withdraw-initiation`
    );

    extractData(response.data);
  }

  // Get initiations for a specific request
  static async getInitiationsForRequest(
    requestId: string,
    pagination: PagedAndSortedResultRequestDto
  ): Promise<PagedResultDto<RescueInitiationDto>> {
    const params = { ...pagination };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescueInitiationDto>>>(
      `${BASE_URL}/request/${requestId}`,
      { params }
    );

    return extractData(response.data);
  }

  // Get initiations for current user (rescuer)
  static async getMyInitiations(
    pagination: PagedAndSortedResultRequestDto
  ): Promise<PagedResultDto<RescueInitiationDto>> {
    const params = { ...pagination };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescueInitiationDto>>>(
      `${BASE_URL}/my-initiations`,
      { params }
    );

    return extractData(response.data);
  }
}

export default RescueInitiationService;
