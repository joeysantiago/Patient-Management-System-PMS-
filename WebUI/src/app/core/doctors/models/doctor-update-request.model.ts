export interface DoctorUpdateRequest {
  fullName: string;
  specialization: string;
  registrationNumber?: string | null;
  phoneNumber: string;
  email?: string | null;
  clinicAddress?: string | null;
}
