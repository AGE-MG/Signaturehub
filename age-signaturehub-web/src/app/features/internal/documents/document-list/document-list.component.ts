import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSort, MatSortModule,  } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { DocumentDto, DocumentSource, DocumentStatusLabel, DocumentStatusMatColor, formatFileSize } from '../../../../core/models/document.model';
import { DocumentStatus } from '../../../../core/models/dasboard.model';
import { Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatAnchor } from "@angular/material/button";
import { MatIconModule } from "@angular/material/icon";
import { MatFormField, MatLabel } from "@angular/material/form-field";
import { MatInput } from "@angular/material/input";
import { MatSelect, MatOption } from "@angular/material/select";
import { FormsModule } from "@angular/forms";
import { MatChipSet, MatChip } from "@angular/material/chips";
import { MatCard } from "@angular/material/card";
import { MatProgressSpinner } from "@angular/material/progress-spinner";

@Component({
  selector: 'app-document-list',
  imports: [MatPaginatorModule, MatSortModule, MatAnchor, MatIconModule, MatFormField, MatLabel, MatInput, MatSelect, FormsModule, MatOption, MatChipSet, MatChip, MatCard, MatProgressSpinner],
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.scss'],
})
export class DocumentListComponent implements OnInit {
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  dataSource = new MatTableDataSource<DocumentDto>([]);
  displayedColumns = ['icon', 'title', 'source', 'status', 'size', 'updatedAt', 'actions'];

  loading = false;
  totalCount = 0;
  pageSize = 10;
  pageIndex = 0;

  searchQuery = '';
  selectedStatus: DocumentStatus | null = null;
  selectedSource: DocumentSource | null = null;

  readonly DocumentStatus = DocumentStatus;
  readonly DocumentSource = DocumentSource;
  readonly DocumentStatusLabel = DocumentStatusLabel;
  readonly DocumentStatusMatColor = DocumentStatusMatColor;
  readonly formatFileSize = formatFileSize;

  readonly statusOptions = Object.entries(DocumentStatusLabel).map(([value, label]) => ({ value: Number(value) as DocumentStatus, label }));
  readonly sourceOptions = Object.values(DocumentSource).map((v) => ({ value: v, label: v }));

  constructor(
    private router: Router,
    private documentService: DocumentService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
  ) { }

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.loading = true;
    this.documentService.getDocuments({
      status: this.selectedStatus ?? undefined,
      source: this.selectedSource ?? undefined,
      search: this.searchQuery || undefined,
      pageNumber: this.pageIndex + 1,
      pageSize: this.pageSize,
    }).subscribe({
      next: (result) => {
        if (Array.isArray(result)) {
          this.dataSource.data = result;
          this.totalCount = result.length;
        } else {
          this.dataSource.data = result.items ?? [];
          this.totalCount = result.totalCount ?? 0;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading documents', error);
        this.snackBar.open('Erro ao carregar documentos', 'Fechar', { duration: 3000 });
        this.loading = false;
      }
    })
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.loadDocuments();
  }

  onSearch(): void {
    this.pageIndex = 0;
    this.loadDocuments();
  }

  onStatusFilter(status: DocumentStatus | null): void {
    this.selectedStatus = status;
    this.pageIndex = 0;
    this.loadDocuments();
  }

  onSourceFilter(): void {
    this.pageIndex = 0;
    this.loadDocuments();
  }

  clearFilters(): void {
    this.searchQuery = '';
    this.selectedStatus = null;
    this.selectedSource = null;
    this.pageIndex = 0;
    this.loadDocuments();
  }

  get hasActiveFilters(): boolean {
    return !!(this.searchQuery || this.selectedStatus !== null || this.selectedSource !== null);
  }

  // Navigation

  goToUpload(): void {
    this.router.navigate(['/documents/upload']);
  }

  viewDocument(document: DocumentDto): void {
    this.router.navigate(['/documents', document.id]);
  }

  // Actions

  downloadDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    this.documentService.downloadDocument(doc.id, doc.originalFilename).subscribe({
      next: (blob) => {
        this.documentService.triggerDownload(blob, doc.originalFilename);
        this.snackBar.open('Download iniciado', 'Fechar', { duration: 3000 });
      },
      error: (error) => {
        console.error('Error downloading document', error);
        this.snackBar.open('Erro ao baixar documento', 'Fechar', { duration: 3000 });
      }
    });
  }

  deleteDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    if (!confirm(`Tem certeza que deseja excluir o documento "${doc.title}"?`)) {
      return;
    }
    this.documentService.deleteDocument(doc.id).subscribe({
      next: () => {
        this.snackBar.open('Documento excluído com sucesso', 'Fechar', { duration: 3000 });
        this.loadDocuments();
      },
      error: (error) => {
        console.error('Error deleting document', error);
        this.snackBar.open('Erro ao excluir documento', 'Fechar', { duration: 3000 });
      }
    });
  }

  signDocument(doc: DocumentDto, event: MouseEvent): void {
    event.stopPropagation();
    this.router.navigate(['/documents', doc.id], { queryParams: { action: 'sign' } });
  }

  // Helpers

  getFileIcon(ext: string): string {
    const map: Record<string, string> = {
      '.pdf': 'picture_as_pdf',
      '.doc': 'description',
      '.docx': 'description',
      '.xls': 'grid_on',
      '.xlsx': 'grid_on',
      '.ppt': 'slideshow',
      '.pptx': 'slideshow',
      '.txt': 'notes',
    };
    return map[ext?.toLowerCase()] || 'insert_drive_file';
  }

  getFileIconColor(ext: string): string {
    const map: Record<string, string> = {
      '.pdf': '#E53935',
      '.doc': '#1E88E5',
      '.docx': '#1E88E5',
      '.xls': '#43A047',
      '.xlsx': '#43A047',
      '.ppt': '#FB8C00',
      '.pptx': '#FB8C00',
      '.txt': '#757575',
    };
    return map[ext?.toLowerCase()] || '#757575';
  }

  canSign(doc: DocumentDto): boolean {
    return (
      doc.status === DocumentStatus.PendingSignatures || doc.status === DocumentStatus.PartiallyCompleted
    )
  }

  canDelete(doc: DocumentDto): boolean {
    return (
      doc.status === DocumentStatus.Draft ||
      doc.status === DocumentStatus.Cancelled
    )
  }

  trackById(_: number, doc: DocumentDto): string {
    return doc.id;
  }
}
