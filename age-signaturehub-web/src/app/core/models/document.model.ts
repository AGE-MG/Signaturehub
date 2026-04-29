export enum DocumentStatus {
  Draft = 0,
  PendingSignatures = 1,
  PartiallyCompleted = 2,
  Completed = 3,
  Rejected = 4,
  Expired = 5,
  Cancelled = 6
}

export enum DocumentSource {
  internal = 'internal',
  TJMG = 'TJMG',
  Tribunus = 'Tribunus',
  external = 'external'
}

// DTOs

export interface SignatoryDto {
  id: string;
  userId: string;
  userName: string;
  email: string;
  signedAt: Date | null;
  status: DocumentStatus;
}

export interface SignatureFlowDto {
  id: string;
  documentId: string;
  order: number;
  status: string;
  signatories: SignatoryDto[];
  createdAt: string;
  completedAt?: string;
}

export interface DocumentDto {
  id: string;
  filename: string;
  originalFilename: string;
  fileExtension: string;
  fileSizeInBytes: number;
  mimeType: string;
  status: DocumentStatus;
  title: string;
  description?: string;
  expiresAt?: Date;
  createdByUserId: string;
  createdAt: Date;
  updatedAt: Date;
  source: DocumentSource;
  signatureFlows: SignatureFlowDto[];
}

export interface CreateDocumentDto {
  title: string;
  description?: string;
  expiresAt?: Date;
  createdByUserId: string;
  source?: DocumentSource;
}

export interface DocumentPagedResult {
  items: DocumentDto[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface DocumentFilterParams {
  status?: DocumentStatus;
  source?: DocumentSource;
  search?: string;
  pageNumber?: number;
  pageSize?: number;
}

// Helpers

export const DocumentStatusLabels: Record<DocumentStatus, string> = {
  [DocumentStatus.Draft]: 'Rascunho',
  [DocumentStatus.PendingSignatures]: 'Aguardando Assinaturas',
  [DocumentStatus.PartiallyCompleted]: 'Parcialmente Assinado',
  [DocumentStatus.Completed]: 'Concluído',
  [DocumentStatus.Rejected]: 'Rejeitado',
  [DocumentStatus.Expired]: 'Expirado',
  [DocumentStatus.Cancelled]: 'Cancelado',
}

export const DocumentStatusColor: Record<DocumentStatus, string> = {
  [DocumentStatus.Draft]: '#6B7280',
  [DocumentStatus.PendingSignatures]: '#F59E0B',
  [DocumentStatus.PartiallyCompleted]: '#3B82F6',
  [DocumentStatus.Completed]: '#10B981',
  [DocumentStatus.Rejected]: '#EF4444',
  [DocumentStatus.Expired]: '#8B5CF6',
  [DocumentStatus.Cancelled]: '#6B7280',
}

export function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  const units = ['KB', 'MB', 'GB', 'TB'];
  let unitIndex = -1;
  let size = bytes;
  do {
    size /= 1024;
    unitIndex++;
  } while (size >= 1024 && unitIndex < units.length - 1);
  return `${size.toFixed(2)} ${units[unitIndex]}`;
}
