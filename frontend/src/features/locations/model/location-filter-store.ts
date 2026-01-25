import { create } from "zustand";
import { PAGE_SIZE } from "./use-locations-list";

export type LocationsFilterState = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
};

const initialState: LocationsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
};

const useLocationsFilterStore = create<LocationsFilterState>((set) => ({
  ...initialState,
  setSearch: (input: string) =>
    set(() => ({ search: input.trim() || undefined })),
  setIsActive: (isActive: boolean | undefined) => set(() => ({ isActive })),
}));
