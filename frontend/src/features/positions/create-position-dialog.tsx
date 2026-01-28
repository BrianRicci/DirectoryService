import { Button } from "@/shared/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog";
import { Input } from "@/shared/components/ui/input";
import { Label } from "@/shared/components/ui/label";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { useState } from "react";
import { X } from "lucide-react";

const createPositionSchema = z.object({
  name: z
    .string()
    .min(1, "Название позиции обязательно")
    .min(2, "Название должно содержать минимум 2 символа")
    .max(150, "Название не должно превышать 150 символов"),
  description: z
    .string()
    .max(500, "Описание не должно превышать 500 символов")
    .optional(),
  departmentIds: z
    .array(z.string())
    .min(1, "Необходимо выбрать минимум одно подразделение"),
});

type Props = {
  open: boolean;
  setOpen: (open: boolean) => void;
};

type CreatePositionData = z.infer<typeof createPositionSchema>;

// Mock departments
const mockDepartments = [
  { departmentId: "1", name: "IT отдел" },
  { departmentId: "2", name: "Разработка" },
  { departmentId: "3", name: "HR отдел" },
  { departmentId: "4", name: "Маркетинг" },
  { departmentId: "5", name: "Финансы" },
];

export function CreatePositionDialog({ open, setOpen: setOpenChange }: Props) {
  const initialData: CreatePositionData = {
    name: "",
    description: "",
    departmentIds: [],
  };

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    reset,
    control,
    watch,
  } = useForm<CreatePositionData>({
    defaultValues: initialData,
    resolver: zodResolver(createPositionSchema),
    mode: "onChange",
  });

  const [departmentInput, setDepartmentInput] = useState("");
  const selectedDepartments = watch("departmentIds");

  const onSubmit = (data: CreatePositionData) => {
    // TODO: Add create logic
    console.log("Creating position:", data);
    reset(initialData);
    setOpenChange(false);
  };

  const handleAddDepartment = (departmentId: string) => {
    if (departmentId && !selectedDepartments.includes(departmentId)) {
      const updatedDepts = [...selectedDepartments, departmentId];
      control._formValues.departmentIds = updatedDepts;
      // Trigger form update
      const event = new Event("input", { bubbles: true });
      document.dispatchEvent(event);
      setDepartmentInput("");
    }
  };

  const handleRemoveDepartment = (departmentId: string) => {
    const updatedDepts = selectedDepartments.filter(
      (id) => id !== departmentId,
    );
    control._formValues.departmentIds = updatedDepts;
    const event = new Event("input", { bubbles: true });
    document.dispatchEvent(event);
  };

  const selectedDeptNames = selectedDepartments
    .map((id) => mockDepartments.find((d) => d.departmentId === id)?.name)
    .filter(Boolean);

  const availableDepartments = mockDepartments.filter(
    (dept) => !selectedDepartments.includes(dept.departmentId),
  );

  return (
    <Dialog open={open} onOpenChange={setOpenChange}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>Создание позиции</DialogTitle>
          <DialogDescription>
            Заполните форму для создания новой должности.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-4 py-0" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-2">
            <Label htmlFor="name">Название</Label>
            <Input
              id="name"
              placeholder="Название позиции"
              {...register("name")}
            />
            {errors.name && (
              <p className="text-sm text-red-500">{errors.name.message}</p>
            )}
          </div>

          <div className="grid gap-2">
            <Label htmlFor="description">Описание</Label>
            <Input
              id="description"
              placeholder="Описание позиции (опционально)"
              {...register("description")}
            />
            {errors.description && (
              <p className="text-sm text-red-500">
                {errors.description.message}
              </p>
            )}
          </div>

          <div className="grid gap-2">
            <Label>Подразделения</Label>
            <div className="flex gap-2">
              <select
                value={departmentInput}
                onChange={(e) => setDepartmentInput(e.target.value)}
                className="flex-1 px-3 py-2 bg-slate-800/50 border border-slate-700 rounded-md text-white text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                <option value="">Выберите подразделение</option>
                {availableDepartments.map((dept) => (
                  <option key={dept.departmentId} value={dept.departmentId}>
                    {dept.name}
                  </option>
                ))}
              </select>
              <Button
                type="button"
                onClick={() => handleAddDepartment(departmentInput)}
                disabled={!departmentInput}
              >
                Добавить
              </Button>
            </div>
            {errors.departmentIds && (
              <p className="text-sm text-red-500">
                {errors.departmentIds.message}
              </p>
            )}
          </div>

          {selectedDepartments.length > 0 && (
            <div className="grid gap-2">
              <p className="text-sm text-slate-400">
                Выбранные подразделения ({selectedDepartments.length})
              </p>
              <div className="flex flex-wrap gap-2">
                {selectedDepartments.map((departmentId) => {
                  const dept = mockDepartments.find(
                    (d) => d.departmentId === departmentId,
                  );
                  return (
                    <div
                      key={departmentId}
                      className="flex items-center gap-2 px-3 py-1 bg-blue-950 border border-blue-700 rounded-full text-sm text-white"
                    >
                      <span>{dept?.name}</span>
                      <button
                        type="button"
                        onClick={() => handleRemoveDepartment(departmentId)}
                        className="ml-1 hover:text-red-400"
                      >
                        <X size={14} />
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
          )}

          <DialogFooter className="pt-2">
            <Button
              variant="outline"
              type="button"
              onClick={() => {
                reset(initialData);
                setOpenChange(false);
              }}
            >
              Отменить
            </Button>
            <Button disabled={!isValid} type="submit">
              Создать позицию
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
