import { useInfiniteQuery } from "@tanstack/react-query";
import { PositionsFilterState } from "./position-filter-store";
import { positionsQueryOptions } from "@/entities/positions/api";
import { RefCallback, useCallback } from "react";

export function usePositionsList({
  search,
  pageSize,
  isActive,
  sortBy,
  sortOrder,
}: PositionsFilterState & { pageSize: number }) {
  const {
    data,
    isPending,
    error,
    isError,
    refetch,
    fetchNextPage,
    isFetchingNextPage,
    hasNextPage,
  } = useInfiniteQuery({
    ...positionsQueryOptions.getPositionsInfiniteOptions({
      search,
      isActive,
      pageSize: pageSize,
      sortBy,
      sortOrder,
    }),
  });

  const cursorRef: RefCallback<HTMLDivElement> = useCallback(
    (el) => {
      const observer = new IntersectionObserver(
        (entries) => {
          if (entries[0].isIntersecting && hasNextPage && !isFetchingNextPage) {
            fetchNextPage();
          }
        },
        { threshold: 0.5 },
      );

      if (el) {
        observer.observe(el);

        return () => observer.disconnect();
      }
    },
    [fetchNextPage, hasNextPage, isFetchingNextPage],
  );

  return {
    positions: data?.items,
    totalPages: data?.totalPages,
    totalCount: data?.totalCount,
    isPending,
    error: error instanceof Error ? error : undefined,
    isError,
    refetch,
    isFetchingNextPage,
    cursorRef,
  };
}
