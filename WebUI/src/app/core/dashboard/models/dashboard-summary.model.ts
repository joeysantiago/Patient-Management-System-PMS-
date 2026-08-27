import { Appointment } from '../../appointments/models/appointment.model';
import { Patient } from '../../patients/models/patient.model';

export interface DashboardSummary {
  newPatientsLast30Days: number;
  todaysAppointmentCount: number;
  todaysAppointments: Appointment[];
  recentPatients: Patient[];
}
