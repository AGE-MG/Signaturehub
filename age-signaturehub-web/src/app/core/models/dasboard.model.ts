export interface DashboardStats {
  pendingDocuments: number;
  signedToday: number;
  totalDocuments: number;
  waitingForOthers: number;
  trends?: {
    pendingTrend: number;
    signedTrend: number;
    totalTrend: number;
  };
}

export interface RecentDocument {
  id: string;
  title: string;
  fileName: string;
  status: DocumentStatus;
  createdAt: Date;
  totalSigners: number;
  signedCount: number;
  createdBy: string;
}

export enum DocumentStatus {
  Draft = 'Draft',
  PendingSignatures = 'PendingSignatures',
  PartiallyCompleted = 'PartiallyCompleted',
  Completed = 'Completed',
  Rejected = 'Rejected',
  Expired = 'Expired',
  Cancelled = 'Cancelled'
}

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  read: boolean;
  createdAt: Date;
  documentId?: string;
  actionUrl?: string;
}

export enum NotificationType {
  NewDocument = 'NewDocument',
  DocumentSigned = 'DocumentSigned',
  DocumentRejected = 'DocumentRejected',
  DocumentCompleted = 'DocumentCompleted',
  DocumentExpiring = 'DocumentExpiring',
  DocumentExpired = 'DocumentExpired'
}