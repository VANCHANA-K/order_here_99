"use client";

import { TableDto } from "@/types/table";
import { disableTable, enableTable } from "@/lib/api";
import { useState } from "react";

interface Props {
  tables: TableDto[];
  refresh: () => Promise<void>;
}

export function TableList({ tables, refresh }: Props) {
  const [loadingId, setLoadingId] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function toggle(table: TableDto) {
    setError(null);
    setLoadingId(table.id);

    try {
      if (table.status === "Active") {
        await disableTable(table.id);
      } else {
        await enableTable(table.id);
      }

      await refresh(); // ✅ sync with backend
    } catch (err: any) {
      setError(err?.message ?? "Unexpected error");
    } finally {
      setLoadingId(null);
    }
  }

  if (tables.length === 0) {
    return <div className="text-gray-500">No tables found.</div>;
  }

  return (
    <div className="space-y-4">
      {error && <div className="text-red-500 text-sm">{error}</div>}

      {tables.map((table) => (
        <div
          key={table.id}
          className="border p-4 rounded flex justify-between items-center bg-white shadow-sm"
        >
          <div>
            <div className="font-medium">{table.name}</div>
            <div className="text-sm text-gray-500">Status: {table.status}</div>
          </div>

          <button
            onClick={() => toggle(table)}
            disabled={loadingId === table.id}
            className={`px-3 py-1 rounded text-white text-sm ${
              table.status === "Active"
                ? "bg-red-500 hover:bg-red-600"
                : "bg-green-500 hover:bg-green-600"
            }`}
          >
            {loadingId === table.id
              ? "Processing..."
              : table.status === "Active"
              ? "Disable"
              : "Enable"}
          </button>
        </div>
      ))}
    </div>
  );
}
