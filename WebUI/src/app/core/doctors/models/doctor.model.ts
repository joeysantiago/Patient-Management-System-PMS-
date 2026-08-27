export interface Doctor {
  id: string;
  fullName: string;
  specialization: string;
  registrationNumber: string | null;
  phoneNumber: string;
  email: string | null;
  clinicAddress: string | null;
  createdAtUtc: string;
}
