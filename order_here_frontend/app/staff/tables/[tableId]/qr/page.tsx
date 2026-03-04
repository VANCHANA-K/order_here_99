import { QrDisplay } from "@/components/QrDisplay";

type Props = {
  params: Promise<{ tableId: string }>;
};

export default async function StaffTableQrPage({ params }: Props) {
  const { tableId } = await params;

  return (
    <div className="max-w-xl mx-auto p-6 space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Staff – Table QR</h1>
        <a href="/staff/tables" className="text-sm underline">
          Back to tables
        </a>
      </div>

      <QrDisplay tableId={tableId} />
    </div>
  );
}
