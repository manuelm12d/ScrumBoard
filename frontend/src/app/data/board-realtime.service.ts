import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from '../core/auth/auth.service';
import { BoardEvent } from '../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class BoardRealtimeService {
  private connection: signalR.HubConnection | null = null;
  private joinedProjectId: string | null = null;
  private readonly eventsSubject = new Subject<BoardEvent>();

  readonly events$ = this.eventsSubject.asObservable();

  constructor(private readonly auth: AuthService) {}

  async connect(projectId: string): Promise<void> {
    const token = this.auth.getToken();
    if (!token) {
      throw new Error('No hay token de sesión para SignalR');
    }

    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${environment.hubUrl}?access_token=${encodeURIComponent(token)}`)
        .withAutomaticReconnect()
        .build();

      this.connection.on('boardEvent', (event: BoardEvent) => this.eventsSubject.next(event));
      await this.connection.start();
    }

    if (this.joinedProjectId && this.joinedProjectId !== projectId) {
      await this.connection.invoke('LeaveBoard', this.joinedProjectId);
    }

    await this.connection.invoke('JoinBoard', projectId);
    this.joinedProjectId = projectId;
  }

  async disconnect(): Promise<void> {
    if (!this.connection) {
      return;
    }

    try {
      if (this.joinedProjectId) {
        await this.connection.invoke('LeaveBoard', this.joinedProjectId);
      }
    } catch {
      // LeaveBoard can fail if the connection already dropped; still stop cleanly.
    }

    this.joinedProjectId = null;
    await this.connection.stop();
    this.connection = null;
  }
}
