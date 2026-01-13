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
import { useCreateLocation } from "./model/use-create-location";
import { useForm } from "react-hook-form";

const createLocationSchema = z.object({
  name: z
    .string()
    .min(1, "Название локации обязательно")
    .min(3, "Название должно содержать минимум 3 символа")
    .max(150, "Название не должно превышать 200 символов"),
  locationAddress: z.object({
    country: z.string().min(1, "Название страны обязательно"),
    region: z.string().min(1, "Название региона обязательно"),
    city: z.string().min(1, "Название города обязательно"),
    street: z.string().min(1, "Название улицы обязательно"),
    house: z.string().min(1, "Номер дома обязателен"),
  }),
  timezone: z.string().min(1, "Timezone обязателен"),
});

type Props = {
  open: boolean;
  setOpen: (open: boolean) => void;
};

type CreateLocationData = z.infer<typeof createLocationSchema>;

export function CreateLocationDialog({ open, setOpen: setOpenChange }: Props) {
  const initialData: CreateLocationData = {
    name: "",
    locationAddress: {
      country: "",
      region: "",
      city: "",
      street: "",
      house: "",
    },
    timezone: "",
  };

  const {
    register,
    handleSubmit,
    formState: { errors, isValid },
    reset,
  } = useForm<CreateLocationData>({
    defaultValues: initialData,
    resolver: zodResolver(createLocationSchema),
  });

  const { createLocation, isPending, error, isError } = useCreateLocation();

  const onSubmit = (data: CreateLocationData) => {
    createLocation(data, {
      onSuccess: () => {
        reset(initialData);
        setOpenChange(false);
      },
    });
  };

  const getErrorMessage = (): string => {
    if (isError) {
      return error ? error.message : "Неизвестная ошибка";
    }

    return "";
  };

  return (
    <Dialog open={open} onOpenChange={setOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Создание локации</DialogTitle>
          <DialogDescription>
            Заполните форму для создания новой локации.
          </DialogDescription>
        </DialogHeader>

        <form className="grid gap-3 py-0" onSubmit={handleSubmit(onSubmit)}>
          <div className="grid gap-2">
            <Label htmlFor="name">Название</Label>
            <Input
              id="name"
              placeholder="Название локации"
              {...register("name")}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name.message}</p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="grid gap-2">
              <Label htmlFor="country">Страна</Label>
              <Input
                id="country"
                placeholder="Название страны"
                {...register("locationAddress.country")}
              />
              {errors.locationAddress?.country && (
                <p className="text-sm text-destructive">
                  {errors.locationAddress.country.message}
                </p>
              )}
            </div>
            <div className="grid gap-2">
              <Label htmlFor="region">Регион</Label>
              <Input
                id="region"
                placeholder="Название региона"
                {...register("locationAddress.region")}
              />
              {errors.locationAddress?.region && (
                <p className="text-sm text-destructive">
                  {errors.locationAddress.region.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="city">Город</Label>
              <Input
                id="city"
                placeholder="Название города"
                {...register("locationAddress.city")}
              />
              {errors.locationAddress?.city && (
                <p className="text-sm text-destructive">
                  {errors.locationAddress.city.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="street">Улица</Label>
              <Input
                id="street"
                placeholder="Название улицы"
                {...register("locationAddress.street")}
              />
              {errors.locationAddress?.street && (
                <p className="text-sm text-destructive">
                  {errors.locationAddress.street.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="house">Дом</Label>
              <Input
                id="house"
                placeholder="Номер дома"
                {...register("locationAddress.house")}
              />
              {errors.locationAddress?.house && (
                <p className="text-sm text-destructive">
                  {errors.locationAddress.house.message}
                </p>
              )}
            </div>

            <div className="grid gap-2">
              <Label htmlFor="timezone">Часовой пояс</Label>
              <Input
                id="timezone"
                placeholder="Europe/Moscow"
                {...register("timezone")}
              />
              {errors.timezone && (
                <p className="text-sm text-destructive">
                  {errors.timezone.message}
                </p>
              )}
            </div>
          </div>

          <DialogFooter className="pt-1">
            {error && (
              <div className="text-sm text-red-500 mt-2 mr-auto">
                {getErrorMessage()}
              </div>
            )}
            <Button disabled={isPending || !isValid} type="submit">
              {isPending ? "Создание..." : "Создать локацию"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
