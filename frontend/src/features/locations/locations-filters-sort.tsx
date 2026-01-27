"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { SortOptions } from "@/shared/api/types";
import {
  setFilterSortOrder,
} from "./model/location-filter-store";

export function LocationsFilterSort({ sortBy, sortOrder }: SortOptions) {
  const handleSortChange = (value: string) => {
    setFilterSortOrder(value as "asc" | "desc");
  };

  return (
    <div className="w-1/2 md:w-[180px]">
      <Select value={sortOrder || "asc"} onValueChange={handleSortChange}>
        <SelectTrigger className="w-full md:w-[180px]">
          <SelectValue placeholder="Сортировка" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="asc">Новые</SelectItem>
          <SelectItem value="desc">Старые</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
