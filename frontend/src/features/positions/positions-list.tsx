"use client";

import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import Link from "next/link";
import PositionCard from "./position-card";
import { PositionsFilterSearch } from "./positions-filters-search";
import { PositionsFilterIsActive } from "./positions-filters-is-active";
import { PositionsFilterDepartments } from "./positions-filters-departments";
import { CreatePositionDialog } from "./create-position-dialog";
import { useGetPositionFilter } from "./model/position-filter-store";
import { usePositionsList } from "./model/use-positions-list";

export default function PositionsList() {
  const { search, isActive, pageSize, sortBy, sortOrder } =
    useGetPositionFilter();

  const [createOpen, setCreateOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);

  const {
    positions,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    cursorRef,
  } = usePositionsList({
    search,
    isActive,
    pageSize,
    sortBy,
    sortOrder,
  });

  if (isError) {
    return (
      <div className="text-red-500">
        Ошибка: {error ? error.message : "Неизвестная ошибка"}
      </div>
    );
  }

  return (
    <div className="max-w-6xl mx-auto p-6">
      <div className="flex flex-col gap-4 mb-4 sm:flex-row sm:items-center sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-white">Позиции</h1>
          <p className="text-sm text-slate-400 mt-1">
            Список всех должностей организации
          </p>
        </div>

        <div className="flex flex-col sm:items-center sm:gap-4">
          <Button onClick={() => setCreateOpen(true)}>Добавить позицию</Button>
        </div>
      </div>

      <div className="flex flex-col gap-4 md:flex-row mb-6">
        <PositionsFilterSearch />
        <div className="flex flex-row gap-4">
          <PositionsFilterIsActive />
          <PositionsFilterDepartments />
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {isPending ? (
          <Spinner />
        ) : (
          positions?.map((position) => (
            <Link
              key={position.positionId}
              href={`/positions/${position.positionId}`}
            >
              <PositionCard key={position.positionId} position={position} />
            </Link>
          ))
        )}
      </div>

      <CreatePositionDialog open={createOpen} setOpen={setCreateOpen} />

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
