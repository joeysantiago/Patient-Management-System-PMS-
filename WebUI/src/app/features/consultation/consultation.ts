import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AppointmentService } from '../../core/appointments/appointment.service';
import { Appointment } from '../../core/appointments/models/appointment.model';
import { ConsultationService } from '../../core/consultations/consultation.service';

const BP_SYSTOLIC_LOW = 90;
const BP_SYSTOLIC_HIGH = 140;
const BP_DIASTOLIC_LOW = 60;
const BP_DIASTOLIC_HIGH = 90;

type MedicationFormGroup = FormGroup<{
  name: FormControl<string>;
  dosage: FormControl<string>;
  frequency: FormControl<string>;
  duration: FormControl<string>;
  instructions: FormControl<string>;
}>;

@Component({
  selector: 'app-consultation',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './consultation.html',
  styleUrl: './consultation.scss',
})
export class Consultation implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly appointmentService = inject(AppointmentService);
  private readonly consultationService = inject(ConsultationService);

  protected readonly appointmentId = this.route.snapshot.paramMap.get('appointmentId')!;
  protected readonly appointment = signal<Appointment | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly bpWarning = signal<string | null>(null);

  protected readonly form = this.fb.nonNullable.group({
    temperatureCelsius: [0, [Validators.required, Validators.min(30), Validators.max(45)]],
    bloodPressureSystolic: [0, [Validators.required, Validators.min(1)]],
    bloodPressureDiastolic: [0, [Validators.required, Validators.min(1)]],
    pulseBpm: [0, [Validators.required, Validators.min(1)]],
    complaints: ['', Validators.required],
    diagnosis: ['', Validators.required],
    medications: this.fb.array<MedicationFormGroup>([]),
  });

  get medications() {
    return this.form.controls.medications;
  }

  ngOnInit(): void {
    this.appointmentService.getById(this.appointmentId).subscribe({
      next: (appointment) => {
        this.appointment.set(appointment);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load this appointment right now. Please try again.');
        this.isLoading.set(false);
      },
    });

    this.consultationService.getByAppointmentId(this.appointmentId).subscribe({
      next: (visit) => {
        this.form.patchValue({
          temperatureCelsius: visit.vitals.temperatureCelsius,
          bloodPressureSystolic: visit.vitals.bloodPressureSystolic,
          bloodPressureDiastolic: visit.vitals.bloodPressureDiastolic,
          pulseBpm: visit.vitals.pulseBpm,
          complaints: visit.complaints,
          diagnosis: visit.diagnosis,
        });
        visit.medications.forEach((medication) => this.addMedication(medication));
        this.checkBpRange();
      },
      error: () => {
        // 404 means no consultation has been recorded for this appointment yet — the normal first-time case.
      },
    });
  }

  addMedication(initial?: {
    name: string;
    dosage: string;
    frequency: string;
    duration: string;
    instructions?: string | null;
  }): void {
    this.medications.push(
      this.fb.nonNullable.group({
        name: [initial?.name ?? '', Validators.required],
        dosage: [initial?.dosage ?? '', Validators.required],
        frequency: [initial?.frequency ?? '', Validators.required],
        duration: [initial?.duration ?? '', Validators.required],
        instructions: [initial?.instructions ?? ''],
      }),
    );
  }

  removeMedication(index: number): void {
    this.medications.removeAt(index);
  }

  checkBpRange(): void {
    const value = this.form.getRawValue();
    const outOfRange =
      value.bloodPressureSystolic < BP_SYSTOLIC_LOW ||
      value.bloodPressureSystolic > BP_SYSTOLIC_HIGH ||
      value.bloodPressureDiastolic < BP_DIASTOLIC_LOW ||
      value.bloodPressureDiastolic > BP_DIASTOLIC_HIGH;

    this.bpWarning.set(
      outOfRange ? 'Blood pressure looks outside the typical 90-140/60-90 mmHg range — please double-check.' : null,
    );
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();

    this.consultationService
      .save({
        appointmentId: this.appointmentId,
        vitals: {
          temperatureCelsius: value.temperatureCelsius,
          bloodPressureSystolic: value.bloodPressureSystolic,
          bloodPressureDiastolic: value.bloodPressureDiastolic,
          pulseBpm: value.pulseBpm,
        },
        complaints: value.complaints,
        diagnosis: value.diagnosis,
        medications: value.medications.map((medication) => ({
          ...medication,
          instructions: medication.instructions || null,
        })),
      })
      .subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.router.navigateByUrl(`/appointments/${this.appointmentId}/prescription`);
        },
        error: (err: HttpErrorResponse) => {
          this.isSubmitting.set(false);
          this.errorMessage.set(
            err.status === 400 || err.status === 404
              ? (err.error?.message ?? 'This appointment cannot be consulted.')
              : 'Unable to save this consultation right now. Please try again.',
          );
        },
      });
  }
}
