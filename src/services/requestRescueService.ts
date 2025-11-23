import httpClient, { extractData } from '../config/httpClient';
import {
  CreateUpdateRequestRescueDto,
  RequestRescueDto,
  RequestRescueResponseDto,
  RequestRescueFilter,
} from '../types/requestRescue';
import {
  ResponseDataDto,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '../types/common';

const BASE_URL = '/app/request-rescue';

export class RequestRescueService {
  // Get paginated list of rescue requests
  static async getList(
    pagination: PagedAndSortedResultRequestDto,
    filter?: RequestRescueFilter
  ): Promise<PagedResultDto<RequestRescueDto>> {
    const params = {
      ...pagination,
      ...filter,
    };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RequestRescueDto>>>(
      BASE_URL,
      { params }
    );

    return extractData(response.data);
  }

  // Get single rescue request by ID
  static async get(id: string): Promise<RequestRescueDto> {
    const response = await httpClient.get<ResponseDataDto<RequestRescueDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Create new rescue request
  static async create(
    input: CreateUpdateRequestRescueDto
  ): Promise<RequestRescueResponseDto> {
    const response = await httpClient.post<ResponseDataDto<RequestRescueResponseDto>>(
      BASE_URL,
      input
    );

    return extractData(response.data);
  }

  // Update existing rescue request
  static async update(
    id: string,
    input: CreateUpdateRequestRescueDto
  ): Promise<RequestRescueResponseDto> {
    const response = await httpClient.put<ResponseDataDto<RequestRescueResponseDto>>(
      `${BASE_URL}/${id}`,
      input
    );

    return extractData(response.data);
  }

  // Delete rescue request
  static async delete(id: string): Promise<RequestRescueResponseDto> {
    const response = await httpClient.delete<ResponseDataDto<RequestRescueResponseDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Activate rescue request
  static async activate(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/activate`
    );

    extractData(response.data);
  }

  // Deactivate rescue request
  static async deactivate(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/deactivate`
    );

    extractData(response.data);
  }
  static async accept(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
    `${BASE_URL}/${id}/accept-initiation`
    );

    extractData(response.data);

  }
}

export default RequestRescueService;
