export interface ClinicSettingsUpdateRequest {
  clinicName: string;
  doctorName: string;
  registrationNumber?: string | null;
  qualification?: string | null;
  phone?: string | null;
  address?: string | null;
  footerNote?: string | null;
}
