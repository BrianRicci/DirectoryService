import { PAGE_SIZE } from "@/shared/api/constants";
import { SortOptions } from "@/shared/api/types";
import { create } from "zustand";
import { useShallow } from "zustand/react/shallow";

export type PositionsFilterState = {
  search?: string;
  isActive?: boolean;
  pageSize: number;
  sortBy?: SortOptions["sortBy"];
  sortOrder?: SortOptions["sortOrder"];
};

type Actions = {
  setSearch: (input: PositionsFilterState["search"]) => void;
  setIsActive: (isActive: PositionsFilterState["isActive"]) => void;
  setSortBy: (sortBy: PositionsFilterState["sortBy"]) => void;
  setSortOrder: (sortOrder: PositionsFilterState["sortOrder"]) => void;
};

type PositionsFilterStore = PositionsFilterState & Actions;

const initialState: PositionsFilterState = {
  search: "",
  isActive: undefined,
  pageSize: PAGE_SIZE,
  sortBy: "createdAt",
  sortOrder: "asc",
};

export const usePositionsFilterStore = create<PositionsFilterStore>((set) => ({
  ...initialState,
  setSearch: (input: PositionsFilterState["search"]) =>
    set(() => ({ search: input?.trim() || undefined })),
  setIsActive: (isActive: PositionsFilterState["isActive"]) =>
    set(() => ({ isActive })),
  setSortBy: (sortBy: PositionsFilterState["sortBy"]) =>
    set(() => ({ sortBy })),
  setSortOrder: (sortOrder: PositionsFilterState["sortOrder"]) =>
    set(() => ({ sortOrder })),
}));

export const useGetPositionFilter = () => {
  return usePositionsFilterStore(
    useShallow((state) => ({
      search: state.search,
      isActive: state.isActive,
      pageSize: state.pageSize,
      sortBy: state.sortBy,
      sortOrder: state.sortOrder,
    })),
  );
};

export const setFilterSearch = (input: PositionsFilterState["search"]) => {
  usePositionsFilterStore.getState().setSearch(input || "");
};

export const setFilterIsActive = (input: PositionsFilterState["isActive"]) => {
  usePositionsFilterStore.getState().setIsActive(input);
};

export const setFilterSortBy = (input: PositionsFilterState["sortBy"]) => {
  usePositionsFilterStore.getState().setSortBy(input);
};

export const setFilterSortOrder = (
  input: PositionsFilterState["sortOrder"],
) => {
  usePositionsFilterStore.getState().setSortOrder(input);
};
