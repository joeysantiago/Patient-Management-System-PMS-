import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { PatientHistory as PatientHistoryModel } from '../../../core/patients/models/patient-history.model';
import { PatientService } from '../../../core/patients/patient.service';
import { downloadBlob } from '../../../core/shared/download-file';

@Component({
  selector: 'app-patient-history',
  imports: [RouterLink, ReactiveFormsModule, DatePipe],
  templateUrl: './patient-history.html',
  styleUrl: './patient-history.scss',
})
export class PatientHistory implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly patientService = inject(PatientService);

  protected readonly patientId = this.route.snapshot.paramMap.get('id')!;
  protected readonly history = signal<PatientHistoryModel | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isExporting = signal(false);
  protected readonly expandedVisitId = signal<string | null>(null);

  protected readonly filterForm = this.fb.nonNullable.group({
    from: [''],
    to: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  applyFilter(): void {
    this.load();
  }

  clearFilter(): void {
    this.filterForm.reset({ from: '', to: '' });
    this.load();
  }

  toggleExpand(visitId: string): void {
    this.expandedVisitId.set(this.expandedVisitId() === visitId ? null : visitId);
  }

  exportCsv(): void {
    this.isExporting.set(true);
    this.errorMessage.set(null);
    const { from, to } = this.filterForm.getRawValue();

    this.patientService.exportHistoryCsv(this.patientId, from || undefined, to || undefined).subscribe({
      next: (blob) => {
        downloadBlob(blob, `patient-${this.patientId}-history.csv`);
        this.isExporting.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export history right now. Please try again.');
        this.isExporting.set(false);
      },
    });
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const { from, to } = this.filterForm.getRawValue();

    this.patientService.getHistory(this.patientId, from || undefined, to || undefined).subscribe({
      next: (history) => {
        this.history.set(history);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load history right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
