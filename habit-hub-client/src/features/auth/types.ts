export interface LoginResponse {
    sessionId: string;
    user: User;
}

export interface User{
    id:string;
    name:string;
    email:string;
    timezone: string;
}
