import type { User } from '../../types/user.ts';

export interface LoginResponse {
  sessionId: string;
  user: User;
}


export interface SessionInfoDto {
  sessionId: string;
  createdAt: string;
  lastActivity: string;
  expiryDate: string;
  status: string;
}

export interface SessionInfo {
  sessionId: string;
  createdAt: Date;
  lastActivity: Date;
  expiryDate: Date;
  status: string;
}

export interface ChangePasswordDto {
  currentPassword: string;
  newPassword: string;
}

export interface ChangeEmailDto {
  newEmail: string;
  password: string;
}
