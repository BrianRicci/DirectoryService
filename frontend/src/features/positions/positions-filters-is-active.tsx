"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useState } from "react";

export function PositionsFilterIsActive() {
  const [isActive, setIsActive] = useState<boolean | undefined>(undefined);

  return (
    <div className="w-1/2 md:w-[180px]">
      <Select
        value={isActive === undefined ? "all" : isActive ? "active" : "deleted"}
        onValueChange={(value) => {
          if (value === "all") setIsActive(undefined);
          else if (value === "deleted") setIsActive(false);
          else setIsActive(true);
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
