import type {
  ArchivedHabitResponseDto,
  CreateHabitDto, //added please
  HabitResponseDto, //added please
  TeamHabitInfo, //added please
  TeamInfo, //added please
  TeamResponseDto, //added please
  UpdateHabitDto,
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

const mapHabitDto = (dto: HabitResponseDto): TeamHabitInfo => ({ //added please
  id: dto.id, //added please
  name: dto.name, //added please
  goal: dto.goal, //added please
  habitType: dto.habitType, //added please
  unit: dto.unit ?? undefined, //added please
  expiryDate: dto.expiryDate ? new Date(dto.expiryDate) : undefined, //added please
  state: dto.state, //added please
  teamId: dto.teamId, //added please
}); //added please

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

export const createTeam = async (name: string) => {
  const response = await api.post('/teams', { name: name });
  return response.data;
};

export const getActiveHabits = async (teamId: string): Promise<TeamHabitInfo[]> => { //added please
  try { //added please
    const response = await api.get<HabitResponseDto[]>(`/teams/${teamId}/habits`, { //added please
      params: { state: 'active' }, //added please
    }); //added please
    return response.data.map(mapHabitDto); //added please
  } catch (error) { //added please
    if (axios.isAxiosError(error)) { //added please
      const status = error.response?.status; //added please
      const serverData = error.response?.data; //added please

      const serverMessage = //added please
        typeof serverData === 'string' //added please
          ? serverData //added please
          : serverData && typeof serverData === 'object' && 'error' in serverData //added please
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

export const getArchivedHabits = async (teamId: string): Promise<TeamHabitInfo[]> => {
  try {
    const response = await api.get<ArchivedHabitResponseDto[]>(`/teams/${teamId}/habits`, {
      params: { state: 'archived' },
    });

    return response.data.map((habit) => mapArchivedHabitDto(habit, teamId));
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
          (status
            ? `Failed to load archived habits (HTTP ${status}).`
            : 'Failed to load archived habits. Please check your connection.')
      );
    }

    throw new Error('Unexpected error while loading archived habits.');
  }
};

export const createTeamHabit = async ( //added please
  teamId: string, //added please
  dto: CreateHabitDto //added please
): Promise<TeamHabitInfo> => { //added please
  try { //added please
    const response = await api.post<HabitResponseDto>(`/teams/${teamId}/habits`, dto); //added please
    return mapHabitDto(response.data); //added please
  } catch (error) { //added please
    if (axios.isAxiosError(error)) { //added please
      const status = error.response?.status; //added please
      const serverData = error.response?.data; //added please

      const serverMessage = //added please
        typeof serverData === 'string' //added please
          ? serverData //added please
          : serverData && typeof serverData === 'object' && 'error' in serverData //added please
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
    const response = await api.patch<HabitResponseDto>(`/habits/${habitId}`, dto);
    return mapHabitDto(response.data);
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
          : serverData && typeof serverData === 'object' && 'error' in serverData
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
          : serverData && typeof serverData === 'object' && 'error' in serverData
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
