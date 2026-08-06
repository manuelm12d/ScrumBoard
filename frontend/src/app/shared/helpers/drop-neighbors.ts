export interface DropNeighbors {
  previousTaskId: string | null;
  nextTaskId: string | null;
}

/**
 * Derives the neighbour ids for PATCH /tasks/{id}/position after a drop.
 * `orderedTaskIds` is the column list AFTER the dragged task has been placed at `toIndex`.
 */
export function resolveDropNeighbors(
  orderedTaskIds: string[],
  draggedId: string,
  toIndex: number
): DropNeighbors {
  if (toIndex < 0 || toIndex >= orderedTaskIds.length) {
    throw new Error('toIndex fuera de rango');
  }

  if (orderedTaskIds[toIndex] !== draggedId) {
    throw new Error('La tarea arrastrada no está en toIndex');
  }

  return {
    previousTaskId: toIndex > 0 ? orderedTaskIds[toIndex - 1] : null,
    nextTaskId: toIndex < orderedTaskIds.length - 1 ? orderedTaskIds[toIndex + 1] : null
  };
}

/**
 * Moves `draggedId` from its current position into `toIndex` within a copy of the list.
 */
export function reorderTaskIds(taskIds: string[], draggedId: string, toIndex: number): string[] {
  const fromIndex = taskIds.indexOf(draggedId);
  if (fromIndex < 0) {
    throw new Error('La tarea arrastrada no está en la lista');
  }

  const next = [...taskIds];
  next.splice(fromIndex, 1);
  const clamped = Math.max(0, Math.min(toIndex, next.length));
  next.splice(clamped, 0, draggedId);
  return next;
}
