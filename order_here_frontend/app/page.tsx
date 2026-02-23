import Link from "next/link";

export default function Home() {
  return (
    <main className="min-h-screen flex items-center justify-center p-6">
      <div className="max-w-md w-full space-y-4">
        <h1 className="text-2xl font-bold">Order Here</h1>
        <p className="text-lg text-gray-600">Staff UI skeleton (Sprint 3)</p>

        <Link
          className="inline-flex items-center justify-center rounded-lg bg-black text-white px-4 py-2"
          href="/staff/tables"
        >
          Go to Staff Tables
        </Link>
      </div>
    </main>
  );
}
