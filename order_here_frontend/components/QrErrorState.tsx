import Link from "next/link";

type Props = {
  title: string;
  message: string;
};

export default function QrErrorState({ title, message }: Props) {
  return (
    <div className="flex min-h-screen items-center justify-center p-6">
      <div className="w-full max-w-md rounded-xl border bg-white p-6 text-center shadow-sm">
        <div className="text-4xl">⚠️</div>

        <h1 className="mt-4 text-2xl font-bold">{title}</h1>

        <p className="mt-3 text-sm text-gray-600">{message}</p>

        <Link href="/" className="mt-6 inline-block rounded-lg bg-black px-4 py-3 text-white">
          Back Home
        </Link>
      </div>
    </div>
  );
}
