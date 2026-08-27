import { ClinicSettings } from '../../settings/models/clinic-settings.model';
import { Patient } from '../../patients/models/patient.model';
import { Visit } from './visit.model';

export interface Prescription {
  clinic: ClinicSettings;
  patient: Patient;
  visit: Visit;
}
