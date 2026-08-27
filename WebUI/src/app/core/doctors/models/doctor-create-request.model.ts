export interface DoctorCreateRequest {
  fullName: string;
  specialization: string;
  registrationNumber?: string | null;
  phoneNumber: string;
  email?: string | null;
  clinicAddress?: string | null;
}
