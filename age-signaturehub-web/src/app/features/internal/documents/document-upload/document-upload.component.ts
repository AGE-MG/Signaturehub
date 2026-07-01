import { CommonModule } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { SecurityContext } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatStepper, MatStepperModule } from '@angular/material/stepper';
import { MatChipsModule } from '@angular/material/chips';
import { formatFileSize } from '../../../../core/models/document.model';
import { Router } from '@angular/router';
import { DocumentService } from '../../../../core/services/document.service';
import { AuthService } from '../../../../core/services/auth.service';

interface FileValidation {
  valid: boolean;
  error?: string;
}

@Component({
  selector: 'app-document-upload',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatStepperModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule,
],
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.scss'],
})
export class DocumentUploadComponent implements OnInit {
  @ViewChild('stepper') stepper!: MatStepper;

  // File Step

  selectedFile: File | null = null;
  dragOver = false;
  fileError: string | null = null;
  previewUrl: SafeResourceUrl | null = null;
  previewText: string | null = null;
  previewType: 'pdf' | 'text' | 'unsupported' | null = null;
  previewLoading = false;

  // Metadata Step
  metadataForm!: FormGroup;
  // Submit
  uploading = false;
  uploadProgress = 0;

  readonly formatFileSize = formatFileSize;
  readonly minDate = new Date()
  private readonly destroyRef = inject(DestroyRef);

  private readonly allowedTypes = ['application/pdf', 'application/vnd.openxmlformats-officedocument.wordprocessingml.document', 'application/msword', 'text/plain'];
  private readonly maxFileSize = 50 * 1024 * 1024; // 50 MB

  constructor(
    private fb: FormBuilder,
    private sanitizer: DomSanitizer,
    private router: Router,
    private documentService: DocumentService,
    private authService: AuthService,
    private snackBar: MatSnackBar
  ) {}
  ngOnInit(): void {
    this.destroyRef.onDestroy(() => this.clearPreview());

    this.metadataForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(255)]],
      description: ['', Validators.maxLength(1000)],
      expiresAt: [null]
    })
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files?.length) {
      this.processFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    const file = event.dataTransfer?.files[0];
    if (file) {
      this.processFile(file);
    }
  }

  private processFile(file: File): void {
    const validation = this.validateFile(file);
    if (!validation.valid) {
      this.fileError = validation.error ?? 'Arquivo inválido';
      return;
    }
    this.fileError = null;
    this.selectedFile = file;
    this.buildPreview(file);

    if (!this.metadataForm.get('title')?.value) {
      const nameWithoutExt = file.name.replace(/\.[^/.]+$/, '');
      this.metadataForm.patchValue({ title: nameWithoutExt });
    }
  }

  private validateFile(file: File): FileValidation {
    if (!this.allowedTypes.includes(file.type)) {
      return { valid: false, error: 'Tipo de arquivo não permitido. Aceitamos PDF, DOCX, DOC e TXT.' };
    }
    if (file.size > this.maxFileSize) {
      return { valid: false, error: `O arquivo excede o tamanho máximo permitido de ${this.maxFileSize} MB.` };
    }
    return { valid: true };
  }

  removeFile(): void {
    this.clearPreview();
    this.selectedFile = null;
    this.fileError = null;
  }

  getFileIcon(): string {
    if (!this.selectedFile) return 'insert_drive_file';
    const ext = this.selectedFile.name.split('.').pop()?.toLowerCase();
    const map: Record<string, string> = {
      pdf: 'picture_as_pdf',
      docx: 'description',
      doc: 'description',
      txt: 'notes'
    }
    return map[ext ?? ''] || 'insert_drive_file';
  }

  getFileIconColor(): string {
    if (!this.selectedFile) return '#94a3b8';
    const ext = this.selectedFile.name.split('.').pop()?.toLowerCase();
    const map: Record<string, string> = {
      pdf: '#e53935',
      docx: '#1565c0',
      doc: '#1e88e5',
      txt: '#43a047'
    }
    return map[ext ?? ''] || '#94a3b8';
  }

  get fileStepComplete(): boolean {
    return !!this.selectedFile && !this.fileError;
  }

  // Submit
  submit(): void {
    if (!this.selectedFile || !this.metadataForm.valid) return;

    this.uploading = true;
    this.uploadProgress = 0;

    const user = this.authService.getUserValue();
    const formValue = this.metadataForm.value;

    const progrressInterval = setInterval(() => {
      if (this.uploadProgress < 85) {
        this.uploadProgress += 10;
      }
    }, 200)

    this.documentService.createDocument(this.selectedFile, {
      title: formValue.title,
      description: formValue.description || undefined,
      expiresAt: formValue.expiresAt ? (formValue.expiresAt as Date).toISOString() : undefined,
      createdByUserId: user?.id || ''
    }).subscribe({
      next: (doc) => {
        clearInterval(progrressInterval);
        this.uploadProgress = 100;
        setTimeout(() => {
          this.snackBar.open('Documento enviado com sucesso!', 'Ver', { duration: 3000 }).onAction().subscribe(() => {
            this.router.navigate(['/documents', doc.id]);
          })
          this.router.navigate(['/documents', doc.id]);
        }, 500)
      },
      error: (err) => {
        clearInterval(progrressInterval);
        this.uploading = false;
        this.uploadProgress = 0;
        const msg = err?.error?.message || 'Erro ao enviar documento. Tente novamente.';
        this.snackBar.open(msg, 'Fechar', { duration: 5000 });
      }
    })
  }

  cancel(): void {
    this.router.navigate(['/documents']);
  }

  get canPreview(): boolean {
    return this.previewType === 'pdf' || this.previewType === 'text';
  }

  private buildPreview(file: File): void {
    this.clearPreview();
    this.previewLoading = true;

    if (file.type === 'application/pdf' || file.name.toLowerCase().endsWith('.pdf')) {
      const objectUrl = URL.createObjectURL(file);
      this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
      this.previewType = 'pdf';
      this.previewLoading = false;
      return;
    }

    if (file.type === 'text/plain' || file.name.toLowerCase().endsWith('.txt')) {
      const reader = new FileReader();
      reader.onload = () => {
        this.previewText = typeof reader.result === 'string' ? reader.result : '';
        this.previewType = 'text';
        this.previewLoading = false;
      };
      reader.onerror = () => {
        this.previewType = 'unsupported';
        this.previewLoading = false;
      };
      reader.readAsText(file);
      return;
    }

    this.previewType = 'unsupported';
    this.previewLoading = false;
  }

  private clearPreview(): void {
    if (this.previewUrl) {
      const unsafeUrl = this.sanitizer.sanitize(SecurityContext.RESOURCE_URL, this.previewUrl);
      if (unsafeUrl) {
        URL.revokeObjectURL(unsafeUrl);
      }
    }

    this.previewUrl = null;
    this.previewText = null;
    this.previewType = null;
    this.previewLoading = false;
  }

}
