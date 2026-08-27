import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DoctorService } from '../../../core/doctors/doctor.service';
import { Doctor } from '../../../core/doctors/models/doctor.model';

@Component({
  selector: 'app-doctor-list',
  imports: [RouterLink],
  templateUrl: './doctor-list.html',
  styleUrl: './doctor-list.scss',
})
export class DoctorList implements OnInit {
  private readonly doctorService = inject(DoctorService);

  protected readonly doctors = signal<Doctor[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  deleteDoctor(doctor: Doctor): void {
    if (!confirm(`Remove Dr. ${doctor.fullName} from the doctor list?`)) {
      return;
    }

    this.doctorService.delete(doctor.id).subscribe({
      next: () => this.load(),
      error: () => {
        this.errorMessage.set('Unable to delete this doctor right now. Please try again.');
      },
    });
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.doctorService.list().subscribe({
      next: (doctors) => {
        this.doctors.set(doctors);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load doctors right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
