import { Button } from "@/shared/components/ui/button";
import { routes } from "@/shared/routes";
import Link from "next/link";
import { SidebarTrigger } from "../../shared/components/ui/sidebar";

export default function Header() {
  return (
    <header className="sticky top-0 z-50 w-full bg-black backdrop-blur border-b border-none">
      <div className="mx-auto px-4 py-1 flex items-center justify-between">
        {/* Left: logo + sidebar trigger */}
        <div className="flex items-center gap-3">
          <SidebarTrigger />
          <Link href={routes.home}>
            <div className="flex items-center justify-center h-7 rounded-full bg-primary text-primary-foreground shadow-md font-semibold text-sm">
              <span className="px-1">Directory Service</span>
            </div>
          </Link>
        </div>

        {/* Right: actions + avatar */}
        <div className="flex items-center gap-3">
          <Button variant="ghost" size="sm" className="hidden md:inline-flex">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              className="w-4 h-4"
              fill="none"
              stroke="currentColor"
            >
              <path
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 5v14M5 12h14"
              />
            </svg>
          </Button>

          <Button variant="ghost" size="sm" className="hidden md:inline-flex">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              viewBox="0 0 24 24"
              className="w-4 h-4"
              fill="none"
              stroke="currentColor"
            >
              <path
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M15 17h5l-1.405-1.405A2.032 2.032 0 0 1 18.5 14.5V11a6.5 6.5 0 1 0-13 0v3.5c0 .538-.214 1.055-.595 1.445L3 17h5m4 0v1a3 3 0 1 1-6 0v-1h6z"
              />
            </svg>
          </Button>

          <div className="w-9 h-9 rounded-full bg-muted flex items-center justify-center text-sm font-medium text-muted-foreground">
            BR
          </div>
        </div>
      </div>
    </header>
  );
}
