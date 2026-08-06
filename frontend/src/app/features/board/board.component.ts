import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { CdkDragDrop, DragDropModule, moveItemInArray, transferArrayItem } from '@angular/cdk/drag-drop';
import { Subscription } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ColumnsService } from '../../data/columns.service';
import { TasksService } from '../../data/tasks.service';
import { ReportsService } from '../../data/reports.service';
import { BoardRealtimeService } from '../../data/board-realtime.service';
import { AuthService } from '../../core/auth/auth.service';
import {
  BoardColumnWithTasksDto,
  BoardDto,
  BoardFilter,
  PRIORITY_LABELS,
  TaskDto,
  TaskPriority,
  UserDto
} from '../../shared/models/api.models';
import { resolveDropNeighbors } from '../../shared/helpers/drop-neighbors';
import {
  applyOptimisticMove,
  applyTaskMovedEvent,
  cloneBoard,
  removeTask,
  upsertTask
} from '../../shared/helpers/board-state';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    DragDropModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './board.component.html',
  styleUrl: './board.component.scss'
})
export class BoardComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly columnsApi = inject(ColumnsService);
  private readonly tasksApi = inject(TasksService);
  private readonly reportsApi = inject(ReportsService);
  private readonly realtime = inject(BoardRealtimeService);
  private readonly auth = inject(AuthService);
  private readonly fb = inject(FormBuilder);
  private readonly confirm = inject(ConfirmationService);
  private readonly messages = inject(MessageService);

  projectId = '';
  board: BoardDto | null = null;
  snapshot: BoardDto | null = null;
  users: UserDto[] = [];
  loading = false;
  columnDialog = false;
  taskDialog = false;
  editingColumn: BoardColumnWithTasksDto | null = null;
  editingTask: TaskDto | null = null;
  taskColumnId: string | null = null;
  private eventsSub?: Subscription;
  private ignoreEchoUntil = 0;

  filterAssignee: string | null = null;
  filterPriority: number | null = null;
  filterSearch = '';

  readonly priorityOptions = Object.entries(PRIORITY_LABELS).map(([value, label]) => ({
    label,
    value: Number(value)
  }));

  readonly columnForm = this.fb.nonNullable.group({
    name: ['', Validators.required]
  });

  readonly taskForm = this.fb.nonNullable.group({
    title: ['', Validators.required],
    description: [''],
    priority: [TaskPriority.Medium as TaskPriority, Validators.required],
    assigneeId: [null as string | null]
  });

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id') ?? '';
    this.auth.listUsers().subscribe({ next: (users) => (this.users = users) });
    this.loadBoard();
    void this.realtime.connect(this.projectId).catch(() => {
      this.messages.add({
        severity: 'warn',
        summary: 'Tiempo real',
        detail: 'No se pudo conectar al hub. El tablero sigue usable sin sincronización.'
      });
    });
    this.eventsSub = this.realtime.events$.subscribe((event) => this.onBoardEvent(event.type, event.payload));
  }

  ngOnDestroy(): void {
    this.eventsSub?.unsubscribe();
    void this.realtime.disconnect();
  }

  get connectedDropLists(): string[] {
    return (this.board?.columns ?? []).map((column) => column.id);
  }

  get currentFilter(): BoardFilter {
    return {
      assigneeId: this.filterAssignee,
      priority: this.filterPriority,
      search: this.filterSearch.trim() || null
    };
  }

  loadBoard(): void {
    this.loading = true;
    this.columnsApi.getBoard(this.projectId, this.currentFilter).subscribe({
      next: (board) => {
        this.board = board;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo cargar el tablero.' });
      }
    });
  }

  applyFilters(): void {
    this.loadBoard();
  }

  clearFilters(): void {
    this.filterAssignee = null;
    this.filterPriority = null;
    this.filterSearch = '';
    this.loadBoard();
  }

  openCreateColumn(): void {
    this.editingColumn = null;
    this.columnForm.reset({ name: '' });
    this.columnDialog = true;
  }

  openEditColumn(column: BoardColumnWithTasksDto): void {
    this.editingColumn = column;
    this.columnForm.reset({ name: column.name });
    this.columnDialog = true;
  }

  saveColumn(): void {
    if (this.columnForm.invalid || !this.board) {
      this.columnForm.markAllAsTouched();
      return;
    }

    const name = this.columnForm.getRawValue().name;
    const request$ = this.editingColumn
      ? this.columnsApi.update(this.editingColumn.id, { name })
      : this.columnsApi.create(this.projectId, { name });

    request$.subscribe({
      next: () => {
        this.columnDialog = false;
        this.messages.add({ severity: 'success', summary: 'Columnas', detail: 'Columna guardada.' });
        this.loadBoard();
      },
      error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo guardar la columna.' })
    });
  }

  confirmDeleteColumn(column: BoardColumnWithTasksDto): void {
    this.confirm.confirm({
      message: `¿Eliminar la columna "${column.name}"?`,
      header: 'Confirmar',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.columnsApi.delete(column.id).subscribe({
          next: () => {
            this.messages.add({ severity: 'success', summary: 'Columnas', detail: 'Columna eliminada.' });
            this.loadBoard();
          },
          error: (error: unknown) => {
            const detail =
              error instanceof HttpErrorResponse && error.status === 409
                ? 'No se puede eliminar una columna que contiene tareas.'
                : 'No se pudo eliminar la columna.';
            this.messages.add({ severity: 'warn', summary: 'Columnas', detail });
          }
        });
      }
    });
  }

  moveColumn(column: BoardColumnWithTasksDto, direction: -1 | 1): void {
    if (!this.board) {
      return;
    }

    const ordered = [...this.board.columns].sort((a, b) => a.order - b.order);
    const index = ordered.findIndex((item) => item.id === column.id);
    const target = index + direction;
    if (index < 0 || target < 0 || target >= ordered.length) {
      return;
    }

    moveItemInArray(ordered, index, target);
    this.columnsApi.reorder(this.projectId, ordered.map((item) => item.id)).subscribe({
      next: () => this.loadBoard(),
      error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo reordenar.' })
    });
  }

  openCreateTask(columnId: string): void {
    this.editingTask = null;
    this.taskColumnId = columnId;
    this.taskForm.reset({
      title: '',
      description: '',
      priority: TaskPriority.Medium,
      assigneeId: null
    });
    this.taskDialog = true;
  }

  openEditTask(task: TaskDto): void {
    this.editingTask = task;
    this.taskColumnId = task.columnId;
    this.taskForm.reset({
      title: task.title,
      description: task.description ?? '',
      priority: task.priority,
      assigneeId: task.assigneeId
    });
    this.taskDialog = true;
  }

  saveTask(): void {
    if (this.taskForm.invalid || !this.taskColumnId) {
      this.taskForm.markAllAsTouched();
      return;
    }

    const raw = this.taskForm.getRawValue();
    if (this.editingTask) {
      this.tasksApi
        .update(this.editingTask.id, {
          title: raw.title,
          description: raw.description || null,
          priority: raw.priority,
          assigneeId: raw.assigneeId
        })
        .subscribe({
          next: (task) => {
            this.taskDialog = false;
            if (this.board) {
              this.board = upsertTask(this.board, task);
            }
            this.messages.add({ severity: 'success', summary: 'Tarea', detail: 'Tarea actualizada.' });
          },
          error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo actualizar la tarea.' })
        });
      return;
    }

    this.tasksApi
      .create({
        columnId: this.taskColumnId,
        title: raw.title,
        description: raw.description || null,
        priority: raw.priority,
        assigneeId: raw.assigneeId
      })
      .subscribe({
        next: (task) => {
          this.taskDialog = false;
          if (this.board) {
            this.board = upsertTask(this.board, task);
          }
          this.messages.add({ severity: 'success', summary: 'Tarea', detail: 'Tarea creada.' });
        },
        error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo crear la tarea.' })
      });
  }

  confirmDeleteTask(task: TaskDto): void {
    this.confirm.confirm({
      message: `¿Eliminar la tarea "${task.title}"?`,
      header: 'Confirmar',
      accept: () => {
        this.tasksApi.delete(task.id).subscribe({
          next: () => {
            if (this.board) {
              this.board = removeTask(this.board, task.id);
            }
            this.messages.add({ severity: 'success', summary: 'Tarea', detail: 'Tarea eliminada.' });
          },
          error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar.' })
        });
      }
    });
  }

  onTaskDrop(event: CdkDragDrop<TaskDto[]>, targetColumn: BoardColumnWithTasksDto): void {
    if (!this.board) {
      return;
    }

    if (event.previousContainer === event.container && event.previousIndex === event.currentIndex) {
      return;
    }

    this.snapshot = cloneBoard(this.board);
    const dragged = event.previousContainer.data[event.previousIndex];

    if (event.previousContainer === event.container) {
      moveItemInArray(event.container.data, event.previousIndex, event.currentIndex);
    } else {
      transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    }

    const orderedIds = event.container.data.map((task) => task.id);
    const neighbors = resolveDropNeighbors(orderedIds, dragged.id, event.currentIndex);
    this.board = applyOptimisticMove(this.board, dragged.id, targetColumn.id, event.currentIndex);
    this.ignoreEchoUntil = Date.now() + 1500;

    this.tasksApi
      .move(dragged.id, {
        targetColumnId: targetColumn.id,
        previousTaskId: neighbors.previousTaskId,
        nextTaskId: neighbors.nextTaskId
      })
      .subscribe({
        next: (task) => {
          if (this.board) {
            this.board = upsertTask(this.board, task);
          }
          this.snapshot = null;
        },
        error: () => {
          if (this.snapshot) {
            this.board = this.snapshot;
            this.snapshot = null;
          }
          this.messages.add({
            severity: 'warn',
            summary: 'Movimiento revertido',
            detail: 'El servidor rechazó el nuevo orden. Se restauró la posición anterior.'
          });
        }
      });
  }

  downloadReport(format: 'pdf' | 'xlsx'): void {
    this.reportsApi.download(this.projectId, format, this.currentFilter).subscribe({
      next: (response) => {
        const blob = response.body;
        if (!blob) {
          return;
        }

        const disposition = response.headers.get('content-disposition') ?? '';
        const match = /filename\*?=(?:UTF-8''|")?([^\";]+)/i.exec(disposition);
        const fallback = `proyecto-reporte.${format === 'pdf' ? 'pdf' : 'xlsx'}`;
        const fileName = match ? decodeURIComponent(match[1].replace(/"/g, '')) : fallback;
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');
        anchor.href = url;
        anchor.download = fileName;
        anchor.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.messages.add({ severity: 'error', summary: 'Reporte', detail: 'No se pudo descargar el reporte.' })
    });
  }

  priorityLabel(priority: TaskPriority): string {
    return PRIORITY_LABELS[priority] ?? String(priority);
  }

  prioritySeverity(priority: TaskPriority): 'success' | 'info' | 'warning' | 'danger' {
    switch (priority) {
      case TaskPriority.Low:
        return 'success';
      case TaskPriority.Medium:
        return 'info';
      case TaskPriority.High:
        return 'warning';
      default:
        return 'danger';
    }
  }

  private onBoardEvent(type: string, payload: unknown): void {
    if (!this.board) {
      return;
    }

    if (Date.now() < this.ignoreEchoUntil && (type === 'task.moved' || type === 'task.created' || type === 'task.updated')) {
      return;
    }

    switch (type) {
      case 'task.created':
      case 'task.updated':
        this.board = upsertTask(this.board, payload as TaskDto);
        break;
      case 'task.deleted': {
        const deleted = payload as { taskId: string };
        this.board = removeTask(this.board, deleted.taskId);
        break;
      }
      case 'task.moved': {
        const moved = payload as {
          taskId: string;
          toColumnId: string;
          position: number;
        };
        this.board = applyTaskMovedEvent(this.board, moved);
        break;
      }
      case 'columns.changed':
        this.loadBoard();
        break;
      default:
        break;
    }
  }
}
