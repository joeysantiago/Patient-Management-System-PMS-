import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

import { AppointmentService } from '../../../core/appointments/appointment.service';
import {
  APPOINTMENT_STATUS_LABELS,
  AppointmentStatus,
} from '../../../core/appointments/models/appointment-status.model';
import { Appointment } from '../../../core/appointments/models/appointment.model';
import { downloadBlob } from '../../../core/shared/download-file';

@Component({
  selector: 'app-appointment-list',
  imports: [RouterLink, DatePipe, ReactiveFormsModule],
  templateUrl: './appointment-list.html',
  styleUrl: './appointment-list.scss',
})
export class AppointmentList implements OnInit {
  private readonly appointmentService = inject(AppointmentService);
  private readonly fb = inject(FormBuilder);

  protected readonly appointments = signal<Appointment[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isExporting = signal(false);

  protected readonly AppointmentStatus = AppointmentStatus;
  protected readonly statusLabels = APPOINTMENT_STATUS_LABELS;

  protected readonly filterForm = this.fb.nonNullable.group({
    date: [''],
  });

  ngOnInit(): void {
    this.load();
  }

  applyFilter(): void {
    this.load();
  }

  clearFilter(): void {
    this.filterForm.reset({ date: '' });
    this.load();
  }

  exportCsv(): void {
    this.isExporting.set(true);
    this.errorMessage.set(null);
    const date = this.filterForm.getRawValue().date || undefined;

    this.appointmentService.exportCsv(date).subscribe({
      next: (blob) => {
        downloadBlob(blob, `appointments-${date ?? 'all'}.csv`);
        this.isExporting.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export appointments right now. Please try again.');
        this.isExporting.set(false);
      },
    });
  }

  protected statusClass(status: AppointmentStatus): string {
    return {
      [AppointmentStatus.Scheduled]: 'status-scheduled',
      [AppointmentStatus.Completed]: 'status-completed',
      [AppointmentStatus.Cancelled]: 'status-cancelled',
      [AppointmentStatus.NoShow]: 'status-noshow',
    }[status];
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    const date = this.filterForm.getRawValue().date || undefined;

    this.appointmentService.list(date).subscribe({
      next: (appointments) => {
        this.appointments.set(appointments);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load appointments right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }
}
