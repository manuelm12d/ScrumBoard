import { BoardColumnWithTasksDto, BoardDto, TaskDto } from '../models/api.models';

export function cloneBoard(board: BoardDto): BoardDto {
  return {
    projectId: board.projectId,
    projectName: board.projectName,
    columns: board.columns.map((column) => ({
      ...column,
      tasks: column.tasks.map((task) => ({ ...task }))
    }))
  };
}

export function findTask(board: BoardDto, taskId: string): { column: BoardColumnWithTasksDto; task: TaskDto; index: number } | null {
  for (const column of board.columns) {
    const index = column.tasks.findIndex((task) => task.id === taskId);
    if (index >= 0) {
      return { column, task: column.tasks[index], index };
    }
  }
  return null;
}

export function applyOptimisticMove(
  board: BoardDto,
  taskId: string,
  targetColumnId: string,
  toIndex: number
): BoardDto {
  const next = cloneBoard(board);
  const located = findTask(next, taskId);
  if (!located) {
    return next;
  }

  located.column.tasks.splice(located.index, 1);
  const target = next.columns.find((column) => column.id === targetColumnId);
  if (!target) {
    return next;
  }

  const moved: TaskDto = { ...located.task, columnId: targetColumnId };
  const clamped = Math.max(0, Math.min(toIndex, target.tasks.length));
  target.tasks.splice(clamped, 0, moved);
  return next;
}

export function upsertTask(board: BoardDto, task: TaskDto): BoardDto {
  const next = cloneBoard(board);
  for (const column of next.columns) {
    column.tasks = column.tasks.filter((item) => item.id !== task.id);
  }

  const target = next.columns.find((column) => column.id === task.columnId);
  if (!target) {
    return next;
  }

  const insertAt = target.tasks.findIndex((item) => item.position > task.position);
  if (insertAt < 0) {
    target.tasks.push(task);
  } else {
    target.tasks.splice(insertAt, 0, task);
  }

  return next;
}

export function removeTask(board: BoardDto, taskId: string): BoardDto {
  const next = cloneBoard(board);
  for (const column of next.columns) {
    column.tasks = column.tasks.filter((task) => task.id !== taskId);
  }
  return next;
}

export function applyTaskMovedEvent(
  board: BoardDto,
  payload: { taskId: string; toColumnId: string; position: number }
): BoardDto {
  const located = findTask(board, payload.taskId);
  if (!located) {
    return board;
  }

  const moved: TaskDto = {
    ...located.task,
    columnId: payload.toColumnId,
    position: payload.position
  };

  return upsertTask(board, moved);
}
