import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";
import { SortOptions } from "@/shared/api/types";
import { PAGE_SIZE } from "@/shared/api/constants";

export type LocationsFilterState = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
  sortBy?: SortOptions["sortBy"];
  sortOrder?: SortOptions["sortOrder"];
};

type Actions = {
  setSearch: (input: LocationsFilterState["search"]) => void;
  setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
  setSortBy: (sortBy: LocationsFilterState["sortBy"]) => void;
  setSortOrder: (sortOrder: LocationsFilterState["sortOrder"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
  sortBy: "createdAt",
  sortOrder: "asc",
};

export const useLocationsFilterStore = create<LocationsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: LocationsFilterState["search"]) =>
    set(() => ({ search: input?.trim() || undefined })),
  setIsActive: (isActive: LocationsFilterState["isActive"]) =>
    set(() => ({ isActive })),
  setSortBy: (sortBy: LocationsFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortOrder: (sortOrder: LocationsFilterState["sortOrder"]) =>
    set(() => ({ sortOrder })),
}));

export const useGetLocationFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      sortBy: state.sortBy,
      sortOrder: state.sortOrder,
    })),
  );
};

export const setFilterSearch = (input: LocationsFilterState["search"]) => {
  useLocationsFilterStore.getState().setSearch(input || "");
};

export const setFilterIsActive = (input: LocationsFilterState["isActive"]) => {
  useLocationsFilterStore.getState().setIsActive(input);
};

export const setFilterSortBy = (input: LocationsFilterState["sortBy"]) => {
  useLocationsFilterStore.getState().setSortBy(input);
};

export const setFilterSortOrder = (
  input: LocationsFilterState["sortOrder"],
) => {
  useLocationsFilterStore.getState().setSortOrder(input);
};
