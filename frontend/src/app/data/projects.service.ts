import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult, ProjectDto, SaveProjectRequest } from '../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class ProjectsService {
  constructor(private readonly http: HttpClient) {}

  search(name: string | null, page: number, pageSize: number): Observable<PagedResult<ProjectDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (name) {
      params = params.set('name', name);
    }

    return this.http.get<PagedResult<ProjectDto>>(`${environment.apiBaseUrl}/api/projects`, { params });
  }

  get(id: string): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${environment.apiBaseUrl}/api/projects/${id}`);
  }

  create(request: SaveProjectRequest): Observable<ProjectDto> {
    return this.http.post<ProjectDto>(`${environment.apiBaseUrl}/api/projects`, request);
  }

  update(id: string, request: SaveProjectRequest): Observable<ProjectDto> {
    return this.http.put<ProjectDto>(`${environment.apiBaseUrl}/api/projects/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/api/projects/${id}`);
  }
}
