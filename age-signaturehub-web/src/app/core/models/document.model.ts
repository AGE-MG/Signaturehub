export enum DocumentStatus {
  Draft = 1,
  PendingSignatures = 2,
  PartiallyCompleted = 3,
  Completed = 4,
  Rejected = 5,
  Expired = 6,
  Cancelled = 7
}

// DTOs

export interface SignatoryDto {
  id: string;
  name?: string;
  email: string;
  signOrder?: number;
  status?: number;
  signedAt?: string;
  completedAt?: string;
  rejectionReason?: string;
  signatureType?: number;
}

export interface SignatureFlowDto {
  id: string;
  documentId: string;
  flowName: string;
  flowType: number;
  currentStep: number;
  totalSteps: number;
  isCompleted: boolean;
  signers: SignatoryDto[];
  completedAt?: string;
}

export interface DocumentDto {
  id: string;
  fileName: string;
  originalFileName: string;
  fileExtension: string;
  fileSizeInBytes: number;
  mimeType: string;
  status: DocumentStatus;
  title: string;
  description?: string;
  expiresAt?: string;
  createdByUserId: string;
  owningDepartment: string;
  isConfidential: boolean;
  createdAt: string;
  updatedAt?: string;
  signatureFlows: SignatureFlowDto[];
}

export interface CreateDocumentDto {
  title: string;
  description?: string;
  expiresAt?: string;
  createdByUserId: string;
  isConfidential?: boolean;
}

export interface TransferDocumentDepartmentDto {
  targetUserId: string;
  reason: string;
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
}

// Helpers

export const DocumentStatusLabel: Record<DocumentStatus, string> = {
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

export const DocumentStatusMatColor: Record<DocumentStatus, 'warn' | 'primary' | 'accent' | undefined> = {
  [DocumentStatus.Draft]: undefined,
  [DocumentStatus.PendingSignatures]: 'warn',
  [DocumentStatus.PartiallyCompleted]: 'accent',
  [DocumentStatus.Completed]: 'primary',
  [DocumentStatus.Rejected]: 'warn',
  [DocumentStatus.Expired]: 'accent',
  [DocumentStatus.Cancelled]: undefined,
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
