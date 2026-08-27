import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';

import { downloadBlob } from '../../core/shared/download-file';
import { VisitReportItem } from '../../core/reports/models/visit-report-item.model';
import { ReportService } from '../../core/reports/report.service';

@Component({
  selector: 'app-reports',
  imports: [ReactiveFormsModule, DatePipe],
  templateUrl: './reports.html',
  styleUrl: './reports.scss',
})
export class Reports implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly reportService = inject(ReportService);

  protected readonly results = signal<VisitReportItem[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isExporting = signal(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly hasSearched = signal(false);

  protected readonly filterForm = this.fb.nonNullable.group({
    from: [''],
    to: [''],
    patientName: [''],
  });

  ngOnInit(): void {
    this.search();
  }

  search(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const { from, to, patientName } = this.filterForm.getRawValue();

    this.reportService.searchVisits(from || undefined, to || undefined, patientName || undefined).subscribe({
      next: (results) => {
        this.results.set(results);
        this.isLoading.set(false);
        this.hasSearched.set(true);
      },
      error: () => {
        this.errorMessage.set('Unable to load report data right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  clearFilter(): void {
    this.filterForm.reset({ from: '', to: '', patientName: '' });
    this.search();
  }

  exportCsv(): void {
    this.isExporting.set(true);
    this.errorMessage.set(null);
    const { from, to, patientName } = this.filterForm.getRawValue();

    this.reportService.exportVisitsCsv(from || undefined, to || undefined, patientName || undefined).subscribe({
      next: (blob) => {
        downloadBlob(blob, 'visit-report.csv');
        this.isExporting.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export the report right now. Please try again.');
        this.isExporting.set(false);
      },
    });
  }
}
