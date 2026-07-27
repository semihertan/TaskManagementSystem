import { User } from './user.interface';

export type UserProfile = User;

export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  username: string;
  email: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}
