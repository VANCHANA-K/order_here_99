export type TableStatus = "Active" | "Inactive";

export interface TableDto {
  id: string;
  name: string;
  status: TableStatus;
}
