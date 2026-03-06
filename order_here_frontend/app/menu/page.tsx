"use client";

import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";

import { getTableSession } from "@/lib/session";
import { getMenuByTable } from "@/lib/api";
import type { MenuItemDto } from "@/types/menu";
import type { CreateOrderItemRequest, CreateOrderViaQrResponse } from "@/types/order";
import { MenuList } from "@/components/MenuList";
import OrderSubmitBar from "@/components/OrderSubmitBar";

export default function MenuPage() {
  const router = useRouter();

  const [items, setItems] = useState<MenuItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [quantities, setQuantities] = useState<Record<string, number>>({});

  useEffect(() => {
    async function load() {
      const session = getTableSession();

      if (!session) {
        setLoading(false);
        router.replace("/");
        return;
      }

      try {
        const data = await getMenuByTable(session.tableId);
        setItems(data);
      } catch {
        setError("Failed to load menu.");
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [router]);

  function increase(id: string) {
    setQuantities((prev) => ({
      ...prev,
      [id]: (prev[id] ?? 0) + 1,
    }));
  }

  function decrease(id: string) {
    setQuantities((prev) => ({
      ...prev,
      [id]: Math.max((prev[id] ?? 0) - 1, 0),
    }));
  }

  function resetQuantities() {
    setQuantities({});
  }

  function handleOrderSuccess(result: CreateOrderViaQrResponse) {
    void result;
    resetQuantities();
  }

  const orderItems: CreateOrderItemRequest[] = useMemo(() => {
    return items
      .map((x) => ({
        menuItemId: x.id,
        quantity: quantities[x.id] ?? 0,
      }))
      .filter((x) => x.quantity > 0);
  }, [items, quantities]);

  return (
    <div className="max-w-md mx-auto p-4 pb-28">
      <div className="mb-4">
        <h1 className="text-xl font-bold">Menu</h1>
        <p className="text-sm text-gray-500">Items for your table session</p>
      </div>

      {loading && <div className="text-sm">Loading menu...</div>}
      {error && <div className="text-sm text-red-600">{error}</div>}

      {!loading && !error && (
        <>
          <MenuList
            items={items}
            quantities={quantities}
            onIncrease={increase}
            onDecrease={decrease}
          />

          <OrderSubmitBar
            items={orderItems}
            onSuccess={handleOrderSuccess}
          />
        </>
      )}
    </div>
  );
}
