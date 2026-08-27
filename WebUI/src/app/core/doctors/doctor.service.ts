import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { DoctorCreateRequest } from './models/doctor-create-request.model';
import { DoctorUpdateRequest } from './models/doctor-update-request.model';
import { Doctor } from './models/doctor.model';

@Injectable({
  providedIn: 'root',
})
export class DoctorService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/doctors`;

  list(): Observable<Doctor[]> {
    return this.http.get<Doctor[]>(this.baseUrl);
  }

  getById(id: string): Observable<Doctor> {
    return this.http.get<Doctor>(`${this.baseUrl}/${id}`);
  }

  create(request: DoctorCreateRequest): Observable<Doctor> {
    return this.http.post<Doctor>(this.baseUrl, request);
  }

  update(id: string, request: DoctorUpdateRequest): Observable<Doctor> {
    return this.http.put<Doctor>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
