import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpResponse } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BoardFilter } from '../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class ReportsService {
  constructor(private readonly http: HttpClient) {}

  download(projectId: string, format: 'pdf' | 'xlsx', filter: BoardFilter = {}): Observable<HttpResponse<Blob>> {
    let params = new HttpParams().set('format', format);
    if (filter.assigneeId) {
      params = params.set('assigneeId', filter.assigneeId);
    }
    if (filter.priority != null) {
      params = params.set('priority', filter.priority);
    }
    if (filter.search) {
      params = params.set('search', filter.search);
    }

    return this.http.get(`${environment.apiBaseUrl}/api/projects/${projectId}/report`, {
      params,
      observe: 'response',
      responseType: 'blob'
    });
  }
}
