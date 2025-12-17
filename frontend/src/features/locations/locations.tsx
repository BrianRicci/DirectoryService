"use client";

import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Card } from "@/shared/components/ui/card";
import { locationsApi } from "@/entities/locations/api";
import { Spinner } from "@/shared/components/ui/spinner";
import { useQuery } from "@tanstack/react-query";
import { useState } from "react";

const PAGE_SIZE = 10;

export default function Locations() {
  const [page, setPage] = useState(1);

  const {
    data: locations,
    isLoading,
    error,
  } = useQuery({
    queryFn: () => locationsApi.getLocations({ page, pageSize: PAGE_SIZE }),
    queryKey: ["locations", { page }],
  });

  if (isLoading) {
    return <Spinner />;
  }

  if (error) {
    return <div className="text-red-500">Ошибка: {error.message}</div>;
  }

  return (
    <div className="max-w-6xl mx-auto p-6">
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4 mb-6">
        <div>
          <h1 className="text-2xl font-semibold text-white">Локации</h1>
          <p className="text-sm text-slate-400 mt-1">
            Список всех локаций организации
          </p>
        </div>

        <div className="flex items-center gap-3">
          <Input
            placeholder="Поиск по названию или адресу"
            className="min-w-[260px] bg-slate-800/50"
          />
          <Button variant="default">Добавить локацию</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {locations?.map((location) => (
          <Card
            key={location.id}
            className="bg-slate-900/40 border-slate-700 px-4"
          >
            <div className="flex justify-between items-start">
              <div>
                <h3 className="text-lg font-medium text-white">
                  {location.name}
                </h3>
                <div className="text-sm text-slate-400 mt-1">
                  {location.address.city}, {location.address.street},{" "}
                  {location.address.house}
                </div>
              </div>
              <div>
                <span
                  className={
                    "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium " +
                    (location.isActive
                      ? "bg-emerald-100 text-emerald-800"
                      : "bg-amber-100 text-amber-800")
                  }
                >
                  {location.isActive ? "Активна" : "Неактивна"}
                </span>
              </div>
            </div>

            <div className="mt-4 text-sm text-slate-300">
              Часовой пояс: {location.timezone}
            </div>

            <div className="mt-4 flex gap-2">
              <Button variant="ghost">Редактировать</Button>
              <Button variant="destructive">Удалить</Button>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
