import type { TableDto } from "@/types/table";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5132";

export type ApiErrorResponse = {
  errorCode: string;
  message: string;
  traceId: string;
};

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
