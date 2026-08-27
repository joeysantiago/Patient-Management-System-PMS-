import { Visit } from '../../consultations/models/visit.model';

export interface PatientHistory {
  patientId: string;
  patientName: string;
  visits: Visit[];
}
