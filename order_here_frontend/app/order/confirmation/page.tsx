import { Suspense } from "react";
import ConfirmationClient from "./ConfirmationClient";

function ConfirmationLoading() {
  return (
    <div className="flex min-h-screen items-center justify-center p-6">
      <div className="w-full max-w-md rounded-xl border bg-white p-6 text-center shadow-sm">
        <h1 className="text-2xl font-bold">Loading Confirmation...</h1>
      </div>
    </div>
  );
}

export default function OrderConfirmationPage() {
  return (
    <Suspense fallback={<ConfirmationLoading />}>
      <ConfirmationClient />
    </Suspense>
  );
}
