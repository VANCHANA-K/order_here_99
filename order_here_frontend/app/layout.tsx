import "./globals.css";
import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Order Here",
  description: "QR Food Ordering Platform",
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className="min-h-screen bg-gray-100">{children}</body>
    </html>
  );
}
