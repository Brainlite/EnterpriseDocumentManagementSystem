export interface LoginRequest {
  email: string;
  password: string;
}

export interface UserPayload {
  sub: string;
  email: string;
  role: string;
  name: string;
}

export interface LoginResponse {
  token: string;
  user: UserPayload;
}
