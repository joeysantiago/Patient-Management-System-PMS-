import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { VisitReportItem } from './models/visit-report-item.model';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/reports`;

  searchVisits(from?: string, to?: string, patientName?: string): Observable<VisitReportItem[]> {
    const params = this.buildParams(from, to, patientName);
    return this.http.get<VisitReportItem[]>(`${this.baseUrl}/visits`, { params });
  }

  exportVisitsCsv(from?: string, to?: string, patientName?: string): Observable<Blob> {
    const params = this.buildParams(from, to, patientName);
    return this.http.get(`${this.baseUrl}/visits/export`, { params, responseType: 'blob' });
  }

  private buildParams(from?: string, to?: string, patientName?: string): HttpParams {
    let params = new HttpParams();
    if (from) {
      params = params.set('from', from);
    }
    if (to) {
      params = params.set('to', to);
    }
    if (patientName) {
      params = params.set('patientName', patientName);
    }

    return params;
  }
}
