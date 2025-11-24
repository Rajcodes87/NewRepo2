import httpClient, { extractData } from '../config/httpClient';
import {
  CreateRescueCompletionDto,
  UpdateRescueCompletionDto,
  RescueCompletionDto,
  RescueCompletionResponseDto,
  RescueCompletionFilter,
  VerifyCompletionDto,
} from '../types/rescueCompletion';
import {
  ResponseDataDto,
  PagedAndSortedResultRequestDto,
  PagedResultDto,
} from '../types/common';

const BASE_URL = '/app/rescue-completion';

export class RescueCompletionService {
  // Get paginated list of rescue completions
  static async getList(
    pagination: PagedAndSortedResultRequestDto,
    filter?: RescueCompletionFilter
  ): Promise<PagedResultDto<RescueCompletionDto>> {
    const params = {
      ...pagination,
      ...filter,
    };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescueCompletionDto>>>(
      BASE_URL,
      { params }
    );

    return extractData(response.data);
  }

  // Get single rescue completion by ID
  static async get(id: string): Promise<RescueCompletionDto> {
    const response = await httpClient.get<ResponseDataDto<RescueCompletionDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Get completion by request ID
  static async getByRequestId(requestId: string): Promise<RescueCompletionDto> {
    const response = await httpClient.get<ResponseDataDto<RescueCompletionDto>>(
      `${BASE_URL}/by-request/${requestId}`
    );

    return extractData(response.data);
  }

  // Create new rescue completion
  static async create(
    input: CreateRescueCompletionDto
  ): Promise<RescueCompletionResponseDto> {
    const response = await httpClient.post<ResponseDataDto<RescueCompletionResponseDto>>(
      BASE_URL,
      input
    );

    return extractData(response.data);
  }

  // Update existing rescue completion
  static async update(
    id: string,
    input: UpdateRescueCompletionDto
  ): Promise<RescueCompletionResponseDto> {
    const response = await httpClient.put<ResponseDataDto<RescueCompletionResponseDto>>(
      `${BASE_URL}/${id}`,
      input
    );

    return extractData(response.data);
  }

  // Delete rescue completion
  static async delete(id: string): Promise<RescueCompletionResponseDto> {
    const response = await httpClient.delete<ResponseDataDto<RescueCompletionResponseDto>>(
      `${BASE_URL}/${id}`
    );

    return extractData(response.data);
  }

  // Verify completion (Admin operation)
  static async verify(id: string, input: VerifyCompletionDto): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/verify-completion`,
      input
    );

    extractData(response.data);
  }

  // Unverify completion (Admin operation)
  static async unverify(id: string): Promise<void> {
    const response = await httpClient.post<ResponseDataDto<object>>(
      `${BASE_URL}/${id}/unverify-completion`
    );

    extractData(response.data);
  }

  // Get completions for current user (rescuer)
  static async getMyCompletions(
    pagination: PagedAndSortedResultRequestDto
  ): Promise<PagedResultDto<RescueCompletionDto>> {
    const params = { ...pagination };

    const response = await httpClient.get<ResponseDataDto<PagedResultDto<RescueCompletionDto>>>(
      `${BASE_URL}/my-completions`,
      { params }
    );

    return extractData(response.data);
  }
}

export default RescueCompletionService;
