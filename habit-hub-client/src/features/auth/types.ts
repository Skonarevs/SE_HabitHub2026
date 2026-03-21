import type { User } from "../../types/user.ts";

export interface LoginResponse {
    sessionId: string;
    user: User;
}


