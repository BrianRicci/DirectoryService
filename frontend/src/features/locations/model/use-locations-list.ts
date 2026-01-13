import { locationsQueryOptions } from "@/entities/locations/api";
import { useQuery } from "@tanstack/react-query";

const PAGE_SIZE = 10;

export function useLocationsList({ page }: { page: number }) {
  const { data, isPending, error, isError } = useQuery(
    locationsQueryOptions.getLocationsOptions({ page, pageSize: PAGE_SIZE })
  );

  return {
    locations: data?.locations,
    totalCount: data?.totalCount,
    isPending,
    error: error instanceof Error ? error : undefined,
    isError,
  };
}
