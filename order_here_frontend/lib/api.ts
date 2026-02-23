import type { TableDto } from "@/types/table";

const BASE_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5132";

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
