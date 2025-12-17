"use client";

import { SidebarProvider } from "@/shared/components/ui/sidebar";
import { QueryClientProvider } from "@tanstack/react-query";
import { AppSidebar } from "../sidebar/app-sidebar";
import Header from "../header/header";
import { queryClient } from "@/shared/api/query-client";

export default function Layout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <QueryClientProvider client={queryClient}>
      <SidebarProvider defaultOpen={false}>
        <div className="h-screen w-full">
          <AppSidebar />
          <div className="flex min-h-screen flex-col">
            <Header />
            <main className="flex-1 bg-black p-4 overflow-auto">
              {children}
            </main>
          </div>
        </div>
      </SidebarProvider>
    </QueryClientProvider>
  );
}
