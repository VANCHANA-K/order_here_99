"use client";

import { useRouter } from "next/navigation";
import { useRef, useState } from "react";
import { createOrderViaQr } from "@/lib/api";
import { getTableSession } from "@/lib/session";
import type { ApiErrorResponse } from "@/types/qr";
import type { CreateOrderItemRequest, CreateOrderViaQrResponse } from "@/types/order";

type Props = {
  items: CreateOrderItemRequest[];
  onSuccess: (result: CreateOrderViaQrResponse) => void;
};

function makeIdempotencyKey() {
  return crypto.randomUUID();
}

function toApiError(err: unknown): ApiErrorResponse {
  if (
    typeof err === "object" &&
    err !== null &&
    "errorCode" in err &&
    "message" in err &&
    typeof (err as { errorCode: unknown }).errorCode === "string" &&
    typeof (err as { message: unknown }).message === "string"
  ) {
    return err as ApiErrorResponse;
  }

  if (err instanceof Error) {
    return {
      errorCode: "REQUEST_FAILED",
      message: err.message,
      traceId: "",
    };
  }

  return {
    errorCode: "REQUEST_FAILED",
    message: "Failed to place order.",
    traceId: "",
  };
}

export default function OrderSubmitBar({ items, onSuccess }: Props) {
  const router = useRouter();

  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isRedirecting, setIsRedirecting] = useState(false);
  const [error, setError] = useState<ApiErrorResponse | null>(null);
  const submitLockRef = useRef(false);
  const submitKeyRef = useRef<string | null>(null);

  async function handleSubmit() {
    if (submitLockRef.current || isSubmitting || isRedirecting || items.length === 0) {
      return;
    }
    submitLockRef.current = true;

    const session = getTableSession();

    if (!session) {
      setError({
        errorCode: "NO_SESSION",
        message: "No table session found.",
        traceId: "",
      });
      submitLockRef.current = false;
      return;
    }

    setError(null);
    setIsSubmitting(true);

    try {
      if (!submitKeyRef.current) {
        submitKeyRef.current = makeIdempotencyKey();
      }

      const result = await createOrderViaQr({
        tableId: session.tableId,
        items,
        idempotencyKey: submitKeyRef.current,
      });

      onSuccess(result);
      submitKeyRef.current = null;
      setIsRedirecting(true);

      setTimeout(() => {
        router.push(
          `/order/confirmation?orderId=${encodeURIComponent(result.orderId)}&status=${encodeURIComponent(result.status)}`
        );
      }, 450);
    } catch (err: unknown) {
      setError(toApiError(err));
    } finally {
      submitLockRef.current = false;
      setIsSubmitting(false);
    }
  }

  return (
    <div className="fixed bottom-0 left-0 right-0 border-t bg-white p-4">
      {isRedirecting && (
        <div className="mb-3 rounded-lg border border-green-200 bg-green-50 p-3 text-sm text-green-700">
          Order created successfully. Redirecting to confirmation...
        </div>
      )}

      {error && (
        <div className="mb-3 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
          {error.message}
        </div>
      )}

      <button
        onClick={handleSubmit}
        disabled={isSubmitting || isRedirecting || items.length === 0}
        className="w-full rounded-lg bg-black py-3 text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isRedirecting ? "Opening confirmation..." : isSubmitting ? "Placing order..." : "Place Order"}
      </button>
    </div>
  );
}
