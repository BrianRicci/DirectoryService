"use client";

import { Input } from "@/shared/components/ui/input";
import { setFilterSearch } from "./model/location-filter-store";
import { useDebounce } from "use-debounce";
import { useEffect, useState } from "react";

export function LocationsFilterSearch({ search }: { search?: string }) {
  const [localSearch, setLocalSearch] = useState(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  return (
    <Input
      value={localSearch}
      onChange={(e) => setLocalSearch(e.target.value)}
      placeholder="Поиск по названию"
      className="min-w-[260px] bg-slate-800/50 sm:mb-0"
    />
  );
}
