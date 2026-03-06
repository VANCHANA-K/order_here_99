"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";

import { resolveQr } from "@/lib/api";
import type { ApiErrorResponse } from "@/types/qr";
import { saveTableSession } from "@/lib/session";

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
    return { errorCode: "REQUEST_FAILED", message: err.message, traceId: "" };
  }

  return { errorCode: "REQUEST_FAILED", message: "Failed to resolve QR token.", traceId: "" };
}

export default function OrderEntryPage() {
  const router = useRouter();
  const params = useParams();
  const token = params.token as string;

  const [error, setError] = useState<ApiErrorResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function load() {
      try {
        const result = await resolveQr(token);
        saveTableSession({
          tableId: result.tableId,
          tableCode: result.tableCode,
          createdAt: Date.now(),
        });
        router.replace("/menu");
      } catch (err: unknown) {
        setError(toApiError(err));
      } finally {
        setLoading(false);
      }
    }

    load();
  }, [router, token]);

  if (loading) {
    return <div className="p-10 text-center">Loading...</div>;
  }

  if (error) {
    return <div className="p-10 text-center text-red-500">{error.message}</div>;
  }

  return <div className="p-10 text-center">Redirecting to menu...</div>;
}
