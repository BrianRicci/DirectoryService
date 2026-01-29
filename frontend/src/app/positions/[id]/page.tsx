import PositionDetail from "@/features/positions/position-detail";

export default async function PositionDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  return <PositionDetail positionId={id} />;
}
