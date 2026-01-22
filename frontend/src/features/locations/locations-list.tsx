"use client";

import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { CreateLocationDialog } from "./create-location-dialog";
import { useLocationsList } from "./model/use-locations-list";
import LocationCard from "./location-card";
import { UpdateLocationDialog } from "./update-location-dialog";
import { Location } from "@/entities/locations/types";

export default function LocationsList() {
  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);

  const [selectedLocation, setSelectedLocation] = useState<Location>(
    {} as Location,
  );

  const {
    locations,
    totalPages,
    totalCount,
    isPending,
    error,
    isError,
    refetch,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList();

  if (isPending) {
    return <Spinner />;
  }

  if (isError) {
    return (
      <div className="text-red-500">
        Ошибка: {error ? error.message : "Неизвестная ошибка"}
      </div>
    );
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
          <Button onClick={() => setCreateOpen(true)}>Добавить локацию</Button>
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {locations?.map((location) => (
          <LocationCard
            key={location.locationId}
            location={location}
            onEdit={() => {
              setSelectedLocation(location);
              setUpdateOpen(true);
            }}
          />
        ))}
      </div>

      <CreateLocationDialog open={createOpen} setOpen={setCreateOpen} />

      {selectedLocation && (
        <UpdateLocationDialog
          key={selectedLocation.locationId}
          location={selectedLocation}
          open={updateOpen}
          onOpenChange={setUpdateOpen}
        />
      )}

      <div ref={cursorRef} className="flex justify-center py-4">
        {isFetchingNextPage && <Spinner />}
      </div>
    </div>
  );
}
