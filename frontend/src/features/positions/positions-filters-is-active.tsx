"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { setFilterIsActive } from "./model/position-filter-store";

export function PositionsFilterIsActive({ isActive }: { isActive?: boolean }) {
  return (
    <div className="w-1/2 md:w-[180px]">
      <Select
        value={isActive === undefined ? "all" : isActive ? "active" : "deleted"}
        onValueChange={(value) => {
          if (value === "all") setFilterIsActive(undefined);
          else if (value === "deleted") setFilterIsActive(false);
          else setFilterIsActive(true);
        }}
      >
        <SelectTrigger className="w-full md:w-[180px]">
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="active">Активные</SelectItem>
          <SelectItem value="deleted">Неактивные</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
