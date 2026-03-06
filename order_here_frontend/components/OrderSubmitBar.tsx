"use client";

import { useRef, useState } from "react";
import { createOrderViaQr } from "@/lib/api";
import { getTableSession } from "@/lib/session";
import type { ApiErrorResponse } from "@/types/qr";
import type { CreateOrderItemRequest, CreateOrderViaQrResponse } from "@/types/order";

function makeIdempotencyKey() {
  return crypto.randomUUID();
}

function toApiError(error: unknown): ApiErrorResponse {
  if (
    typeof error === "object" &&
    error !== null &&
    "errorCode" in error &&
    "message" in error &&
    typeof (error as { errorCode: unknown }).errorCode === "string" &&
    typeof (error as { message: unknown }).message === "string"
  ) {
    return error as ApiErrorResponse;
  }

  if (error instanceof Error) {
    return { errorCode: "REQUEST_FAILED", message: error.message, traceId: "" };
  }

  return { errorCode: "REQUEST_FAILED", message: "Failed to place order", traceId: "" };
}

export function OrderSubmitBar({
  items,
  onSuccess,
}: {
  items: CreateOrderItemRequest[];
  onSuccess: (result: CreateOrderViaQrResponse) => void;
}) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<ApiErrorResponse | null>(null);
  const submitLockRef = useRef(false);
  const submitKeyRef = useRef<string | null>(null);

  async function submit() {
    if (submitLockRef.current || isSubmitting) return;
    submitLockRef.current = true;
    setError(null);

    const session = getTableSession();
    if (!session) {
      setError({ errorCode: "NO_SESSION", message: "No table session", traceId: "" });
      submitLockRef.current = false;
      return;
    }

    setIsSubmitting(true);
    try {
      if (!submitKeyRef.current) {
        submitKeyRef.current = makeIdempotencyKey();
      }

      const res = await createOrderViaQr({
        tableId: session.tableId,
        items,
        idempotencyKey: submitKeyRef.current,
      });

      submitKeyRef.current = null;
      onSuccess(res);
    } catch (error: unknown) {
      setError(toApiError(error));
    } finally {
      submitLockRef.current = false;
      setIsSubmitting(false);
    }
  }

  const disabled = isSubmitting || items.length === 0;

  return (
    <div className="fixed bottom-0 left-0 right-0 border-t bg-white p-4">
      {error && <div className="mb-2 rounded border p-2 text-sm">{error.message}</div>}

      <button
        onClick={submit}
        disabled={disabled}
        className="w-full rounded bg-black py-3 text-white disabled:opacity-50"
      >
        {isSubmitting ? "Placing order..." : "Place Order"}
      </button>
    </div>
  );
}
