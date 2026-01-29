import { infiniteQueryOptions, queryOptions } from "@tanstack/react-query";
import { Position } from "./types";
import { apiClient } from "@/shared/api/axios-instance";
import { Envelope } from "@/shared/api/envelope";
import { PaginationResponse } from "@/shared/api/types";
import { LocationsFilterState as PositionsFilterState } from "@/features/locations/model/location-filter-store";

export type CreatePositionRequest = {
  name: string;
  description: string;
  departnentIds: string[];
};

export type UpdatePositionRequest = {
  positionId: string;
  name: string;
  description: string;
  departmentIds: string[];
};

export type GetPositionsRequest = {
  search?: string;
  departmentIds?: string[];
  isActive?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
};

export const positionsApi = {
  getPositions: async (
    request: GetPositionsRequest,
  ): Promise<Envelope<PaginationResponse<Position>>> => {
    const response = await apiClient.get<
      Envelope<PaginationResponse<Position>>
    >("/positions", {
      params: request,
    });

    return response.data;
  },

  createPosition: async (
    request: CreatePositionRequest,
  ): Promise<Envelope<Position>> => {
    const response = await apiClient.post<Envelope<Position>>(
      "/positions",
      request,
    );

    return response.data;
  },

  updatePosition: async ({
    positionId,
    name,
    description,
    departmentIds,
  }: UpdatePositionRequest): Promise<Envelope<Position>> => {
    const response = await apiClient.patch<Envelope<Position>>(
      `/positions/${positionId}`,
      { name, description, departmentIds },
    );
    return response.data;
  },

  deletePosition: async (positionId: string): Promise<Envelope<Position>> => {
    const response = await apiClient.delete<Envelope<Position>>(
      `/positions/${positionId}`,
    );

    return response.data;
  },
};

export const positionsQueryOptions = {
  baseKey: "positions",

  getPositionsOptions: ({
    page,
    pageSize,
  }: {
    page: number;
    pageSize: number;
  }) => {
    return queryOptions({
      queryFn: () => positionsApi.getPositions({ page, pageSize: pageSize }),
      queryKey: [positionsQueryOptions.baseKey, { page, pageSize }],
    });
  },

  getPositionsInfiniteOptions: (filter: PositionsFilterState) => {
    return infiniteQueryOptions({
      queryKey: [positionsQueryOptions.baseKey, filter],
      queryFn: ({ pageParam }) => {
        return positionsApi.getPositions({ ...filter, page: pageParam });
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
      select: (data): PaginationResponse<Position> => ({
        items: data.pages.flatMap((page) => page?.result?.items ?? []),
        totalCount: data?.pages[0]?.result?.totalCount ?? 0,
        page: data?.pages[0]?.result?.page ?? 1,
        pageSize: data?.pages[0]?.result?.pageSize ?? filter.pageSize,
        totalPages: data?.pages[0]?.result?.totalPages ?? 0,
      }),
    });
  },
};
