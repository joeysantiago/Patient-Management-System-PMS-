import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';

import { PatientService } from '../../../core/patients/patient.service';
import { Patient } from '../../../core/patients/models/patient.model';
import { downloadBlob } from '../../../core/shared/download-file';

@Component({
  selector: 'app-patient-list',
  imports: [RouterLink, ReactiveFormsModule],
  templateUrl: './patient-list.html',
  styleUrl: './patient-list.scss',
})
export class PatientList implements OnInit, OnDestroy {
  private readonly patientService = inject(PatientService);
  private readonly destroyed = new Subject<void>();

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly patients = signal<Patient[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isExporting = signal(false);

  ngOnInit(): void {
    this.searchControl.valueChanges
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntil(this.destroyed))
      .subscribe((term) => this.load(term));

    this.load('');
  }

  ngOnDestroy(): void {
    this.destroyed.next();
    this.destroyed.complete();
  }

  exportCsv(): void {
    this.isExporting.set(true);
    this.errorMessage.set(null);

    this.patientService.exportListCsv(this.searchControl.value || undefined).subscribe({
      next: (blob) => {
        downloadBlob(blob, 'patients.csv');
        this.isExporting.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export patients right now. Please try again.');
        this.isExporting.set(false);
      },
    });
  }

  private load(search: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.patientService.list(search).subscribe({
      next: (patients) => {
        this.patients.set(patients);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load patients right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
