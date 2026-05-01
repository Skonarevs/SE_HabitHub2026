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

export interface CreateHabitDto { //added please
  name: string; //added please
  goal: string; //added please
  habitType: 'Binary' | 'Quantitative'; //added please
  unit: string; //added please
  expiryDate?: string; //added please
} //added please

export interface UpdateHabitDto {
  name?: string;
  goal?: string;
  habitType?: 'Binary' | 'Quantitative';
  unit?: string;
  expiryDate?: string;
}

export interface HabitResponseDto { //added please
  id: string; //added please
  name: string; //added please
  goal: string; //added please
  habitType: string; //added please
  unit?: string | null; //added please
  expiryDate?: string | null; //added please
  state: string; //added please
  teamId: string; //added please
  teamName?: string | null; //added please
} //added please

export interface ArchivedHabitResponseDto {
  id: string;
  name: string;
  goal: string;
  habitType: string;
  unit?: string | null;
  expiryDate?: string | null;
  archivedAt?: string | null;
}

export interface TeamHabitInfo { //added please
  id: string; //added please
  name: string; //added please
  goal: string; //added please
  habitType: string; //added please
  unit?: string; //added please
  expiryDate?: Date; //added please
  state: string; //added please
  teamId: string; //added please
} //added please
