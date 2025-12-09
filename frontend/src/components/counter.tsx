"use client";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useCounter } from "@/hooks/use-counter";
import { JSX, useEffect } from "react";

export default function Counter(): JSX.Element {
  const { counter, click, isWin } = useCounter();

  useEffect(() => {
    console.log("Counter mounted");
  }, [counter]);

  return (
    <div className="flex flex-col gap-4">
      <Count count={counter} />

      <Button onClick={click} variant={"secondary"}>
        Увеличить
      </Button>

      <Input type="text" placeholder="Max leiter" />

      {isWin && <span className="text-green-500">You win!</span>}
    </div>
  );
}

type Props = {
  count: number;
};

function Count({ count }: Props): JSX.Element {
  return <span className="text-blue-500">{count}</span>;
}
