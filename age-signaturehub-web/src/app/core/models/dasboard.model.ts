export interface DashboardStats {
  totalDocuments: number;
  draftDocuments: number;
  pendingDocuments: number;
  completedDocuments: number;
  rejectedDocuments: number;
  expiredDocuments: number;
  unreadNotifications: number;
}

export interface RecentDocument {
  id: string;
  title: string;
  originalFileName: string;
  fileExtension: string;
  status: DocumentStatus;
  createdAt: string;
  updatedAt: string;
}

export enum DocumentStatus {
  Draft = 1,
  PendingSignatures = 2,
  PartiallyCompleted = 3,
  Completed = 4,
  Rejected = 5,
  Expired = 6,
  Cancelled = 7
}

export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  relatedDocumentId?: string;
  createdAt: string;
}

export enum NotificationType {
  DocumentCreated = 1,
  DocumentSigned = 2,
  DocumentRejected = 3,
  DocumentExpired = 4,
  DocumentCompleted = 5,
  SignatureRequested = 6,
  System = 7,
  DocumentUpdated = 8,
  DocumentDeleted = 9
}
