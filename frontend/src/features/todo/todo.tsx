"use client";

import { Input } from "../../shared/components/ui/input";
import { Button } from "../../shared/components/ui/button";
import { useState } from "react";
import { Checkbox } from "../../shared/components/ui/checkbox";

type Todo = {
  id: number;
  title: string;
  completed: boolean;
};

export default function Todo() {
  const [todos, setTodos] = useState<Todo[]>([
    { id: 1, title: "Learn TypeScript", completed: false },
    { id: 2, title: "Build a React App", completed: true },
    { id: 3, title: "Write Tests", completed: false },
  ]);

  const [input, setInput] = useState("");

  const addTodo = () => {
    const newTodo: Todo = {
      id: todos.length + 1,
      title: input,
      completed: false,
    };

    setTodos((prevTodos) => [...prevTodos, newTodo]);
  };

  const toggleTodo = (id: number) => {
    setTodos((prevTodos) =>
      prevTodos.map((todo) => {
        if (todo.id === id) {
          return { ...todo, completed: !todo.completed };
        }
        return todo;
      })
    );
  };

  return (
    <div className="max-w-2xl mx-auto p-6 space-y-6">
      {/* Форма добавления */}
      <div className="flex gap-2">
        <Input
          placeholder="Введите название"
          className="flex-1"
          value={input}
          onChange={(event) => setInput(event.target.value)}
        />
        <Button onClick={addTodo}>Добавить задачу</Button>
      </div>

      {/* Список задач */}
      <div className="space-y-3">
        {[...todos].reverse().map((todo) => (
          <div
            key={todo.id}
            className="flex items-center gap-4 p-4 bg-card border rounded-lg shadow-sm hover:shadow-md transition-shadow"
          >
            <span className="text-sm text-muted-foreground font-mono">
              #{todo.id}
            </span>
            <span
              className={
                todo.completed
                  ? "flex-1 text-base line-through text-muted-foreground"
                  : "flex-1 text-base"
              }
            >
              {todo.title}
            </span>
            <Checkbox
              checked={todo.completed}
              onCheckedChange={() => toggleTodo(todo.id)}
            />
            <span className="text-lg">{todo.completed ? "✅" : "❌"}</span>
          </div>
        ))}
      </div>
    </div>
  );
}
