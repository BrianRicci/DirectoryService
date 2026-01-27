"use client";

import { Button } from "@/shared/components/ui/button";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";
import { CreateLocationDialog } from "./create-location-dialog";
import { useLocationsList } from "./model/use-locations-list";
import LocationCard from "./location-card";
import { UpdateLocationDialog } from "./update-location-dialog";
import { Location } from "@/entities/locations/types";
import { useGetLocationFilter } from "./model/location-filter-store";
import { LocationsFilterSearch } from "./locations-filters-search";
import { LocationsFilterIsActive } from "./locations-filters-is-active";
import { LocationsFilterSort } from "./locations-filters-sort";

export default function LocationsList() {
  const { search, isActive, pageSize, sortBy, sortOrder } =
    useGetLocationFilter();

  const [createOpen, setCreateOpen] = useState(false);
  const [updateOpen, setUpdateOpen] = useState(false);

  const [selectedLocation, setSelectedLocation] = useState<Location>(
    {} as Location,
  );

  const {
    locations,
    isPending,
    error,
    isError,
    isFetchingNextPage,
    cursorRef,
  } = useLocationsList({
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
          <h1 className="text-2xl font-semibold text-white">Локации</h1>
          <p className="text-sm text-slate-400 mt-1">
            Список всех локаций организации
          </p>
        </div>

        <div className="flex flex-col sm:items-center sm:gap-4">
          <Button onClick={() => setCreateOpen(true)}>Добавить локацию</Button>
        </div>
      </div>

      <div className="flex flex-col gap-4 md:flex-row mb-6">
        <LocationsFilterSearch search={search} />
        <div className="flex flex-row gap-4">
          <LocationsFilterSort sortBy={sortBy} sortOrder={sortOrder} />
          <LocationsFilterIsActive isActive={isActive} />
        </div>
      </div>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
        {isPending ? (
          <Spinner />
        ) : (
          locations?.map((location) => (
            <LocationCard
              key={location.locationId}
              location={location}
              onEdit={() => {
                setSelectedLocation(location);
                setUpdateOpen(true);
              }}
            />
          ))
        )}
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
