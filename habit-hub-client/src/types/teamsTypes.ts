export interface CreateTeamDto {
  name: string;
}

export interface TeamInfo {
  teamId: string;
  name: string;
  creatorId: string;
  createdAt?: Date;
  inviteCode?: string | null;
}
export interface TeamResponseDto {
  id: string;
  name: string;
  creatorId: string;
  createdAt?: string | Date | null;
  inviteCode?: string | null;
}

export interface InviteCodeResponseDto {
  code: string;
  expiryDate: Date;
}

export interface JoinTeamDto {
  code: string;
}

export interface MembershipResponseDto {
  teamId: string;
  teamName: string;
  status: string;
}

export interface CreateHabitDto {
  name: string;
  goal: string;
  habitType: 'Binary' | 'Quantitative';
  unit: string;
  expiryDate?: string;
}

export interface UpdateHabitDto {
  name?: string;
  goal?: string;
  habitType?: 'Binary' | 'Quantitative';
  unit?: string;
  expiryDate?: string;
}

export interface HabitResponseDto {
  id: string;
  name: string;
  goal: string;
  habitType: string;
  unit?: string | null;
  expiryDate?: string | null;
  state: string;
  teamId: string;
  teamName?: string | null;
}

export interface ArchivedHabitResponseDto {
  id: string;
  name: string;
  goal: string;
  habitType: string;
  unit?: string | null;
  expiryDate?: string | null;
  archivedAt?: string | null;
}

export interface TeamHabitInfo {
  id: string;
  name: string;
  goal: string;
  habitType: string;
  unit?: string;
  expiryDate?: Date;
  state: string;
  teamId: string;
}
