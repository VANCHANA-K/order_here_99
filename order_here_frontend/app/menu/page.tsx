"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

import { getTableSession } from "@/lib/session";
import { getMenuByTable } from "@/lib/api";
import type { MenuItemDto } from "@/types/menu";
import type { CreateOrderItemRequest, CreateOrderViaQrResponse } from "@/types/order";
import { MenuList } from "@/components/MenuList";
import { OrderSubmitBar } from "@/components/OrderSubmitBar";

export default function MenuPage() {
  const router = useRouter();

  const [items, setItems] = useState<MenuItemDto[]>([]);
  const [quantities, setQuantities] = useState<Record<string, number>>({});
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successOrder, setSuccessOrder] = useState<CreateOrderViaQrResponse | null>(null);

  useEffect(() => {
    async function load() {
      const session = getTableSession();
      if (!session) {
        setLoading(false);
        router.replace("/");
        return;
      }

      try {
        setLoading(true);
        setError(null);
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

  function handleChangeQuantity(menuItemId: string, quantity: number) {
    setQuantities((prev) => ({ ...prev, [menuItemId]: quantity }));
  }

  const selectedItems: CreateOrderItemRequest[] = items
    .filter((x) => x.isAvailable && (quantities[x.id] ?? 0) > 0)
    .map((x) => ({
      menuItemId: x.id,
      quantity: quantities[x.id],
    }));

  return (
    <div className="max-w-md mx-auto p-4 pb-28">
      <div className="mb-4">
        <h1 className="text-xl font-bold">Menu</h1>
        <p className="text-sm text-gray-500">Items for your table session</p>
      </div>

      {successOrder && (
        <div className="mb-3 rounded border border-green-200 bg-green-50 p-3 text-sm text-green-700">
          Order created successfully ({successOrder.status}): {successOrder.orderId}
        </div>
      )}

      {loading && <div className="text-sm">Loading menu...</div>}
      {error && <div className="text-sm text-red-600">{error}</div>}
      {!loading && !error && (
        <>
          <MenuList
            items={items}
            quantities={quantities}
            onChangeQuantity={handleChangeQuantity}
          />
          <OrderSubmitBar
            items={selectedItems}
            onSuccess={(result) => {
              setSuccessOrder(result);
              setQuantities({});
            }}
          />
        </>
      )}
    </div>
  );
}
