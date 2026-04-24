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
  Draft = 0,
  PendingSignatures = 1,
  PartiallyCompleted = 2,
  Completed = 3,
  Rejected = 4,
  Expired = 5,
  Cancelled = 6
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
  System = 7
}
