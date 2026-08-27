import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { DoctorService } from '../../../core/doctors/doctor.service';

@Component({
  selector: 'app-doctor-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './doctor-form.html',
  styleUrl: './doctor-form.scss',
})
export class DoctorForm implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly doctorService = inject(DoctorService);

  protected readonly doctorId = this.route.snapshot.paramMap.get('id');
  protected readonly isEditMode = this.doctorId !== null;

  protected readonly form = this.fb.nonNullable.group({
    fullName: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(200)]],
    specialization: ['', [Validators.required, Validators.maxLength(150)]],
    registrationNumber: [''],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[+()\-.\s\d]{7,20}$/)]],
    email: ['', Validators.email],
    clinicAddress: [''],
  });

  protected readonly isLoadingDoctor = signal(this.isEditMode);
  protected readonly isSubmitting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    if (!this.doctorId) {
      return;
    }

    this.doctorService.getById(this.doctorId).subscribe({
      next: (doctor) => {
        this.form.patchValue({
          fullName: doctor.fullName,
          specialization: doctor.specialization,
          registrationNumber: doctor.registrationNumber ?? '',
          phoneNumber: doctor.phoneNumber,
          email: doctor.email ?? '',
          clinicAddress: doctor.clinicAddress ?? '',
        });
        this.isLoadingDoctor.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load this doctor right now. Please try again.');
        this.isLoadingDoctor.set(false);
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
      specialization: value.specialization,
      registrationNumber: value.registrationNumber || null,
      phoneNumber: value.phoneNumber,
      email: value.email || null,
      clinicAddress: value.clinicAddress || null,
    };

    const onSuccess = (): void => {
      this.isSubmitting.set(false);
      this.router.navigateByUrl('/doctors');
    };

    const onError = (err: HttpErrorResponse): void => {
      this.isSubmitting.set(false);
      this.errorMessage.set(
        err.status === 400
          ? 'Please check the highlighted fields and try again.'
          : 'Unable to save this doctor right now. Please try again.',
      );
    };

    if (this.isEditMode && this.doctorId) {
      this.doctorService.update(this.doctorId, request).subscribe({ next: onSuccess, error: onError });
    } else {
      this.doctorService.create(request).subscribe({ next: onSuccess, error: onError });
    }
  }
}
