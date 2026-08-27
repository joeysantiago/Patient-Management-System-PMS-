import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ClinicSettingsUpdateRequest } from './models/clinic-settings-update-request.model';
import { ClinicSettings } from './models/clinic-settings.model';

@Injectable({
  providedIn: 'root',
})
export class SettingsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/settings`;

  getClinicSettings(): Observable<ClinicSettings> {
    return this.http.get<ClinicSettings>(`${this.baseUrl}/clinic`);
  }

  updateClinicSettings(request: ClinicSettingsUpdateRequest): Observable<ClinicSettings> {
    return this.http.put<ClinicSettings>(`${this.baseUrl}/clinic`, request);
  }
}
