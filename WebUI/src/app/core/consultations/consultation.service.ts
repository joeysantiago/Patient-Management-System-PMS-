import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { Prescription } from './models/prescription.model';
import { VisitCreateRequest } from './models/visit-create-request.model';
import { Visit } from './models/visit.model';

@Injectable({
  providedIn: 'root',
})
export class ConsultationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/consultations`;

  getByAppointmentId(appointmentId: string): Observable<Visit> {
    return this.http.get<Visit>(`${this.baseUrl}/by-appointment/${appointmentId}`);
  }

  save(request: VisitCreateRequest): Observable<Visit> {
    return this.http.post<Visit>(this.baseUrl, request);
  }

  getPrescription(appointmentId: string): Observable<Prescription> {
    return this.http.get<Prescription>(`${this.baseUrl}/by-appointment/${appointmentId}/prescription`);
  }

  exportPrescriptionPdf(appointmentId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/by-appointment/${appointmentId}/prescription/pdf`, {
      responseType: 'blob',
    });
  }
}
