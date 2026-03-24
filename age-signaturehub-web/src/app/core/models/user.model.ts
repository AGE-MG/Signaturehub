export interface User {
  id: string;
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

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
