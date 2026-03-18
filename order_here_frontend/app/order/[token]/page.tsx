"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";

import { resolveQr } from "@/lib/api";
import { saveTableSession } from "@/lib/session";
import type { QrResolveResponse, ApiErrorResponse } from "@/types/qr";
import QrErrorState from "@/components/QrErrorState";

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

  return {
    errorCode: "UNEXPECTED_ERROR",
    message: "Unexpected error",
  };
}

export default function OrderEntryPage() {
  const params = useParams();
  const router = useRouter();
  const token = typeof params.token === "string" ? params.token : "";

  const [data, setData] = useState<QrResolveResponse | null>(null);
  const [error, setError] = useState<ApiErrorResponse | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!token) {
      setLoading(false);
      return;
    }

    async function load() {
      try {
        const result = await resolveQr(token);
        setData(result);

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
  }, [token, router]);

  if (loading) {
    return <div className="p-10 text-center">Loading QR...</div>;
  }

  if (!token) {
    return <QrErrorState title="Invalid QR" message="Missing QR token." />;
  }

  if (error) {
    switch (error.errorCode) {
      case "QR_EXPIRED":
        return (
          <QrErrorState
            title="QR Expired"
            message="This QR code has expired. Please ask staff for a new QR code."
          />
        );

      case "QR_INACTIVE":
        return (
          <QrErrorState
            title="QR Disabled"
            message="This QR code is disabled. Please ask staff for a new QR code."
          />
        );

      case "TABLE_INACTIVE":
        return (
          <QrErrorState
            title="Table Closed"
            message="This table is currently unavailable. Please contact staff."
          />
        );

      case "QR_NOT_FOUND":
      case "TABLE_NOT_FOUND":
      case "QR_INVALID":
      case "NOT_FOUND":
        return (
          <QrErrorState
            title="Invalid QR"
            message="This QR code is invalid or could not be used."
          />
        );

      default:
        return (
          <QrErrorState
            title="System Error"
            message="Something went wrong. Please try again or contact staff."
          />
        );
    }
  }

  if (!data) {
    return <QrErrorState title="QR Error" message="Unable to load QR information." />;
  }

  return null;
}
