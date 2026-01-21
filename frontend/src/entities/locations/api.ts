import { queryOptions } from "@tanstack/react-query";
import { Address, Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";

export type CreateLocationRequest = {
  name: string;
  locationAddress: Address;
  timezone: string;
};

export type UpdateLocationRequest = {
  locationId: string;
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

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest,
  ): Promise<Envelope<LocationsResult>> => {
    const response = await apiClient.get<Envelope<LocationsResult>>(
      "/locations",
      {
        params: request,
      },
    );

    return response.data;
  },

  createLocation: async (
    request: CreateLocationRequest,
  ): Promise<Envelope<Location>> => {
    const response = await apiClient.post<Envelope<Location>>(
      "/locations",
      request,
    );

    return response.data;
  },

  updateLocation: async ({
    locationId,
    name,
    locationAddress,
    timezone,
  }: UpdateLocationRequest): Promise<Envelope<Location>> => {
    const response = await apiClient.patch<Envelope<Location>>(
      `/locations/${locationId}`,
      { name, locationAddress, timezone },
    );
    return response.data;
  },

  deleteLocation: async (locationId: string): Promise<Envelope<Location>> => {
    const response = await apiClient.delete<Envelope<Location>>(
      `/locations/${locationId}`,
    );

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
