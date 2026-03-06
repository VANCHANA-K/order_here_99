"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { getTableSession } from "@/lib/session";

export default function ConfirmationClient() {
  const params = useSearchParams();
  const router = useRouter();

  const orderId = params.get("orderId");
  const status = params.get("status");
  const session = getTableSession();

  if (!session) {
    return (
      <div className="flex min-h-screen items-center justify-center p-6">
        <div className="w-full max-w-md rounded-xl border bg-white p-6 text-center shadow-sm">
          <h1 className="text-2xl font-bold">Session Expired</h1>
          <p className="mt-2 text-sm text-gray-600">
            Your table session could not be found.
          </p>

          <button
            onClick={() => router.push("/")}
            className="mt-6 w-full rounded-lg bg-black px-4 py-3 text-white"
          >
            Go Home
          </button>
        </div>
      </div>
    );
  }

  if (!orderId) {
    return (
      <div className="flex min-h-screen items-center justify-center p-6">
        <div className="w-full max-w-md rounded-xl border bg-white p-6 text-center shadow-sm">
          <h1 className="text-2xl font-bold">Missing Order</h1>
          <p className="mt-2 text-sm text-gray-600">
            We could not find order confirmation details.
          </p>

          <button
            onClick={() => router.replace("/menu")}
            className="mt-6 w-full rounded-lg bg-black px-4 py-3 text-white"
          >
            Back to Menu
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center p-6">
      <div className="w-full max-w-md rounded-xl border bg-white p-6 text-center shadow-sm">
        <h1 className="text-2xl font-bold">Order Placed</h1>

        <p className="mt-2 text-sm text-gray-600">
          Your order has been sent successfully.
        </p>

        <div className="mt-6 rounded-lg border bg-gray-50 p-4 text-left">
          <p className="text-xs text-gray-500">Table</p>
          <p className="text-lg font-semibold">{session.tableCode}</p>

          <p className="mt-4 text-xs text-gray-500">Order ID</p>
          <p className="break-all text-sm">{orderId}</p>

          <p className="mt-4 text-xs text-gray-500">Status</p>
          <p className="text-sm font-medium">{status ?? "PENDING"}</p>
        </div>

        <button
          onClick={() => router.replace("/menu")}
          className="mt-6 w-full rounded-lg bg-black px-4 py-3 text-white"
        >
          Place Another Order
        </button>
      </div>
    </div>
  );
}
