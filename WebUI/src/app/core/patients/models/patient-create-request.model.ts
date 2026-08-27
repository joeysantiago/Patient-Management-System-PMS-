export interface PatientCreateRequest {
  fullName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber: string;
  address?: string | null;
}
