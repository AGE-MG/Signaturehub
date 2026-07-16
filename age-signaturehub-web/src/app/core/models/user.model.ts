export enum LoginMode {
  Internal = 1,
  ActiveDirectory = 2,
}

export interface User {
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

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
  loginMode: LoginMode;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  tokenExpiration: Date;
  user: User;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  confirmPassword: string;
  department?: string;
  position?: string;
  registrationNumber?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

export interface ExternalNotificationService {
  name: string;
  eventCount: number;
}

export interface NotificationCapabilities {
  emailConfigured: boolean;
  externalServices: ExternalNotificationService[];
}
