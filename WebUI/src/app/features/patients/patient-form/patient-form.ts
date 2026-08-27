import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { PatientService } from '../../../core/patients/patient.service';

@Component({
  selector: 'app-patient-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './patient-form.html',
  styleUrl: './patient-form.scss',
})
export class PatientForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly patientService = inject(PatientService);

  protected readonly patientId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = this.patientId !== null;

  protected readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    dateOfBirth: ['', Validators.required],
    gender: ['', Validators.required],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[+()\-.\s\d]{7,20}$/)]],
    address: [''],
  });

  protected readonly isLoadingPatient = signal(this.isEditMode);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.patientId) {
      return;
    }

    this.patientService.getById(this.patientId).subscribe({
      next: (patient) => {
        this.form.patchValue({
          fullName: patient.fullName,
          dateOfBirth: patient.dateOfBirth,
          gender: patient.gender,
          phoneNumber: patient.phoneNumber,
          address: patient.address ?? '',
        });
        this.isLoadingPatient.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load this patient right now. Please try again.');
        this.isLoadingPatient.set(false);
      },
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();
    const request = {
      fullName: value.fullName,
      dateOfBirth: value.dateOfBirth,
      gender: value.gender,
      phoneNumber: value.phoneNumber,
      address: value.address || null,
    };

    const onSuccess = (): void => {
      this.isSubmitting.set(false);
      this.router.navigateByUrl('/patients');
    };

    const onError = (err: HttpErrorResponse): void => {
      this.isSubmitting.set(false);
      this.errorMessage.set(
        err.status === 400
          ? 'Please check the highlighted fields and try again.'
          : 'Unable to save this patient right now. Please try again.',
      );
    };

    if (this.isEditMode && this.patientId) {
      this.patientService.update(this.patientId, request).subscribe({ next: onSuccess, error: onError });
    } else {
      this.patientService.create(request).subscribe({ next: onSuccess, error: onError });
    }
  }
}
