"use client";

import { Button } from "@/shared/components/ui/button";
import { Input } from "@/shared/components/ui/input";
import { Card } from "@/shared/components/ui/card";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { usePositionDetails } from "./model/use-position-details";
import { Spinner } from "@/shared/components/ui/spinner";
import { useState } from "react";

type Department = {
  departmentId: string;
  name: string;
};

type Props = {
  positionId: string;
};

export default function PositionDetail({ positionId }: Props) {
  const [linkedDepartments, setLinkedDepartments] = useState<Department[]>([
    { departmentId: "1", name: "IT отдел" },
    { departmentId: "2", name: "Разработка" },
  ]);

  const [availableDepartments] = useState<Department[]>([
    { departmentId: "1", name: "IT отдел" },
    { departmentId: "2", name: "Разработка" },
    { departmentId: "3", name: "HR отдел" },
    { departmentId: "4", name: "Маркетинг" },
  ]);

  const [selectedDepartmentToAdd, setSelectedDepartmentToAdd] = useState("");
  const [isEditing, setIsEditing] = useState(false);

  const dept = availableDepartments.find(
    (d) => d.departmentId === selectedDepartmentToAdd,
  );

  if (
    dept &&
    !linkedDepartments.find((d) => d.departmentId === dept.departmentId)
  ) {
    setLinkedDepartments([...linkedDepartments, dept]);
    setSelectedDepartmentToAdd("");
  }

  const handleRemoveDepartment = (departmentId: string) => {
    setLinkedDepartments(
      linkedDepartments.filter((d) => d.departmentId !== departmentId),
    );
  };

  const departmentsToAdd = availableDepartments.filter(
    (dept) =>
      !linkedDepartments.find((ld) => ld.departmentId === dept.departmentId),
  );

  const router = useRouter();

  const { position, isPending, error, isError } =
    usePositionDetails(positionId);

  if (isPending) {
    return (
      <div className="flex justify-center items-center h-64">
        <Spinner />
      </div>
    );
  }

  if (isError) {
    return (
      <div className="text-red-500">
        Ошибка: {error ? error.message : "Неизвестная ошибка"}
      </div>
    );
  }

  if (position === undefined) {
    return (
      <div className="flex justify-center items-center h-64">
        Позиция не найдена.
      </div>
    );
  }

  return (
    <div className="max-w-4xl mx-auto p-6">
      {/* Header */}
      <div className="mb-6">
        <button
          onClick={() => router.back()}
          className="text-blue-400 hover:text-blue-300 mb-4 inline-block bg-none border-none p-0 cursor-pointer"
        >
          ← Вернуться к списку
        </button>
        <h1 className="text-3xl font-bold text-white">{position?.name}</h1>
      </div>

      {/* Main Info Section */}
      <Card className="bg-slate-900/40 border-slate-700 mb-6 p-6">
        <div className="flex justify-between items-start mb-4">
          <h2 className="text-xl font-semibold text-white">
            Основная информация
          </h2>
          <Button variant="outline" onClick={() => setIsEditing(!isEditing)}>
            {isEditing ? "Отменить" : "Редактировать"}
          </Button>
        </div>

        {isEditing ? (
          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium text-slate-300 mb-2 block">
                Название
              </label>
              <Input
                value={position?.name}
                // onChange={(e) =>
                //   setPosition({ ...position, name: e.target.value })
                // }
                className="bg-slate-800/50"
              />
            </div>
            <div>
              <label className="text-sm font-medium text-slate-300 mb-2 block">
                Описание
              </label>
              <Input
                value={position?.description}
                // onChange={(e) =>
                //   setPosition({ ...position, description: e.target.value })
                // }
                className="bg-slate-800/50"
              />
            </div>
            <div className="flex gap-2">
              <Button className="bg-emerald-600 hover:bg-emerald-700">
                Сохранить
              </Button>
              <Button variant="outline" onClick={() => setIsEditing(false)}>
                Отменить
              </Button>
            </div>
          </div>
        ) : (
          <div className="space-y-3">
            <div>
              <p className="text-sm text-slate-400">Название</p>
              <p className="text-white font-medium">{position?.name}</p>
            </div>
            <div>
              <p className="text-sm text-slate-400">Описание</p>
              <p className="text-white font-medium">{position?.description}</p>
            </div>
            <div>
              <p className="text-sm text-slate-400">Статус</p>
              <span
                className={
                  "inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium " +
                  (position?.isActive
                    ? "bg-emerald-100 text-emerald-800"
                    : "bg-amber-100 text-amber-800")
                }
              >
                {position?.isActive ? "Активна" : "Неактивна"}
              </span>
            </div>
          </div>
        )}
      </Card>

      {/* Departments Section */}
      <Card className="bg-slate-900/40 border-slate-700 mb-6 p-6">
        <h2 className="text-xl font-semibold text-white mb-4">
          Привязанные подразделения
        </h2>

        {/* Add Department */}
        <div className="mb-6 p-4 bg-slate-800/30 rounded-lg border border-slate-700">
          <h3 className="text-sm font-medium text-white mb-3">
            Добавить подразделение
          </h3>
          <div className="flex gap-2">
            <select
              value={selectedDepartmentToAdd}
              onChange={(e) => setSelectedDepartmentToAdd(e.target.value)}
              className="flex-1 px-3 py-2 bg-slate-800/50 border border-slate-700 rounded-md text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value="">Выберите подразделение</option>
              {departmentsToAdd.map((dept) => (
                <option key={dept.departmentId} value={dept.departmentId}>
                  {dept.name}
                </option>
              ))}
            </select>
            <Button
              // onClick={handleAddDepartment}
              disabled={!selectedDepartmentToAdd}
            >
              Добавить
            </Button>
          </div>
        </div>

        {/* Linked Departments List */}
        <div>
          <h3 className="text-sm font-medium text-slate-300 mb-3">
            Подразделения ({linkedDepartments.length})
          </h3>
          {linkedDepartments.length > 0 ? (
            <div className="space-y-2">
              {linkedDepartments.map((dept) => (
                <div
                  key={dept.departmentId}
                  className="flex justify-between items-center p-3 bg-slate-800/30 rounded-lg border border-slate-700"
                >
                  <div>
                    <p className="text-white font-medium">{dept.name}</p>
                    <p className="text-xs text-slate-400">
                      ID: {dept.departmentId}
                    </p>
                  </div>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-red-400 hover:text-red-300 hover:bg-red-950/30"
                    onClick={() => handleRemoveDepartment(dept.departmentId)}
                  >
                    Удалить
                  </Button>
                </div>
              ))}
            </div>
          ) : (
            <div className="p-4 text-center text-slate-400">
              Подразделения не привязаны
            </div>
          )}
        </div>
      </Card>

      {/* Actions */}
      <div className="flex gap-2">
        <Link href="/positions" className="flex-1">
          <Button variant="outline" className="w-full">
            Закрыть
          </Button>
        </Link>
        <Button variant="ghost" className="text-red-400 hover:text-red-300">
          Удалить позицию
        </Button>
      </div>
    </div>
  );
}
