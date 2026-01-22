export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}
export interface TokenResponse {
  token: string;
  expires: string;
}
export interface User {
  id: string;
  email: string;
  fullName: string;
}
