import { Position } from "@/entities/positions/types";
import { Card } from "@/shared/components/ui/card";

type Props = {
  position: Position;
};

export default function PositionCard({ position }: Props) {
  return (
    <Card className="bg-slate-900/40 border-slate-700 px-4 cursor-pointer hover:bg-slate-800/50 transition-colors">
      <div className="flex justify-between items-start mb-2">
        <div className="flex-1">
          <h3 className="text-lg font-medium text-white">{position.name}</h3>
          <div className="text-sm text-slate-400 mt-1">
            {position.description}
          </div>
        </div>
        <div>
          <span
            className={
              "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium " +
              (position.isActive
                ? "bg-emerald-100 text-emerald-800"
                : "bg-amber-100 text-amber-800")
            }
          >
            {position.isActive ? "Активна" : "Неактивна"}
          </span>
        </div>
      </div>

      <div className="text-sm text-slate-400 pb-3">
        Подразделений:{" "}
        <span className="font-semibold">
          {position.departments.length.toString()}
        </span>
      </div>
    </Card>
  );
}
