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

export default function PositionsList() {
  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 9;

  // Mock data for demonstration
  const mockPositions = [
    {
      positionId: "1",
      name: "Разработчик",
      description: "Senior C# разработчик",
      isActive: true,
      departmentsCount: 2,
    },
    {
      positionId: "2",
      name: "Менеджер",
      description: "Проект-менеджер",
      isActive: true,
      departmentsCount: 1,
    },
    {
      positionId: "3",
      name: "Дизайнер",
      description: "UX/UI дизайнер",
      isActive: false,
      departmentsCount: 0,
    },
  ];

  const positions = mockPositions;
  const isPending = false;

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
              <PositionCard position={position} />
            </Link>
          ))
        )}
      </div>

      {/* Pagination */}
      <div className="flex justify-center items-center gap-2 mt-8">
        <Button
          variant="outline"
          disabled={currentPage === 1}
          onClick={() => setCurrentPage(currentPage - 1)}
        >
          Назад
        </Button>
        <span className="text-white">Страница {currentPage}</span>
        <Button
          variant="outline"
          onClick={() => setCurrentPage(currentPage + 1)}
        >
          Вперёд
        </Button>
      </div>

      <CreatePositionDialog open={createOpen} setOpen={setCreateOpen} />
    </div>
  );
}
