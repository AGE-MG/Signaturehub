import { DocumentStatus } from "./document.model";
import { SignatureType } from "./signer.model";

export interface PublicDocumentVerification {
  documentId: string;
  versionNumber: number;
  title: string;
  originalFileName: string;
  contentHash: string;
  status: DocumentStatus;
  createdAt: string;
  updatedAt?: string;
  verificationUrl: string;
  signedSigners: PublicSignedSigner[];
}

export interface PublicSignedSigner {
  signerId: string;
  name: string;
  email: string;
  signatureType?: SignatureType;
  signedAt?: string;
}
