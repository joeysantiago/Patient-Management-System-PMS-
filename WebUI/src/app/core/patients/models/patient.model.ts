export interface Patient {
  id: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  phoneNumber: string;
  address: string | null;
  createdAtUtc: string;
}
