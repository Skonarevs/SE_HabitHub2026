import type {
  ArchivedHabitResponseDto,
  CreateHabitDto,
  HabitResponseDto,
  TeamHabitInfo,
  TeamInfo,
  TeamResponseDto,
  UpdateHabitDto,
} from '../types/teamsTypes';
import api from './axiosInstance';
import axios from 'axios';

const getAxiosErrorMessage = (error: unknown, fallback: string): string => {
  if (!axios.isAxiosError(error)) {
    return fallback;
  }

  const status = error.response?.status;
  const serverData = error.response?.data;

  const serverMessage =
    typeof serverData === 'string'
      ? serverData
      : serverData && typeof serverData === 'object' && 'error' in serverData
        ? String(serverData.error)
        : null;

  return (
    serverMessage ||
    (status
      ? `${fallback} (HTTP ${status}).`
      : `${fallback} Please check your connection.`)
  );
};

const mapTeamDto = (dto: TeamResponseDto): TeamInfo => ({
  teamId: dto.id,
  name: dto.name,
  creatorId: dto.creatorId,
  createdAt: dto.createdAt ? new Date(dto.createdAt) : undefined,
  inviteCode: dto.inviteCode ?? null,
});

const getTeamDetails = async (teamId: string): Promise<TeamResponseDto> => {
  const response = await api.get<TeamResponseDto>(`/teams/${teamId}`);
  return response.data;
};

const mapHabitDto = (dto: HabitResponseDto): TeamHabitInfo => ({
 
  id: dto.id,
  name: dto.name,
  goal: dto.goal,
  habitType: dto.habitType,
  unit: dto.unit ?? undefined,
  expiryDate: dto.expiryDate ? new Date(dto.expiryDate) : undefined,
  state: dto.state,
  teamId: dto.teamId,
});

const mapArchivedHabitDto = (
  dto: ArchivedHabitResponseDto,
  teamId: string
): TeamHabitInfo => ({
  id: dto.id,
  name: dto.name,
  goal: dto.goal,
  habitType: dto.habitType,
  unit: dto.unit ?? undefined,
  expiryDate: dto.expiryDate ? new Date(dto.expiryDate) : undefined,
  state: 'Archived',
  teamId,
});

export interface ReminderResponseDto {
  habitId: string;
  habitName: string;
  enabled: boolean;
  reminderTime: string;
  teamName?: string; // Optional in case some habits are personal
}

export interface TeamMemberDto {
  userId: string;
  name: string;
  email: string;
  joinedAt: string;
  status: string;
}

export const getUserReminders = async (): Promise<ReminderResponseDto[]> => {
  try {
    const response = await api.get<ReminderResponseDto[]>('/reminders');
    return response.data;
  } catch (error) {
    console.error('Failed to fetch reminders', error);
    throw new Error('Failed to load reminders.');
  }
};

