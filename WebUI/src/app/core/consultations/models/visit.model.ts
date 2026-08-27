import { Medication } from './medication.model';
import { Vitals } from './vitals.model';

export interface Visit {
  id: string;
  patientId: string;
  appointmentId: string;
  visitDateUtc: string;
  vitals: Vitals;
  complaints: string;
  diagnosis: string;
  medications: Medication[];
  vitalsOutOfRange: boolean;
  vitalsWarning: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}
