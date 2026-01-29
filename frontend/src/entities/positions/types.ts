export type Position = {
  positionId: string;
  name: string;
  description: string;
  isActive: boolean;
  isDelete?: boolean;
  createdAt: Date;
  updatedAt: Date;
  departmentIds: string[];
};
