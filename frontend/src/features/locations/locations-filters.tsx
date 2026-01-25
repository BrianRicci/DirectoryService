import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";

export function LocationsFilter() {
  return (
    <div className="flex items-center gap-3">
      <Input
        value={search}
        onChange={(e) => setSearch(e.target.value)}
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
