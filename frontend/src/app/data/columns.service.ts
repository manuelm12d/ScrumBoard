import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  BoardColumnDto,
  BoardDto,
  BoardFilter,
  SaveColumnRequest
} from '../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class ColumnsService {
  constructor(private readonly http: HttpClient) {}

  getBoard(projectId: string, filter: BoardFilter = {}): Observable<BoardDto> {
    let params = new HttpParams();
    if (filter.assigneeId) {
      params = params.set('assigneeId', filter.assigneeId);
    }
    if (filter.priority != null) {
      params = params.set('priority', filter.priority);
    }
    if (filter.search) {
      params = params.set('search', filter.search);
    }

    return this.http.get<BoardDto>(`${environment.apiBaseUrl}/api/projects/${projectId}/board`, { params });
  }

  list(projectId: string): Observable<BoardColumnDto[]> {
    return this.http.get<BoardColumnDto[]>(`${environment.apiBaseUrl}/api/projects/${projectId}/columns`);
  }

  create(projectId: string, request: SaveColumnRequest): Observable<BoardColumnDto> {
    return this.http.post<BoardColumnDto>(`${environment.apiBaseUrl}/api/projects/${projectId}/columns`, request);
  }

  update(id: string, request: SaveColumnRequest): Observable<BoardColumnDto> {
    return this.http.put<BoardColumnDto>(`${environment.apiBaseUrl}/api/columns/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/api/columns/${id}`);
  }

  reorder(projectId: string, orderedColumnIds: string[]): Observable<BoardColumnDto[]> {
    return this.http.put<BoardColumnDto[]>(
      `${environment.apiBaseUrl}/api/projects/${projectId}/columns/order`,
      { orderedColumnIds }
    );
  }
}
