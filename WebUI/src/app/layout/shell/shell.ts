import { Component, OnDestroy, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';

import { Auth } from '../../core/auth/auth';
import { Patient } from '../../core/patients/models/patient.model';
import { PatientService } from '../../core/patients/patient.service';

const MAX_SEARCH_RESULTS = 5;

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet, ReactiveFormsModule],
  templateUrl: './shell.html',
  styleUrl: './shell.scss',
})
export class Shell implements OnDestroy {
  protected readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly patientService = inject(PatientService);
  private readonly destroyed = new Subject<void>();

  protected readonly searchControl = new FormControl('', { nonNullable: true });
  protected readonly searchResults = signal<Patient[]>([]);
  protected readonly showResults = signal(false);

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((term) => {
          if (!term.trim()) {
            this.showResults.set(false);
            return [];
          }
          return this.patientService.list(term);
        }),
        takeUntil(this.destroyed),
      )
      .subscribe((results) => {
        this.searchResults.set(results.slice(0, MAX_SEARCH_RESULTS));
        this.showResults.set(true);
      });
  }

  ngOnDestroy(): void {
    this.destroyed.next();
    this.destroyed.complete();
  }

  selectPatient(patient: Patient): void {
    this.showResults.set(false);
    this.searchControl.setValue('', { emitEvent: false });
    this.router.navigate(['/patients', patient.id]);
  }

  closeResults(): void {
    // Delay so a click on a result fires before the dropdown is hidden by blur.
    setTimeout(() => this.showResults.set(false), 150);
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }
}
