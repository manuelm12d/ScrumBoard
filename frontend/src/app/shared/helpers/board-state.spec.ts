import { BoardDto, TaskPriority } from '../models/api.models';
import { applyOptimisticMove, applyTaskMovedEvent, removeTask } from './board-state';

function sampleBoard(): BoardDto {
  return {
    projectId: 'p1',
    projectName: 'Demo',
    columns: [
      {
        id: 'c1',
        name: 'Todo',
        order: 0,
        tasks: [
          {
            id: 't1',
            columnId: 'c1',
            projectId: 'p1',
            title: 'One',
            description: null,
            priority: TaskPriority.Medium,
            assigneeId: null,
            assigneeName: null,
            position: 1024,
            createdAtUtc: '2026-01-01T00:00:00Z'
          },
          {
            id: 't2',
            columnId: 'c1',
            projectId: 'p1',
            title: 'Two',
            description: null,
            priority: TaskPriority.High,
            assigneeId: null,
            assigneeName: null,
            position: 2048,
            createdAtUtc: '2026-01-01T00:00:00Z'
          }
        ]
      },
      {
        id: 'c2',
        name: 'Doing',
        order: 1,
        tasks: []
      }
    ]
  };
}

describe('board-state helpers', () => {
  it('applies an optimistic move across columns and can be treated as reversible via snapshot', () => {
    const original = sampleBoard();
    const snapshot = structuredClone(original);
    const moved = applyOptimisticMove(original, 't1', 'c2', 0);

    expect(moved.columns[0].tasks.map((task) => task.id)).toEqual(['t2']);
    expect(moved.columns[1].tasks.map((task) => task.id)).toEqual(['t1']);
    expect(snapshot.columns[0].tasks.map((task) => task.id)).toEqual(['t1', 't2']);
  });

  it('removes a task from the board', () => {
    const next = removeTask(sampleBoard(), 't2');
    expect(next.columns[0].tasks.map((task) => task.id)).toEqual(['t1']);
  });

  it('applies a task.moved realtime payload by position', () => {
    const next = applyTaskMovedEvent(sampleBoard(), {
      taskId: 't1',
      toColumnId: 'c2',
      position: 500
    });

    expect(next.columns[1].tasks[0].id).toBe('t1');
    expect(next.columns[1].tasks[0].position).toBe(500);
    expect(next.columns[0].tasks.map((task) => task.id)).toEqual(['t2']);
  });
});
