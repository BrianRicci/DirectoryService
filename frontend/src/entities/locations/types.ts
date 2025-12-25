export type Location = {
  id: string;
  name: string;
  address: Address;
  timezone: string;
  isActive: boolean;
  isDelete?: boolean;
  createdAt: Date;
  updatedAt: Date;
};

export type Address = {
  country: string;
  region: string;
  city: string;
  street: string;
  house: string;
};
