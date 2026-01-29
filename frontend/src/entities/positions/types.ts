export type Position = {
  positionId: string;
  name: string;
  description: string;
  isActive: boolean;
  isDelete?: boolean;
  createdAt: Date;
  updatedAt: Date;
  departments: Department[];
};

export type Department = {
  departmentId: string;
  name: string;
  identifier: string;
  parentId?: string;
  path: string;
  depth: number;
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
};
