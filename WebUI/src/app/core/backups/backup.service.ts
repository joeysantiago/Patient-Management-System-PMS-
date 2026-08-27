import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { BackupFile } from './models/backup-file.model';

@Injectable({
  providedIn: 'root',
})
export class BackupService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiBaseUrl}/backups`;

  list(): Observable<BackupFile[]> {
    return this.http.get<BackupFile[]>(this.baseUrl);
  }

  runNow(): Observable<BackupFile> {
    return this.http.post<BackupFile>(`${this.baseUrl}/run`, {});
  }
}
