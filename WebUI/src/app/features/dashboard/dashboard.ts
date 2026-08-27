import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { DashboardSummary } from '../../core/dashboard/models/dashboard-summary.model';
import { DashboardService } from '../../core/dashboard/dashboard.service';
import {
  APPOINTMENT_STATUS_LABELS,
  AppointmentStatus,
} from '../../core/appointments/models/appointment-status.model';

@Component({
  selector: 'app-dashboard',
  imports: [RouterLink, DatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class Dashboard implements OnInit {
  protected readonly auth = inject(Auth);
  private readonly dashboardService = inject(DashboardService);

  protected readonly statusLabels = APPOINTMENT_STATUS_LABELS;
  protected readonly AppointmentStatus = AppointmentStatus;

  protected readonly summary = signal<DashboardSummary | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.dashboardService.getSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load dashboard data right now.');
        this.isLoading.set(false);
      },
    });
  }
}
