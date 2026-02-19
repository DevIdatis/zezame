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

export interface TimeEntry {
  id: string;
  userId: string;
  startTime: string; // ISO string desde .NET
  endTime: string | null;
  durationMin: number | null;
  description: string | null;
  type: number;
}

export enum EntryType {
  Work = 0,
  Break = 1,
  Lunch = 2,
}
