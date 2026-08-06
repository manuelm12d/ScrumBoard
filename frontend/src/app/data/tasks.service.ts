import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateTaskRequest,
  MoveTaskRequest,
  TaskDto,
  UpdateTaskRequest
} from '../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class TasksService {
  constructor(private readonly http: HttpClient) {}

  get(id: string): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${environment.apiBaseUrl}/api/tasks/${id}`);
  }

  create(request: CreateTaskRequest): Observable<TaskDto> {
    return this.http.post<TaskDto>(`${environment.apiBaseUrl}/api/tasks`, request);
  }

  update(id: string, request: UpdateTaskRequest): Observable<TaskDto> {
    return this.http.put<TaskDto>(`${environment.apiBaseUrl}/api/tasks/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/api/tasks/${id}`);
  }

  move(id: string, request: MoveTaskRequest): Observable<TaskDto> {
    return this.http.patch<TaskDto>(`${environment.apiBaseUrl}/api/tasks/${id}/position`, request);
  }
}
