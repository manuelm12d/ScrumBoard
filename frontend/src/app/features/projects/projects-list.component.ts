import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { DropdownModule } from 'primeng/dropdown';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TagModule } from 'primeng/tag';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProjectsService } from '../../data/projects.service';
import {
  PROJECT_STATUS_LABELS,
  ProjectDto,
  ProjectStatus,
  SaveProjectRequest
} from '../../shared/models/api.models';

@Component({
  selector: 'app-projects-list',
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    TableModule,
    DialogModule,
    InputTextModule,
    InputTextareaModule,
    DropdownModule,
    ConfirmDialogModule,
    ToastModule,
    TagModule
  ],
  providers: [ConfirmationService, MessageService],
  templateUrl: './projects-list.component.html',
  styleUrl: './projects-list.component.scss'
})
export class ProjectsListComponent implements OnInit {
  private readonly projectsApi = inject(ProjectsService);
  private readonly fb = inject(FormBuilder);
  private readonly confirm = inject(ConfirmationService);
  private readonly messages = inject(MessageService);

  items: ProjectDto[] = [];
  totalItems = 0;
  page = 1;
  pageSize = 10;
  nameFilter = '';
  loading = false;
  dialogVisible = false;
  editing: ProjectDto | null = null;

  readonly statusOptions = Object.entries(PROJECT_STATUS_LABELS).map(([value, label]) => ({
    label,
    value: Number(value)
  }));

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
    startDate: [new Date().toISOString().slice(0, 10), Validators.required],
    plannedEndDate: [''],
    status: [ProjectStatus.Planned as ProjectStatus, Validators.required]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.projectsApi.search(this.nameFilter.trim() || null, this.page, this.pageSize).subscribe({
      next: (result) => {
        this.items = result.items;
        this.totalItems = result.totalItems;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudieron cargar los proyectos.' });
      }
    });
  }

  onPage(event: { first?: number | null; rows?: number | null }): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize;
    this.pageSize = rows;
    this.page = Math.floor(first / rows) + 1;
    this.load();
  }

  openCreate(): void {
    this.editing = null;
    this.form.reset({
      name: '',
      description: '',
      startDate: new Date().toISOString().slice(0, 10),
      plannedEndDate: '',
      status: ProjectStatus.Planned
    });
    this.dialogVisible = true;
  }

  openEdit(project: ProjectDto): void {
    this.editing = project;
    this.form.reset({
      name: project.name,
      description: project.description ?? '',
      startDate: project.startDate,
      plannedEndDate: project.plannedEndDate ?? '',
      status: project.status
    });
    this.dialogVisible = true;
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const request: SaveProjectRequest = {
      name: raw.name,
      description: raw.description || null,
      startDate: raw.startDate,
      plannedEndDate: raw.plannedEndDate || null,
      status: raw.status
    };

    const request$ = this.editing
      ? this.projectsApi.update(this.editing.id, request)
      : this.projectsApi.create(request);

    request$.subscribe({
      next: () => {
        this.dialogVisible = false;
        this.messages.add({
          severity: 'success',
          summary: 'Guardado',
          detail: this.editing ? 'Proyecto actualizado.' : 'Proyecto creado.'
        });
        this.load();
      },
      error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo guardar el proyecto.' })
    });
  }

  confirmDelete(project: ProjectDto): void {
    this.confirm.confirm({
      message: `¿Eliminar el proyecto "${project.name}"?`,
      header: 'Confirmar',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'Eliminar',
      rejectLabel: 'Cancelar',
      accept: () => {
        this.projectsApi.delete(project.id).subscribe({
          next: () => {
            this.messages.add({ severity: 'success', summary: 'Eliminado', detail: 'Proyecto eliminado.' });
            this.load();
          },
          error: () => this.messages.add({ severity: 'error', summary: 'Error', detail: 'No se pudo eliminar.' })
        });
      }
    });
  }

  statusLabel(status: ProjectStatus): string {
    return PROJECT_STATUS_LABELS[status] ?? String(status);
  }
}
