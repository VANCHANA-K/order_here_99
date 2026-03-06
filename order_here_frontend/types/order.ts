export type CreateOrderItemRequest = {
  menuItemId: string;
  quantity: number;
};

export type CreateOrderViaQrRequest = {
  tableId: string;
  items: CreateOrderItemRequest[];
  idempotencyKey?: string;
};

export type CreateOrderViaQrResponse = {
  orderId: string;
  status: "PENDING" | string;
  createdAtUtc: string;
};
