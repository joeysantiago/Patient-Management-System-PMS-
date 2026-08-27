import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AppointmentService } from '../../../core/appointments/appointment.service';
import {
  APPOINTMENT_STATUS_LABELS,
  AppointmentStatus,
} from '../../../core/appointments/models/appointment-status.model';
import { PatientService } from '../../../core/patients/patient.service';
import { Patient } from '../../../core/patients/models/patient.model';

@Component({
  selector: 'app-appointment-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './appointment-form.html',
  styleUrl: './appointment-form.scss',
})
export class AppointmentForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly appointmentService = inject(AppointmentService);
  private readonly patientService = inject(PatientService);

  protected readonly appointmentId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = this.appointmentId !== null;

  protected readonly statusOptions = Object.entries(APPOINTMENT_STATUS_LABELS).map(([value, label]) => ({
    value: Number(value) as AppointmentStatus,
    label,
  }));

  protected readonly form = this.fb.nonNullable.group({
    patientId: ['', Validators.required],
    scheduledAt: ['', Validators.required],
    status: [AppointmentStatus.Scheduled],
    notes: [''],
  });

  protected readonly patients = signal<Patient[]>([]);
  protected readonly isLoadingPatients = signal(true);
  protected readonly isLoadingAppointment = signal(this.isEditMode);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.patientService.list().subscribe({
      next: (patients) => {
        this.patients.set(patients);
        this.isLoadingPatients.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load patients right now. Please try again.');
        this.isLoadingPatients.set(false);
      },
    });

    if (this.appointmentId) {
      this.appointmentService.getById(this.appointmentId).subscribe({
        next: (appointment) => {
          this.form.patchValue({
            patientId: appointment.patientId,
            scheduledAt: appointment.scheduledAt.slice(0, 16),
            status: appointment.status,
            notes: appointment.notes ?? '',
          });
          this.isLoadingAppointment.set(false);
        },
        error: () => {
          this.errorMessage.set('Unable to load this appointment right now. Please try again.');
          this.isLoadingAppointment.set(false);
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();

    const onSuccess = (): void => {
      this.isSubmitting.set(false);
      this.router.navigateByUrl('/appointments');
    };

    const onError = (err: HttpErrorResponse): void => {
      this.isSubmitting.set(false);
      if (err.status === 409) {
        this.errorMessage.set(err.error?.message ?? 'This patient already has a scheduled appointment.');
      } else if (err.status === 400) {
        this.errorMessage.set('Please check the highlighted fields and try again.');
      } else {
        this.errorMessage.set('Unable to save this appointment right now. Please try again.');
      }
    };

    if (this.isEditMode && this.appointmentId) {
      this.appointmentService
        .update(this.appointmentId, {
          patientId: value.patientId,
          scheduledAt: value.scheduledAt,
          status: value.status,
          notes: value.notes || null,
        })
        .subscribe({ next: onSuccess, error: onError });
    } else {
      this.appointmentService
        .create({
          patientId: value.patientId,
          scheduledAt: value.scheduledAt,
          notes: value.notes || null,
        })
        .subscribe({ next: onSuccess, error: onError });
    }
  }
}
