import { Address } from "./types";
import { apiClient } from "@/shared/api/axios-instance";

export type CreateLocationRequest = {
  name: string;
  address: Address;
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
  getLocations: async (request: GetLocationsRequest): Promise<Location[]> => {
    const response = await apiClient.get<LocationsResult>("/locations", {
      params: request,
    });

    return response.data.locations;
  },

  createLocation: async (request: CreateLocationRequest): Promise<Location> => {
    const response = await apiClient.post("/locations", request);

    return response.data;
  },
};
