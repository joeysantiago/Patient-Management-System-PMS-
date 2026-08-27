import { AppointmentStatus } from './appointment-status.model';

export interface Appointment {
  id: string;
  patientId: string;
  patientName: string;
  patientPhoneNumber: string;
  scheduledAt: string;
  status: AppointmentStatus;
  notes: string | null;
  createdAtUtc: string;
}
