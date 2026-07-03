export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  expiresAt: string;
}

export interface SessionResponse {
  expiresAt: string;
}
