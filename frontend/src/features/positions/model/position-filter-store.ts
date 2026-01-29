import { SortOptions } from "@/shared/api/types";

export type LocationsFilterState = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
  sortBy?: SortOptions["sortBy"];
  sortOrder?: SortOptions["sortOrder"];
};
