export interface AppointmentCreateRequest {
  patientId: string;
  scheduledAt: string;
  notes?: string | null;
}
