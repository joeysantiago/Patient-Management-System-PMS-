import { Medication } from './medication.model';
import { Vitals } from './vitals.model';

export interface VisitCreateRequest {
  appointmentId: string;
  vitals: Vitals;
  complaints: string;
  diagnosis: string;
  medications: Medication[];
}
