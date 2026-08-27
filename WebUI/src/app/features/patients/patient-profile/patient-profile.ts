import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { PatientHistory } from '../../../core/patients/models/patient-history.model';
import { Patient } from '../../../core/patients/models/patient.model';
import { PatientService } from '../../../core/patients/patient.service';
import { downloadBlob } from '../../../core/shared/download-file';

@Component({
  selector: 'app-patient-profile',
  imports: [RouterLink, DatePipe],
  templateUrl: './patient-profile.html',
  styleUrl: './patient-profile.scss',
})
export class PatientProfile implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly patientService = inject(PatientService);

  protected readonly patientId = this.route.snapshot.paramMap.get('id')!;
  protected readonly patient = signal<Patient | null>(null);
  protected readonly history = signal<PatientHistory | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isExportingPdf = signal(false);

  ngOnInit(): void {
    this.patientService.getById(this.patientId).subscribe({
      next: (patient) => {
        this.patient.set(patient);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load this patient right now. Please try again.');
        this.isLoading.set(false);
      },
    });

    this.patientService.getHistory(this.patientId).subscribe({
      next: (history) => this.history.set(history),
      error: () => {
        // History failing to load shouldn't block the rest of the profile.
      },
    });
  }

  exportPdf(): void {
    this.isExportingPdf.set(true);
    this.errorMessage.set(null);

    this.patientService.exportRecordPdf(this.patientId).subscribe({
      next: (blob) => {
        downloadBlob(blob, `patient-${this.patientId}-record.pdf`);
        this.isExportingPdf.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export this patient record right now. Please try again.');
        this.isExportingPdf.set(false);
      },
    });
  }
}
