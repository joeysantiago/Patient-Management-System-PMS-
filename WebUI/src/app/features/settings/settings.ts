import { DatePipe, DecimalPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { Auth } from '../../core/auth/auth';
import { BackupService } from '../../core/backups/backup.service';
import { BackupFile } from '../../core/backups/models/backup-file.model';
import { SettingsService } from '../../core/settings/settings.service';

@Component({
  selector: 'app-settings',
  imports: [ReactiveFormsModule, DatePipe, DecimalPipe],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
})
export class Settings implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly settingsService = inject(SettingsService);
  private readonly auth = inject(Auth);
  private readonly backupService = inject(BackupService);

  protected readonly backups = signal<BackupFile[]>([]);
  protected readonly isLoadingBackups = signal(true);
  protected readonly isRunningBackup = signal(false);
  protected readonly backupErrorMessage = signal<string | null>(null);

  protected readonly isLoadingClinic = signal(true);
  protected readonly isSavingClinic = signal(false);
  protected readonly clinicErrorMessage = signal<string | null>(null);
  protected readonly clinicSuccessMessage = signal<string | null>(null);

  protected readonly isChangingPassword = signal(false);
  protected readonly passwordErrorMessage = signal<string | null>(null);
  protected readonly passwordSuccessMessage = signal<string | null>(null);

  protected readonly passwordForm = this.fb.nonNullable.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(8)]],
    confirmPassword: ['', Validators.required],
  });

  protected readonly clinicForm = this.fb.nonNullable.group({
    clinicName: ['', Validators.required],
    doctorName: ['', Validators.required],
    registrationNumber: [''],
    qualification: [''],
    phone: [''],
    address: [''],
    footerNote: [''],
  });

  ngOnInit(): void {
    this.settingsService.getClinicSettings().subscribe({
      next: (settings) => {
        this.clinicForm.patchValue({
          clinicName: settings.clinicName,
          doctorName: settings.doctorName,
          registrationNumber: settings.registrationNumber ?? '',
          qualification: settings.qualification ?? '',
          phone: settings.phone ?? '',
          address: settings.address ?? '',
          footerNote: settings.footerNote ?? '',
        });
        this.isLoadingClinic.set(false);
      },
      error: () => {
        this.clinicErrorMessage.set('Unable to load clinic settings right now. Please try again.');
        this.isLoadingClinic.set(false);
      },
    });

    this.loadBackups();
  }

  runBackupNow(): void {
    this.isRunningBackup.set(true);
    this.backupErrorMessage.set(null);

    this.backupService.runNow().subscribe({
      next: () => {
        this.isRunningBackup.set(false);
        this.loadBackups();
      },
      error: () => {
        this.isRunningBackup.set(false);
        this.backupErrorMessage.set('Unable to run a backup right now. Please try again.');
      },
    });
  }

  private loadBackups(): void {
    this.isLoadingBackups.set(true);

    this.backupService.list().subscribe({
      next: (backups) => {
        this.backups.set(backups);
        this.isLoadingBackups.set(false);
      },
      error: () => {
        this.backupErrorMessage.set('Unable to load backups right now. Please try again.');
        this.isLoadingBackups.set(false);
      },
    });
  }

  saveClinicSettings(): void {
    if (this.clinicForm.invalid || this.isSavingClinic()) {
      this.clinicForm.markAllAsTouched();
      return;
    }

    this.isSavingClinic.set(true);
    this.clinicErrorMessage.set(null);
    this.clinicSuccessMessage.set(null);

    const value = this.clinicForm.getRawValue();
    this.settingsService
      .updateClinicSettings({
        clinicName: value.clinicName,
        doctorName: value.doctorName,
        registrationNumber: value.registrationNumber || null,
        qualification: value.qualification || null,
        phone: value.phone || null,
        address: value.address || null,
        footerNote: value.footerNote || null,
      })
      .subscribe({
        next: () => {
          this.isSavingClinic.set(false);
          this.clinicSuccessMessage.set('Clinic settings saved.');
        },
        error: (err: HttpErrorResponse) => {
          this.isSavingClinic.set(false);
          this.clinicErrorMessage.set(
            err.status === 400
              ? 'Please check the highlighted fields and try again.'
              : 'Unable to save clinic settings right now. Please try again.',
          );
        },
      });
  }

  changePassword(): void {
    if (this.passwordForm.invalid || this.isChangingPassword()) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword, confirmPassword } = this.passwordForm.getRawValue();
    if (newPassword !== confirmPassword) {
      this.passwordErrorMessage.set('New password and confirmation do not match.');
      return;
    }

    this.isChangingPassword.set(true);
    this.passwordErrorMessage.set(null);
    this.passwordSuccessMessage.set(null);

    this.auth.changePassword({ currentPassword, newPassword }).subscribe({
      next: () => {
        this.isChangingPassword.set(false);
        this.passwordSuccessMessage.set('Password changed successfully.');
        this.passwordForm.reset();
      },
      error: (err: HttpErrorResponse) => {
        this.isChangingPassword.set(false);
        this.passwordErrorMessage.set(
          err.status === 400
            ? (err.error?.message ?? 'The current password is incorrect.')
            : 'Unable to change the password right now. Please try again.',
        );
      },
    });
  }
}
