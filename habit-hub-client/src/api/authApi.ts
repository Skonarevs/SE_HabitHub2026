import axios from 'axios';
import api from './axiosInstance';
import type { ChangePasswordDto, ChangeEmailDto } from '../features/auth/types';

export const changePassword = async (dto: ChangePasswordDto): Promise<void> => {
  try {
    await api.post('/auth/change-password', dto);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status === 400
            ? 'Invalid password data. Please check and try again.'
            : status === 401
              ? 'Current password is incorrect.'
              : `Failed to change password (HTTP ${status}).`)
      );
    }

    throw new Error('Unexpected error while changing password.');
  }
};

export const changeEmail = async (dto: ChangeEmailDto): Promise<void> => {
  try {
    await api.post('/auth/change-email', dto);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status === 400
            ? 'Invalid email or password.'
            : status === 401
              ? 'Password is incorrect.'
              : status === 409
                ? 'This email is already in use.'
                : `Failed to change email (HTTP ${status}).`)
      );
    }

    throw new Error('Unexpected error while changing email.');
  }
};
