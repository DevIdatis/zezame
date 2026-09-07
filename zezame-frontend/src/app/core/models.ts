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

// Coincide con ProjectDto del backend
export interface Project {
  id: string;
  name: string;
  description: string | null;
  color: string;
  emoji: string | null;
  status: string; // 'Active' | 'Paused' | 'Completed'
  deadline: string | null;
  hours: number;
  progress: number;
  memberCount: number;
  tasksCompleted: number;
  tasksTotal: number;
  ownerId: string;
  ownerName: string;
  createdAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description?: string | null;
  color?: string | null;
  emoji?: string | null;
  deadline?: string | null;
}

export interface UpdateProjectRequest {
  name?: string | null;
  description?: string | null;
  color?: string | null;
  emoji?: string | null;
  deadline?: string | null;
  status?: string | null;
}

// Coincide con ActivityDto del backend
export interface Activity {
  id: string;
  name: string;
  description: string | null;
  emoji: string | null;
  status: string;
  projectId: string;
  projectName: string;
  assignedToName: string | null;
  estimatedMinutes: number | null;
  actualMinutes: number | null;
  createdAt: string;
}

export interface CreateActivityRequest {
  name: string;
  description?: string | null;
  projectId: string;
  emoji?: string | null;
  assignedToId?: string | null;
  estimatedMinutes?: number | null;
}

// Reporting
export interface CalendarDay {
  day: number;
  hours: number;
  events: number;
}

export interface DayEntry {
  id: string;
  description: string | null;
  projectName: string | null;
  activityName: string | null;
  startTime: string;
  endTime: string | null;
  durationMin: number | null;
  type: string; // 'Work' | 'Break'
}

export interface DaySummary {
  date: string;
  totalMinutes: number;
  taskCount: number;
  entries: DayEntry[];
}
