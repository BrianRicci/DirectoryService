"use client";

import { Input } from "@/shared/components/ui/input";
import { useState } from "react";

export function PositionsFilterSearch() {
  const [search, setSearch] = useState("");

  return (
    <Input
      value={search}
      onChange={(e) => setSearch(e.target.value)}
      placeholder="Поиск по названию"
      className="min-w-[260px] bg-slate-800/50 sm:mb-0"
    />
  );
}
