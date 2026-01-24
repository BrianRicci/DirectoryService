import { locationsQueryOptions } from "@/entities/locations/api";
import { useInfiniteQuery } from "@tanstack/react-query";
import { RefCallback, useCallback } from "react";

const PAGE_SIZE = 6;

export function useLocationsList() {
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
    ...locationsQueryOptions.getLocationsInfiniteOptions({
      pageSize: PAGE_SIZE,
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
    locations: data?.items,
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
