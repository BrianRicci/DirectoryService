import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Address, Location } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { PaginationResponse } from "@/shared/api/types";

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

export const locationsApi = {
  getLocations: async (
    request: GetLocationsRequest,
  ): Promise<Envelope<PaginationResponse<Location>>> => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Location>>
    >("/locations", {
      params: request,
    });

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

  getLocationsInfiniteOptions: ({ pageSize }: { pageSize: number }) => {
    return infiniteQueryOptions({
      queryKey: [locationsQueryOptions.baseKey],
      queryFn: ({ pageParam }) => {
        return locationsApi.getLocations({ page: pageParam, pageSize });
      },
      initialPageParam: 1,
      getNextPageParam: (response) => {
        const page = response?.result?.page;
        const totalPages = response?.result?.totalPages;

        if (
          !response ||
          page === undefined ||
          totalPages === undefined ||
          page >= totalPages
        ) {
          return undefined;
        }

        return page + 1;
      },
      select: (data): PaginationResponse<Location> => ({
        items: data.pages.flatMap((page) => page?.result?.items ?? []),
        totalCount: data?.pages[0]?.result?.totalCount ?? 0,
        page: data?.pages[0]?.result?.page ?? 1,
        pageSize: data?.pages[0]?.result?.pageSize ?? pageSize,
        totalPages: data?.pages[0]?.result?.totalPages ?? 0,
      }),
    });
  },
};
