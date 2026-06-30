import { ApplicationRef, ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { AuditLogDto } from '../../../core/models/signer.model';
import { FormBuilder, FormsModule, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuditLogService } from '../../../core/services/signer.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { start } from 'repl';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { Router } from '@angular/router';
import { DatePipe, SlicePipe } from '@angular/common';
import { MatCard, MatCardModule } from "@angular/material/card";
import { MatFormField, MatLabel, MatPrefix } from "@angular/material/form-field";
import { MatInput } from "@angular/material/input";
import { MatDatepicker, MatDatepickerInput, MatDatepickerModule } from "@angular/material/datepicker";
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';



interface ActionMeta {
  label: string;
  icon: string;
  color: string;
}

const ACTION_META: Record<string, ActionMeta> = {
  'document_created':   { label: 'Documento criado',   icon: 'add_circle',     color: '#10b981' },
  'document_uploaded':  { label: 'Upload realizado',    icon: 'upload_file',    color: '#3b82f6' },
  'document_signed':    { label: 'Assinado',            icon: 'draw',           color: '#10b981' },
  'document_rejected':  { label: 'Rejeitado',           icon: 'cancel',         color: '#ef4444' },
  'document_expired':   { label: 'Expirado',            icon: 'timer_off',      color: '#8b5cf6' },
  'document_cancelled': { label: 'Cancelado',           icon: 'block',          color: '#6b7280' },
  'document_downloaded':{ label: 'Download realizado',  icon: 'download',       color: '#f59e0b' },
  'document_viewed':    { label: 'Visualizado',         icon: 'visibility',     color: '#64748b' },
  'flow_created':       { label: 'Fluxo criado',        icon: 'account_tree',   color: '#3b82f6' },
  'flow_completed':     { label: 'Fluxo concluído',     icon: 'check_circle',   color: '#10b981' },
}

@Component({
  selector: 'app-history',
  imports: [
    MatCard,
    MatFormField,
    MatLabel,
    MatPrefix,
    MatInput, MatDatepickerInput,
    MatCardModule,
    FormsModule,
    ReactiveFormsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    DatePipe,
    SlicePipe,
    MatPaginatorModule
  ],
  templateUrl: './history.component.html',
  styleUrls: ['./history.component.scss'],
})
export class HistoryComponent implements OnInit {
  dataSource = new MatTableDataSource<AuditLogDto>([]);
  displayedColumns = ['timestamp', 'action', 'document', 'details', 'ip', 'navigate']
  loading = false;
  totalCount = 0;
  pageSize = 20;
  pageIndex = 0;

  readonly maxDate = new Date();
  filterForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private auditLogService: AuditLogService,
    private snackbar: MatSnackBar,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private appRef: ApplicationRef,
  ) {
    this.filterForm = this.fb.group({
      startDate: [this.defaultStart()],
      endDate: [new Date()]
    })
  }

  ngOnInit(): void {
    this.load();
  }

  load() {
    const { startDate, endDate } = this.filterForm.value;
    if (!startDate || !endDate) return;

    this.loading = true;
    this.auditLogService.GetByDateRange({
      startDate: (startDate as Date).toISOString(),
      endDate: (endDate as Date).toISOString(),
    }).subscribe({
      next: (logs) => {
        this.dataSource.data = logs;
        this.totalCount = logs.length;
        this.loading = false;
        this.cdr.detectChanges();
        this.appRef.tick();
      },
      error: (error) => {
        this.snackbar.open('Erro ao carregar histórico', 'Fechar', { duration: 3000 });
        this.loading = false;
        this.cdr.detectChanges();
        this.appRef.tick();
      }
    });
  }

  onPageChange(e: PageEvent): void {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
  }

  get pagedData(): AuditLogDto[] {
    const start = this.pageIndex * this.pageSize;
    return this.dataSource.data.slice(start, start + this.pageSize);
  }

  getActionMeta(action?: string): ActionMeta {
    if (!action) return {
      label: action ?? 'Ação',
      icon: 'info',
      color: '#94a3b8'
    }
    return ACTION_META[action.toLowerCase().replace(/ /g, '_')] ?? {
      label: action,
      icon: 'history',
      color: '#94a3b8'
    }
  }

  navigateToDocument(log: AuditLogDto): void {
    if (log.documentId) this.router.navigate(['/documents', log.documentId]);
  }

  private defaultStart(): Date {
    const d = new Date();
    d.setDate(d.getDate() - 30)
    return d
  }
}

