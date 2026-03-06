import type { MenuItemDto } from "@/types/menu";

type Props = {
  items: MenuItemDto[];
  quantities: Record<string, number>;
  onChangeQuantity: (menuItemId: string, quantity: number) => void;
};

export function MenuList({ items, quantities, onChangeQuantity }: Props) {
  if (items.length === 0) {
    return <div className="text-sm text-gray-500">No menu items available.</div>;
  }

  return (
    <div className="space-y-3">
      {items.map((x) => {
        const disabled = !x.isAvailable;

        return (
          <div
            key={x.id}
            className={`rounded-xl border p-4 flex items-center justify-between ${
              disabled ? "opacity-60" : "bg-white"
            }`}
          >
            <div>
              <div className="font-semibold text-base">{x.name}</div>
              <div className="text-xs text-gray-500">{x.code}</div>
            </div>

            <div className="flex items-center gap-3">
              <div className="font-bold">{x.price}฿</div>

              {disabled ? (
                <button
                  disabled
                  className="px-3 py-2 rounded-lg text-sm font-medium bg-gray-200 text-gray-500 cursor-not-allowed"
                >
                  Unavailable
                </button>
              ) : (
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => onChangeQuantity(x.id, Math.max((quantities[x.id] ?? 0) - 1, 0))}
                    disabled={(quantities[x.id] ?? 0) === 0}
                    className="w-8 h-8 rounded border text-sm disabled:opacity-40"
                  >
                    -
                  </button>
                  <span className="min-w-6 text-center text-sm font-medium">
                    {quantities[x.id] ?? 0}
                  </span>
                  <button
                    onClick={() => onChangeQuantity(x.id, (quantities[x.id] ?? 0) + 1)}
                    className="w-8 h-8 rounded bg-black text-white text-sm"
                  >
                    +
                  </button>
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
