import type { TeamInfo, TeamResponseDto } from '../types/teamsTypes';
import api from './axiosInstance';
import axios from 'axios';

const mapTeamDto = (dto: TeamResponseDto): TeamInfo => ({
  teamId: dto.id,
  name: dto.name,
  creatorId: dto.creatorId,
  createdAt: new Date(dto.createdAt),
});

export const getTeamsInfo = async (): Promise<TeamInfo[]> => {
  try {
    const response = await api.get<TeamResponseDto[]>('/teams');
    return response.data.map(mapTeamDto);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      if (import.meta.env.DEV) {
        console.error('getTeamsInfo failed', {
          status,
          serverData,
          url: '/teams',
        });
      }

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData &&
              typeof serverData === 'object' &&
              'error' in serverData
            ? String(serverData.error)
            : null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to load teams (HTTP ${status}).`
            : 'Failed to load teams. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while loading teams.');
  }
};

export const createTeam = async (name: string) => {
  const response = await api.post('/teams', { name: name });
  return response.data;
};
