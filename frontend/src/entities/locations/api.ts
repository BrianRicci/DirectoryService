import { queryOptions } from "@tanstack/react-query";
import { Address } from "./types";
import { apiClient } from "@/shared/api/axios-instance";

export type CreateLocationRequest = {
  name: string;
  locationAddress: Address;
  timezone: string;
};

export type GetLocationsRequest = {
  search?: string;
  departmentIds?: string[];
  isActive?: boolean;
  page?: number;
  pageSize?: number;
};

export type LocationsResult = {
  locations: Location[];
  totalCount: number;
};

// export type Envelope<T = unknown> = {
//   result: T | null;
//   error: ApiError | null;
//   isError: boolean;
//   timeGenerated: string;
// };

// export type ApiError = {
//   messages: ErrorMessage[];
//   type: ErrorType;
// };

// export type ErrorMessage = {
//   code: string;
//   message: string;
//   invalidField?: string | null;
// };

export type ErrorType = "validation" | "not_found" | "conflict" | "failure";

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest
  ): Promise<LocationsResult> => {
    const response = await apiClient.get<LocationsResult>("/locations", {
      params: request,
    });

    return response.data;
  },

  createLocation: async (request: CreateLocationRequest): Promise<Location> => {
    const response = await apiClient.post("/locations", request);

    return response.data;
  },

  deleteLocation: async (locationId: string): Promise<Location> => {
    const response = await apiClient.delete(`/locations/${locationId}`);

    return response.data;
  },
};

export const locationsQueryOptions = {
  baseKey: "locations",

  getLocationsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () => locationsApi.getLocations({ page, pageSize: pageSize }),
      queryKey: [locationsQueryOptions.baseKey, { page, pageSize }],
    });
  },
};
