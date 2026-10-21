import { OrganizationRole } from './enum/organization-role.enum';

export interface LoginRequest {
  email: string;
  password: string;
}
export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
  organizationName: string;
}
export interface TokenResponse {
  token: string;
  expires: string;
  refreshToken?: string;
}
export interface User {
  id: string;
  email: string;
  fullName: string;
  organizationId?: string;
  organizationName?: string;
  role: OrganizationRole;
}

export interface UserOrgDto {
  id: string;
  email: string;
  fullName: string;
  organizationId?: string;
  organizationName?: string;
  role: OrganizationRole;
  avatar?: string;
}

export interface Invitation {
  id: string;
  email: string;
  role: string; // 'Employee' | 'Manager' | 'Admin'
  status: string; // 'Pending' | 'Accepted' | 'Revoked'
  expiresAt: string;
  createdAt: string;
}

export interface CreateInvitationRequest {
  email: string;
  role: string;
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

export interface ReportSummary {
  totalHours: number;
  totalChange: number;
  avgDailyHours: number;
  avgChange: number;
  productivity: number;
  productivityChange: number;
  focusHours: number;
  focusChange: number;
}

export interface ProjectBreakdown {
  name: string;
  hours: number;
  percentage: number;
}

export interface WeeklyPoint {
  week: string;
  hours: number;
  target: number;
}

export interface TeamMember {
  name: string;
  avatar: string;
  hours: number;
  productivity: number;
}

// Perfil (GET/PUT /api/users/me)
export interface Profile {
  id: string;
  email: string;
  fullName: string;
  role: string;
  timezone: string | null;
  language: string | null;
}

export interface UpdateProfileRequest {
  fullName?: string;
  email?: string;
  timezone?: string;
  language?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// Se llama AppNotification para no chocar con el tipo global 'Notification' del navegador.
export interface AppNotification {
  id: string;
  title: string;
  message: string;
  type: string; // 'Info' | 'Welcome' | 'Activity' | 'Project'
  isRead: boolean;
  createdAt: string;
}
