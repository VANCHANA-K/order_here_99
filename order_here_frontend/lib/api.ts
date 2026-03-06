import type { TableDto } from "@/types/table";
import type { ApiErrorResponse, QrResolveResponse } from "@/types/qr";
import type { MenuItemDto } from "@/types/menu";
import type { CreateOrderViaQrRequest, CreateOrderViaQrResponse } from "@/types/order";

const BASE_URL = process.env.NEXT_PUBLIC_API_BASE_PATH?.replace(/\/$/, "") ?? "";

export type GenerateQrResponse = {
  tableId: string;
  token: string;
  qrUrl: string;
  generatedAtUtc: string;
};

async function parseError(res: Response): Promise<ApiErrorResponse | null> {
  try {
    return (await res.json()) as ApiErrorResponse;
  } catch {
    return null;
  }
}

// getTables - GET /tables
export async function getTables(): Promise<TableDto[]> {
  const res = await fetch(`${BASE_URL}/api/v1/tables`, {
    cache: "no-store",
  });

  if (!res.ok) {
    throw new Error(`Failed to fetch tables (${res.status})`);
  }

  return res.json();
}
// disableTable - PATCH /tables/{id}/disable
export async function disableTable(id: string): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/v1/tables/${id}/disable`, {
    method: "PATCH",
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to disable table: ${text}`);
  }
}

// enableTable - POST /tables/{id}/activate
export async function enableTable(id: string): Promise<void> {
  const res = await fetch(`${BASE_URL}/api/v1/tables/${id}/activate`, {
    method: "POST",
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to enable table: ${text}`);
  }
}
// createTable - POST /tables
export async function createTable(name: string) {
  const res = await fetch(`${BASE_URL}/api/v1/tables`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    // Backend expects { code: string } per CreateTableRequest(Code)
    body: JSON.stringify({ code: name }),
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(`Failed to create table: ${text}`);
  }

  return res.json();
}

export async function generateTableQr(tableId: string): Promise<GenerateQrResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/tables/${tableId}/qr`, {
    method: "POST",
    headers: { "content-type": "application/json" },
  });

  if (!res.ok) {
    const err = await parseError(res);
    const msg = err
      ? `${err.errorCode}: ${err.message} (traceId=${err.traceId})`
      : "Failed to generate QR";
    throw new Error(msg);
  }

  return (await res.json()) as GenerateQrResponse;
}

export async function resolveQr(token: string): Promise<QrResolveResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/qr/${token}`, {
    cache: "no-store",
  });

  const data = await res.json();

  if (!res.ok) {
    const error = data as ApiErrorResponse;
    throw error;
  }

  return data as QrResolveResponse;
}

export async function getMenuByTable(tableId: string): Promise<MenuItemDto[]> {
  const res = await fetch(`${BASE_URL}/api/v1/tables/${tableId}/menu`, {
    cache: "no-store",
  });

  const data = await res.json();

  if (!res.ok) {
    throw data;
  }

  return data as MenuItemDto[];
}

export async function createOrderViaQr(
  payload: CreateOrderViaQrRequest
): Promise<CreateOrderViaQrResponse> {
  const res = await fetch(`${BASE_URL}/api/v1/orders/qr`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });

  const data = await res.json().catch(() => null);

  if (!res.ok) {
    if (data) {
      throw data as ApiErrorResponse;
    }

    throw {
      errorCode: "REQUEST_FAILED",
      message: `Failed to create order (${res.status})`,
      traceId: "",
    } as ApiErrorResponse;
  }

  return data as CreateOrderViaQrResponse;
}
