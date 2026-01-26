import { create } from "zustand";
import { PAGE_SIZE } from "./use-locations-list";
import { useShallow } from "zustand/react/shallow";

export type LocationsFilterState = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
};

type Actions = {
  setSearch: (input: LocationsFilterState["search"]) => void;
  setIsActive: (isActive: LocationsFilterState["isActive"]) => void;
};

type LocationsFilterStore = LocationsFilterState & Actions;

const initialState: LocationsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
};

export const useLocationsFilterStore = create<LocationsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: LocationsFilterState["search"]) =>
    set(() => ({ search: input?.trim() || undefined })),
  setIsActive: (isActive: LocationsFilterState["isActive"]) =>
    set(() => ({ isActive })),
}));

export const useGetLocationFilter = () => {
  return useLocationsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
    })),
  );
};

export const setFilterSearch = (input: LocationsFilterState["search"]) => {
  useLocationsFilterStore.getState().setSearch(input || "");
};

export const setFilterIsActive = (input: LocationsFilterState["isActive"]) => {
  useLocationsFilterStore.getState().setIsActive(input);
};
