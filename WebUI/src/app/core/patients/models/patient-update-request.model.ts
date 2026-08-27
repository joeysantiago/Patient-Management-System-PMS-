export interface PatientUpdateRequest {
  fullName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber: string;
  address?: string | null;
}
