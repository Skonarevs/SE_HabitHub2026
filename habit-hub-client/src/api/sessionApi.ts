import api from './axiosInstance';
import axios from 'axios';
import type { SessionInfo, SessionInfoDto } from '../features/auth/types';
//helper
const mapSessionDto = (dto: SessionInfoDto): SessionInfo => ({
  sessionId: dto.sessionId,
  createdAt: new Date(dto.createdAt),
  lastActivity: new Date(dto.lastActivity),
  expiryDate: new Date(dto.expiryDate),
  status: dto.status,
});

//new function

export const getActiveSessions = async (): Promise<SessionInfo[]> => {
  try {
    const response = await api.get<SessionInfoDto[]>('/auth/sessions');
    return response.data.map(mapSessionDto);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      if (import.meta.env.DEV) {
        console.error('getActiveSessions failed', {
          status,
          serverData,
          url: '/auth/sessions',
        });
      }

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData && typeof serverData === 'object' && 'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to load sessions (HTTP ${status}).`
            : 'Failed to load sessions. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while loading sessions.');
  }
};
