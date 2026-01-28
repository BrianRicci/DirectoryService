"use client";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/shared/components/ui/select";
import { useState } from "react";

export function PositionsFilterDepartments() {
  const [department, setDepartment] = useState<string>("all");

  // Mock departments
  const departments = [
    { id: "1", name: "IT отдел" },
    { id: "2", name: "HR отдел" },
    { id: "3", name: "Маркетинг" },
  ];

  return (
    <div className="w-1/2 md:w-[200px]">
      <Select value={department} onValueChange={setDepartment}>
        <SelectTrigger className="w-full md:w-[200px]">
          <SelectValue placeholder="Подразделение" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">Все подразделения</SelectItem>
          {departments.map((dept) => (
            <SelectItem key={dept.id} value={dept.id}>
              {dept.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}
