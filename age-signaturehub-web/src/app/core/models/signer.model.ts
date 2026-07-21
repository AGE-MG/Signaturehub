// Enums

export enum SignerRole {
  Signer = 1,
  Approver = 2,
  Witness = 3,
  Observer = 4,
}

export enum SignatureStatus {
  Pending = 1,
  Signed = 2,
  Rejected = 3,
  Expired = 4,
  Cancelled = 5,
}

export enum SignatureType {
  Electronic = 1,
  DigitalA1 = 2,
  DigitalA3 = 3,
  Biometric = 4,
}

// DTOs

export interface CertificateInfo {
  serialNumber?: string;
  subjectName?: string;
  issuerName?: string;
  validFrom?: Date;
  validTo?: Date;
  thumbprint?: string;
  isValid?: boolean;
  rawData?: string;
  password?: string;
}

export interface SignerDto {
  id: string;
  name?: string;
  email?: string;
  document?: string;
  role: SignerRole;
  signOrder: number;
  status: SignatureStatus;
  signatureType?: SignatureType;
  signedAt?: string;
  rejectionReason?: string;
  certificateInfo?: CertificateInfo;
  createdAt?: string;
}

export interface SignRequest {
  signerId: string;
  signatureType: SignatureType;
  certificateData?: number[];
  pin?: string;
  ipAddress?: string;
  userAgent?: string;
  deviceInfo?: string;
  location?: string;
}

export interface RejectRequest {
  signerId: string;
  reason?: string;
}

// Signature Flow

export interface CreateSignatureFlowDto {
  documentId: string;
  flowName: string;
  flowType: number;
  signers: CreateSignerDto[];
}

export interface CreateSignerDto {
  name: string;
  email: string;
  document: string;
  role: SignerRole;
  signOrder: number;
}

export interface SignatureFlowDetailDto {
  id: string;
  documentId: string;
  flowName: string;
  flowType: number;
  currentStep: number;
  totalSteps: number;
  isCompleted: boolean;
  signers: SignerDto[];
  completedAt?: string;
}

// Audit Log

export interface AuditLogDto {
  id: string;
  documentId: string;
  signerId?: string;
  userId?: string;
  action?: string;
  details?: string;
  ipAddress?: string;
  userAgent?: string;
  timestamp: string;
}

export interface AuditLogFilter {
  startDate: string;
  endDate: string
}

// User

export interface UserDto {
  id: string;
  networkUserName?: string;
  fullName: string;
  email: string;
  profilePicture?: string;
  department?: string;
  position?: string;
  registrationNumber?: string;
  roles: string[];
}

export interface UpdateProfileDto {
  fullName: string;
  department?: string;
  position?: string;
  registrationNumber?: string;
}


export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

// Display helper

export const SignerRoleLabel: Record<SignerRole, string> = {
  [SignerRole.Approver]: 'Aprovador',
  [SignerRole.Signer]: 'Assinante',
  [SignerRole.Witness]: 'Testemunha',
  [SignerRole.Observer]: 'Observador',
}

export const SignatureStatusLabel: Record<SignatureStatus, string> = {
  [SignatureStatus.Pending]: 'Pendente',
  [SignatureStatus.Signed]: 'Assinado',
  [SignatureStatus.Rejected]: 'Rejeitado',
  [SignatureStatus.Cancelled]: 'Cancelado',
  [SignatureStatus.Expired]: 'Expirado',
};

export const SignatureStatusColor: Record<SignatureStatus, string> = {
  [SignatureStatus.Pending]: '#F59E0B',
  [SignatureStatus.Signed]: '#10B981',
  [SignatureStatus.Rejected]: '#EF4444',
  [SignatureStatus.Cancelled]: '#6B7280',
  [SignatureStatus.Expired]: '#8B5CF6',
};

export const SignatureTypeLabel: Record<SignatureType, string> = {
  [SignatureType.Electronic]: 'Assinatura Eletrônica',
  [SignatureType.DigitalA1]: 'Certificado Digital A1',
  [SignatureType.DigitalA3]: 'Certificado Digital A3',
  [SignatureType.Biometric]: 'Biometria',
};

export const SignatureTypeIcon: Record<SignatureType, string> = {
  [SignatureType.Electronic]: 'draw',
  [SignatureType.DigitalA1]: 'verified_user',
  [SignatureType.DigitalA3]: 'security',
  [SignatureType.Biometric]: 'fingerprint',
};
