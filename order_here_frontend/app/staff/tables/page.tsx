"use client";

import { useEffect, useState } from "react";
import { getTables } from "@/lib/api";
import { TableDto } from "@/types/table";
import { TableList } from "@/components/TableList";
import { CreateTableForm } from "@/components/CreateTableForm";

export default function StaffTablesPage() {
  const [tables, setTables] = useState<TableDto[]>([]);
  const [loading, setLoading] = useState(true);

  async function load() {
    setLoading(true);
    const data = await getTables();
    setTables(data);
    setLoading(false);
  }

  useEffect(() => {
    load();
  }, []);

  return (
    <div className="max-w-xl mx-auto p-6 space-y-6">
      <h1 className="text-2xl font-bold">Staff – Table Management</h1>

      {/* 🔵 Create Form กลับมาแล้ว */}
      <CreateTableForm onCreated={load} />

      {loading ? (
        <div>Loading...</div>
      ) : (
        <TableList tables={tables} refresh={load} />
      )}
    </div>
  );
}