export const leaveTeam = async (teamId: string): Promise<void> => {
  try {
    await api.post(`/teams/${teamId}/leave`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;
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

export const getTeamsInfo = async (): Promise<TeamInfo[]> => {
  try {
    const response = await api.get<TeamResponseDto[]>('/teams');
    const teams = await Promise.all(
      response.data.map(async (team) => {
        try {
          const detailedTeam = await getTeamDetails(team.id);
          return mapTeamDto(detailedTeam);
        } catch {
          return mapTeamDto(team);
        }
      })
    );

    return teams;
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
export const joinTeam = async (data: { code: string }): Promise<void> => {
  try {
    const response = await api.post('/teams/join', data);
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData &&
              typeof serverData === 'object' &&
              'error' in serverData
            ? String(serverData.error)
            : null;

      if (status === 400 || status === 404) {
        throw new Error(
          serverMessage || 'Invalid invite code. Please check and try again.'
        );
      }

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to join team (HTTP ${status}).`
            : 'Failed to join team. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while joining team.');
  }
};

export const createTeam = async (name: string) => {
  const response = await api.post('/teams', { name: name });
  return response.data;
};

export const getActiveHabits = async (
  teamId: string
): Promise<TeamHabitInfo[]> => {
  try {
    const response = await api.get<HabitResponseDto[]>(
      `/teams/${teamId}/habits`,
      {
        params: { state: 'active' },
      }
    );

    return response.data.map(mapHabitDto);
  } catch (error) {
    throw new Error(
      getAxiosErrorMessage(error, 'Failed to load active habits')
    );
  }
};

export const getArchivedHabits = async (
  teamId: string
): Promise<TeamHabitInfo[]> => {
  try {
    const response = await api.get<ArchivedHabitResponseDto[]>(
      `/teams/${teamId}/habits`,
      {
        params: { state: 'archived' },
      }
    );

    return response.data.map((habit) => mapArchivedHabitDto(habit, teamId));
  } catch (error) {
    throw new Error(
      getAxiosErrorMessage(error, 'Failed to load archived habits')
    );
  }
};

export const createTeamHabit = async (
 
  teamId: string,
  dto: CreateHabitDto
): Promise<TeamHabitInfo> => {
 
  try {
   
    const response = await api.post<HabitResponseDto>(
      `/teams/${teamId}/habits`,
      dto
    );
    return mapHabitDto(response.data);
  } catch (error) {
   
    if (axios.isAxiosError(error)) {
     
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to create habit (HTTP ${status}).`
            : 'Failed to create habit. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while creating habit.');
  }
};

export const updateHabit = async (
  habitId: string,
  dto: UpdateHabitDto
): Promise<TeamHabitInfo> => {
  try {
    const response = await api.patch<HabitResponseDto>(
      `/habits/${habitId}`,
      dto
    );
    return mapHabitDto(response.data);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to update habit (HTTP ${status}).`
            : 'Failed to update habit. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while updating habit.');
  }
};

export const archiveHabit = async (habitId: string): Promise<void> => {
  try {
    await api.post(`/habits/${habitId}/archive`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to archive habit (HTTP ${status}).`
            : 'Failed to archive habit. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while archiving habit.');
  }
};

export const deleteHabit = async (habitId: string): Promise<void> => {
  try {
    await api.delete(`/habits/${habitId}`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to delete habit (HTTP ${status}).`
            : 'Failed to delete habit. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while deleting habit.');
  }
};

export const deleteTeam = async (teamId: string): Promise<void> => {
  try {
    await api.delete(`/teams/${teamId}`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to delete team (HTTP ${status}).`
            : 'Failed to delete team. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while deleting team.');
  }
};

export const getTeamMembers = async (
  teamId: string
): Promise<TeamMemberDto[]> => {
  try {
    const response = await api.get<TeamMemberDto[]>(`/teams/${teamId}/members`);
    console.log(response.data);
    return response.data;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

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
            ? `Failed to get team (HTTP ${status}).`
            : 'Failed to get team. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while getting team.');
  }
};

export const setHabitReminder = async (
  habitId: string,
  reminderTime: string
): Promise<void> => {
  try {
    await api.patch(`/habits/${habitId}/reminder`, { reminderTime });
  } catch (error) {
    console.error('Failed to set reminder', error);
    throw new Error('Failed to save reminder to server.');
  }
};

export const toggleHabitReminder = async (
  habitId: string,
  enabled: boolean
): Promise<void> => {
  try {
    await api.patch(`/habits/${habitId}/reminder/enabled`, { enabled });
  } catch (error) {
    console.error('Failed to toggle reminder', error);
    throw new Error('Failed to update reminder status.');
  }
};

export const kickTeamMember = async (
  teamId: string,
  memberId: string
): Promise<void> => {
  try {
    await api.post(`/teams/${teamId}/members/${memberId}/kick`);
  } catch (error) {
    if (axios.isAxiosError(error)) {
      const status = error.response?.status;
      const serverData = error.response?.data;

      const serverMessage =
        typeof serverData === 'string'
          ? serverData
          : serverData?.message || serverData?.error || null;

      throw new Error(
        serverMessage ||
          (status
            ? `Failed to remove member (HTTP ${status}).`
            : 'Failed to remove member. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while removing member.');
  }
};
