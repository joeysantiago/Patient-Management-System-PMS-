export enum AppointmentStatus {
  Scheduled = 0,
  Completed = 1,
  Cancelled = 2,
  NoShow = 3,
}

export const APPOINTMENT_STATUS_LABELS: Record<AppointmentStatus, string> = {
  [AppointmentStatus.Scheduled]: 'Scheduled',
  [AppointmentStatus.Completed]: 'Completed',
  [AppointmentStatus.Cancelled]: 'Cancelled',
  [AppointmentStatus.NoShow]: 'No-show',
};
