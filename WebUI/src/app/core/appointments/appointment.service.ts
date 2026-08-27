import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AppointmentCreateRequest } from './models/appointment-create-request.model';
import { AppointmentUpdateRequest } from './models/appointment-update-request.model';
import { Appointment } from './models/appointment.model';

@Injectable({
  providedIn: 'root',
})
export class AppointmentService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/appointments`;

  list(date?: string): Observable<Appointment[]> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }

    return this.http.get<Appointment[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Appointment> {
    return this.http.get<Appointment>(`${this.baseUrl}/${id}`);
  }

  create(request: AppointmentCreateRequest): Observable<Appointment> {
    return this.http.post<Appointment>(this.baseUrl, request);
  }

  update(id: string, request: AppointmentUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, request);
  }

  exportCsv(date?: string): Observable<Blob> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }

    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }
}
