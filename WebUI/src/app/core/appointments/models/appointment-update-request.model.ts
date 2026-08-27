import { AppointmentStatus } from './appointment-status.model';

export interface AppointmentUpdateRequest {
  patientId: string;
  scheduledAt: string;
  status: AppointmentStatus;
  notes?: string | null;
}
