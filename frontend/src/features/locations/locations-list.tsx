"use client";

import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { CreateLocationDialog } from "./create-location-dialog";
import { useLocationsList } from "./model/use-locations-list";
import LocationCard from "./location-card";

export default function LocationsList() {
  const [page, setPage] = useState(1);
  const [open, setOpen] = useState(false);

  const {
    locations,
    totalCount,
    isPending: getIsPending,
    error,
  } = useLocationsList({ page });

  if (getIsPending) {
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
          <Button onClick={() => setOpen(true)}>Добавить локацию</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {locations?.map((location) => (
          <LocationCard key={location.id} location={location} />
        ))}
      </div>

      <CreateLocationDialog open={open} setOpen={setOpen} />
    </div>
  );
}
