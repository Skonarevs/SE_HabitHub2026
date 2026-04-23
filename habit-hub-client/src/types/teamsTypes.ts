export interface CreateTeamDto {
  name: string;
}

export interface TeamInfo {
  teamId: string;
  name: string;
  creatorId: string;
  createdAt: Date;
}
export interface TeamResponseDto {
  id: string;
  name: string;
  creatorId: string;
  createdAt: Date;
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
