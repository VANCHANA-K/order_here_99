import type { MenuItemDto } from "@/types/menu";

type Props = {
  items: MenuItemDto[];
  quantities: Record<string, number>;
  onIncrease: (id: string) => void;
  onDecrease: (id: string) => void;
};

export function MenuList({
  items,
  quantities,
  onIncrease,
  onDecrease,
}: Props) {
  if (items.length === 0) {
    return <div className="text-sm text-gray-500">No menu items available.</div>;
  }

  return (
    <div className="space-y-3">
      {items.map((x) => {
        const disabled = !x.isAvailable;
        const qty = quantities[x.id] ?? 0;

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
              <div className="mt-1 font-bold">{x.price}฿</div>
            </div>

            <div className="flex flex-col items-end gap-2">
              {disabled ? (
                <button
                  disabled
                  className="rounded-lg bg-gray-200 px-3 py-2 text-sm text-gray-500"
                >
                  Unavailable
                </button>
              ) : (
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => onDecrease(x.id)}
                    className="rounded border px-3 py-1"
                  >
                    -
                  </button>

                  <span className="min-w-[24px] text-center">{qty}</span>

                  <button
                    onClick={() => onIncrease(x.id)}
                    className="rounded border px-3 py-1"
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
