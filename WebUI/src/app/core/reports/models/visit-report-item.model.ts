import { Medication } from '../../consultations/models/medication.model';
import { Vitals } from '../../consultations/models/vitals.model';

export interface VisitReportItem {
  visitId: string;
  patientId: string;
  patientName: string;
  visitDateUtc: string;
  complaints: string;
  diagnosis: string;
  vitals: Vitals;
  medications: Medication[];
}
