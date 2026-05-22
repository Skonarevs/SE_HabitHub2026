import type {
  ArchivedHabitResponseDto,
  CreateHabitDto, //added please
  HabitEntryResponseDto,
  HabitResponseDto, //added please
  TeamHabitInfo, //added please
  TeamInfo, //added please
  TeamResponseDto, //added please
  UpdateHabitDto,
  HabitLeaderboardEntry,
} from '../types/teamsTypes'; //added please
import api from './axiosInstance';
import axios from 'axios';

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
  reminderTime: dto.reminderTime,
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
  reminderTime: dto.reminderTime,
});

export interface ReminderResponseDto {
  habitId: string;
  habitName: string;
  enabled: boolean;
  reminderTime: string;
  teamName?: string;
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
  //added please
  try {
    //added please
    const response = await api.get<HabitResponseDto[]>(
      `/teams/${teamId}/habits`,
      {
        //added please
        params: { state: 'active' }, //added please
      }
    ); //added please
    return response.data.map(mapHabitDto); //added please
  } catch (error) {
    //added please
    if (axios.isAxiosError(error)) {
      //added please
      const status = error.response?.status; //added please
      const serverData = error.response?.data; //added please

      const serverMessage = //added please
        typeof serverData === 'string' //added please
          ? serverData //added please
          : serverData &&
              typeof serverData === 'object' &&
              'error' in serverData //added please
            ? String(serverData.error) //added please
            : null; //added please

      throw new Error( //added please
        serverMessage || //added please
          (status //added please
            ? `Failed to load active habits (HTTP ${status}).` //added please
            : 'Failed to load active habits. Please check your connection.') //added please
      ); //added please
    } //added please

    throw new Error('Unexpected error while loading active habits.'); //added please
  } //added please
}; //added please

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
            ? `Failed to load archived habits (HTTP ${status}).`
            : 'Failed to load archived habits. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while loading archived habits.');
  }
};

export const getHabitLeaderboard = async (
  habitId: string
): Promise<HabitLeaderboardEntry[]> => {
  try {
    const response = await api.get(`/habits/${habitId}/leaderboard`);

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
            ? `Failed to load leaderboard (HTTP ${status}).`
            : 'Failed to load leaderboard. Please check your connection.')
      );
    }
    return [];
  }
};
export const createTeamHabit = async (
  //added please
  teamId: string, //added please
  dto: CreateHabitDto //added please
): Promise<TeamHabitInfo> => {
  //added please
  try {
    //added please
    const response = await api.post<HabitResponseDto>(
      `/teams/${teamId}/habits`,
      dto
    ); //added please
    return mapHabitDto(response.data); //added please
  } catch (error) {
    //added please
    if (axios.isAxiosError(error)) {
      //added please
      const status = error.response?.status; //added please
      const serverData = error.response?.data; //added please

      const serverMessage = //added please
        typeof serverData === 'string' //added please
          ? serverData //added please
          : serverData &&
              typeof serverData === 'object' &&
              'error' in serverData //added please
            ? String(serverData.error) //added please
            : null; //added please

      throw new Error( //added please
        serverMessage || //added please
          (status //added please
            ? `Failed to create habit (HTTP ${status}).` //added please
            : 'Failed to create habit. Please check your connection.') //added please
      ); //added please
    } //added please

    throw new Error('Unexpected error while creating habit.'); //added please
  } //added please
}; //added please

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

export interface TeamMemberResponseDto {
  userId: string;
  name: string;
  email: string;
  joinedAt: string;
  status: string;
}

export const getTeamMembers = async (
  teamId: string
): Promise<TeamMemberResponseDto[]> => {
  try {
    const response = await api.get<TeamMemberResponseDto[]>(
      `/teams/${teamId}/members`
    );
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

export interface LogProgressDto {
  status: 'Logged' | 'Skipped';
  value?: number;
  notes?: string;
}

export const logProgress = async (
  habitId: string,
  dto: LogProgressDto
): Promise<HabitEntryResponseDto> => {
  try {
    const response = await api.post<HabitEntryResponseDto>(
      `/habits/${habitId}/entries`,
      dto
    );
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
          (status === 409
            ? 'Already logged for today.'
            : status
              ? `Failed to log progress (HTTP ${status}).`
              : 'Failed to log progress. Please check your connection.')
      );
    }
    throw new Error('Unexpected error while logging progress.');
  }
};

export const getHabitEntries = async (
  habitId: string
): Promise<HabitEntryResponseDto[]> => {
  try {
    const response = await api.get<HabitEntryResponseDto[]>(
      `/habits/${habitId}/entries`
    );
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
            ? `Failed to load habit entries (HTTP ${status}).`
            : 'Failed to load habit entries. Please check your connection.')
      );
    }
    throw new Error('Unexpected error while loading habit entries.');
  }
};

export const getAllMemberHabits = async (
  teamId: string
): Promise<TeamHabitInfo[]> => {
  try {
    const response = await api.get<HabitResponseDto[]>(
      `/teams/${teamId}/habits`
    );

    return response.data.map(mapHabitDto);
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
            ? `Failed to load member habits (HTTP ${status}).`
            : 'Failed to load member habits. Please check your connection.')
      );
    }
    throw new Error('Unexpected error while loading member habits.');
  }
};

export const undoLog = async (
  habitId: string,
  entryId: string
): Promise<void> => {
  try {
    await api.delete(`/habits/${habitId}/entries/${entryId}`);
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
            ? `Failed to undo log (HTTP ${status}).`
            : 'Failed to undo log. Please check your connection.')
      );
    }
    throw new Error('Unexpected error while undoing log.');
  }
};
