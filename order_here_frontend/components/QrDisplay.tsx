"use client";

import { useEffect, useMemo, useState } from "react";
import QRCode from "qrcode";
import { generateTableQr } from "@/lib/api";

type Props = {
  tableId: string;
};

export function QrDisplay({ tableId }: Props) {
  const [loading, setLoading] = useState(false);
  const [token, setToken] = useState<string | null>(null);
  const [qrUrl, setQrUrl] = useState<string | null>(null);
  const [qrDataUrl, setQrDataUrl] = useState<string | null>(null);
  const [generatedAtUtc, setGeneratedAtUtc] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const qrValue = useMemo(() => {
    if (!qrUrl) return "";
    return qrUrl;
  }, [qrUrl]);

  async function onGenerate() {
    setError(null);
    setLoading(true);
    try {
      const res = await generateTableQr(tableId);
      setToken(res.token);
      setQrUrl(res.qrUrl);
      setGeneratedAtUtc(res.generatedAtUtc);

      const dataUrl = await QRCode.toDataURL(res.qrUrl, {
        errorCorrectionLevel: "M",
        margin: 2,
        scale: 8,
      });
      setQrDataUrl(dataUrl);
    } catch (e: any) {
      setError(e?.message ?? "Failed to generate QR");
      setToken(null);
      setQrUrl(null);
      setQrDataUrl(null);
      setGeneratedAtUtc(null);
    } finally {
      setLoading(false);
    }
  }

  function onDownload() {
    if (!qrDataUrl) return;
    const a = document.createElement("a");
    a.href = qrDataUrl;
    a.download = `table-${tableId}-qr.png`;
    a.click();
  }

  // auto-generate ครั้งแรกเมื่อเข้า page (optional)
  useEffect(() => {
    onGenerate();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [tableId]);

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        <button
          onClick={onGenerate}
          disabled={loading}
          className="px-4 py-2 rounded bg-black text-white disabled:opacity-50"
        >
          {loading ? "Generating..." : "Generate QR"}
        </button>

        <button
          onClick={onDownload}
          disabled={!qrDataUrl}
          className="px-4 py-2 rounded border disabled:opacity-50"
        >
          Download PNG
        </button>
      </div>

      {error && (
        <div className="text-sm text-red-600 border border-red-200 bg-red-50 p-3 rounded">
          {error}
        </div>
      )}

      {qrDataUrl && (
        <div className="border rounded p-4 bg-white space-y-3">
          <div className="text-sm text-gray-600">
            Table ID: <span className="font-mono">{tableId}</span>
          </div>

          <img src={qrDataUrl} alt="QR Code" className="w-64 h-64" />

          <div className="text-sm">
            <div className="text-gray-600">QR URL</div>
            <div className="font-mono break-all">{qrValue}</div>
          </div>

          {token && (
            <div className="text-sm text-gray-600">
              Token: <span className="font-mono break-all">{token}</span>
            </div>
          )}

          {generatedAtUtc && (
            <div className="text-sm text-gray-600">
              GeneratedAtUtc: <span className="font-mono">{generatedAtUtc}</span>
            </div>
          )}
        </div>
      )}
    </div>
  );
}
