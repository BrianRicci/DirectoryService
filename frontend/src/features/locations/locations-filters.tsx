import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import {
  setFilterIsActive,
  setFilterSearch,
  useGetLocationFilter,
} from "./model/location-filter-store";
import { useDebounce } from "use-debounce";
import { useEffect, useState } from "react";

export function LocationsFilter() {
  const { search, isActive } = useGetLocationFilter();

  const [localSearch, setLocalSearch] = useState(search ?? "");
  const [debouncedSearch] = useDebounce(localSearch, 300);

  useEffect(() => {
    setFilterSearch(debouncedSearch);
  }, [debouncedSearch]);

  return (
    <div className="flex items-center gap-3">
      <Input
        value={localSearch}
        onChange={(e) => setLocalSearch(e.target.value)}
        placeholder="Поиск по названию"
        className="min-w-[260px] bg-slate-800/50"
      />

      <Select
        value={isActive === undefined ? "all" : isActive ? "active" : "deleted"}
        onValueChange={(value) => {
          if (value === "all") setFilterIsActive(undefined);
          else if (value === "deleted") setFilterIsActive(false);
          else setFilterIsActive(true);
        }}
      >
        <SelectTrigger className="w-[180px]">
          <SelectValue placeholder="Статус" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все</SelectItem>
          <SelectItem value="active">Активные</SelectItem>
          <SelectItem value="deleted">Удалённые</SelectItem>
        </SelectContent>
      </Select>
    </div>
  );
}
