import Link from "next/link";
import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarTrigger,
} from "../../shared/components/ui/sidebar";
import { routes } from "@/shared/routes";
import {
  Calculator,
  Home,
  ListTodo,
  MapPin,
  Network,
  Users,
} from "lucide-react";

const items = {
  Main: [{ label: "Главная", href: routes.home, icon: Home }],
  Tools: [
    { label: "Счетчик", href: routes.counter, icon: Calculator },
    { label: "Список дел", href: routes.todo, icon: ListTodo },
  ],
  Management: [
    { label: "Подразделения", href: routes.departments, icon: Network },
    { label: "Локации", href: routes.locations, icon: MapPin },
    { label: "Позиции", href: routes.positions, icon: Users },
  ],
};

export function AppSidebar() {
  return (
    <Sidebar className="border-none">
      <SidebarHeader>
        <div className="flex items-center gap-3">
          <SidebarTrigger />
          <Link href={routes.home}>
            <div className="flex items-center justify-center h-7 rounded-full bg-primary text-primary-foreground shadow-md font-semibold text-sm">
              <span className="px-1">Directory Service</span>
            </div>
          </Link>
        </div>
      </SidebarHeader>
      <SidebarContent className="pt-2 pl-2">
        <SidebarGroup>
          <SidebarMenu>
            {items.Main.map((item) => (
              <SidebarMenuItem key={item.href}>
                <SidebarMenuButton asChild>
                  <Link key={item.href} href={item.href}>
                    {item.icon && <item.icon className="mr-3 h-4 w-4 inline" />}
                    {item.label}
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>

        <SidebarGroup>
          <SidebarGroupLabel>Tools</SidebarGroupLabel>
          <SidebarMenu>
            {items.Tools.map((item) => (
              <SidebarMenuItem key={item.href}>
                <SidebarMenuButton asChild>
                  <Link key={item.href} href={item.href}>
                    {item.icon && <item.icon className="mr-3 h-4 w-4 inline" />}
                    {item.label}
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>

        <SidebarGroup>
          <SidebarGroupLabel>Management</SidebarGroupLabel>
          <SidebarMenu>
            {items.Management.map((item) => (
              <SidebarMenuItem key={item.href}>
                <SidebarMenuButton asChild>
                  <Link key={item.href} href={item.href}>
                    {item.icon && <item.icon className="mr-3 h-4 w-4 inline" />}
                    {item.label}
                  </Link>
                </SidebarMenuButton>
              </SidebarMenuItem>
            ))}
          </SidebarMenu>
        </SidebarGroup>
      </SidebarContent>
    </Sidebar>
  );
}
