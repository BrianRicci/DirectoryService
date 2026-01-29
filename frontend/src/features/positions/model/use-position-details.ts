import { positionsQueryOptions } from "@/entities/positions/api";
import { useQuery } from "@tanstack/react-query";

export function usePositionDetails(positionId: string) {
  const { data, isPending, error, isError, refetch } = useQuery({
    ...positionsQueryOptions.getPositionOptions(positionId),
  });

  return {
    position: data?.result,
    isPending,
    error: error instanceof Error ? error : undefined,
    isError,
    refetch,
  };
}
