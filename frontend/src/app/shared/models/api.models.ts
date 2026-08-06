export enum ProjectStatus {
  Planned = 0,
  Active = 1,
  OnHold = 2,
  Completed = 3,
  Cancelled = 4
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3
}

export interface UserDto {
  id: string;
  name: string;
  email: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  user: UserDto;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface ProjectDto {
  id: string;
  name: string;
  description: string | null;
  startDate: string;
  plannedEndDate: string | null;
  status: ProjectStatus;
  createdAtUtc: string;
}

export interface SaveProjectRequest {
  name: string;
  description: string | null;
  startDate: string;
  plannedEndDate: string | null;
  status: ProjectStatus;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface BoardColumnDto {
  id: string;
  projectId: string;
  name: string;
  order: number;
}

export interface SaveColumnRequest {
  name: string;
}

export interface TaskDto {
  id: string;
  columnId: string;
  projectId: string;
  title: string;
  description: string | null;
  priority: TaskPriority;
  assigneeId: string | null;
  assigneeName: string | null;
  position: number;
  createdAtUtc: string;
}

export interface CreateTaskRequest {
  columnId: string;
  title: string;
  description: string | null;
  priority: TaskPriority;
  assigneeId: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description: string | null;
  priority: TaskPriority;
  assigneeId: string | null;
}

export interface MoveTaskRequest {
  targetColumnId: string;
  previousTaskId: string | null;
  nextTaskId: string | null;
}

export interface BoardColumnWithTasksDto {
  id: string;
  name: string;
  order: number;
  tasks: TaskDto[];
}

export interface BoardDto {
  projectId: string;
  projectName: string;
  columns: BoardColumnWithTasksDto[];
}

export interface BoardFilter {
  assigneeId?: string | null;
  priority?: number | null;
  search?: string | null;
}

export interface BoardEvent {
  type: string;
  payload: unknown;
}

export const PROJECT_STATUS_LABELS: Record<ProjectStatus, string> = {
  [ProjectStatus.Planned]: 'Planificado',
  [ProjectStatus.Active]: 'Activo',
  [ProjectStatus.OnHold]: 'En pausa',
  [ProjectStatus.Completed]: 'Completado',
  [ProjectStatus.Cancelled]: 'Cancelado'
};

export const PRIORITY_LABELS: Record<TaskPriority, string> = {
  [TaskPriority.Low]: 'Baja',
  [TaskPriority.Medium]: 'Media',
  [TaskPriority.High]: 'Alta',
  [TaskPriority.Critical]: 'Crítica'
};
