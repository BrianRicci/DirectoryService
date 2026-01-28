import PositionDetail from "@/features/positions/position-detail";

export default function PositionDetailPage({
  params,
}: {
  params: { id: string };
}) {
  return <PositionDetail positionId={params.id} />;
}
