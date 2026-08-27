import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { ConsultationService } from '../../core/consultations/consultation.service';
import { Prescription } from '../../core/consultations/models/prescription.model';
import { downloadBlob } from '../../core/shared/download-file';

@Component({
  selector: 'app-prescription',
  imports: [RouterLink, DatePipe],
  templateUrl: './prescription.html',
  styleUrl: './prescription.scss',
})
export class PrescriptionPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly consultationService = inject(ConsultationService);

  protected readonly appointmentId = this.route.snapshot.paramMap.get('appointmentId')!;
  protected readonly prescription = signal<Prescription | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly isExportingPdf = signal(false);

  ngOnInit(): void {
    this.consultationService.getPrescription(this.appointmentId).subscribe({
      next: (prescription) => {
        this.prescription.set(prescription);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to load this prescription right now. Please try again.');
        this.isLoading.set(false);
      },
    });
  }

  print(): void {
    window.print();
  }

  exportPdf(): void {
    this.isExportingPdf.set(true);
    this.errorMessage.set(null);

    this.consultationService.exportPrescriptionPdf(this.appointmentId).subscribe({
      next: (blob) => {
        downloadBlob(blob, `prescription-${this.appointmentId}.pdf`);
        this.isExportingPdf.set(false);
      },
      error: () => {
        this.errorMessage.set('Unable to export this prescription right now. Please try again.');
        this.isExportingPdf.set(false);
      },
    });
  }
}
