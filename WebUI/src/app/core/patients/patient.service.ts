import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PatientCreateRequest } from './models/patient-create-request.model';
import { PatientHistory } from './models/patient-history.model';
import { PatientUpdateRequest } from './models/patient-update-request.model';
import { Patient } from './models/patient.model';

@Injectable({
  providedIn: 'root',
})
export class PatientService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/patients`;

  list(search?: string): Observable<Patient[]> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get<Patient[]>(this.baseUrl, { params });
  }

  getById(id: string): Observable<Patient> {
    return this.http.get<Patient>(`${this.baseUrl}/${id}`);
  }

  create(request: PatientCreateRequest): Observable<Patient> {
    return this.http.post<Patient>(this.baseUrl, request);
  }

  update(id: string, request: PatientUpdateRequest): Observable<Patient> {
    return this.http.put<Patient>(`${this.baseUrl}/${id}`, request);
  }

  getHistory(patientId: string, from?: string, to?: string): Observable<PatientHistory> {
    let params = new HttpParams();
    if (from) {
      params = params.set('from', from);
    }
    if (to) {
      params = params.set('to', to);
    }

    return this.http.get<PatientHistory>(`${this.baseUrl}/${patientId}/history`, { params });
  }

  exportHistoryCsv(patientId: string, from?: string, to?: string): Observable<Blob> {
    let params = new HttpParams();
    if (from) {
      params = params.set('from', from);
    }
    if (to) {
      params = params.set('to', to);
    }

    return this.http.get(`${this.baseUrl}/${patientId}/history/export`, { params, responseType: 'blob' });
  }

  exportListCsv(search?: string): Observable<Blob> {
    let params = new HttpParams();
    if (search) {
      params = params.set('search', search);
    }

    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }

  exportRecordPdf(patientId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${patientId}/export/pdf`, { responseType: 'blob' });
  }
}
