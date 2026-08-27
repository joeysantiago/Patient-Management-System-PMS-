import { Routes } from '@angular/router';

import { authGuard } from './core/auth/auth-guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'login',
    loadComponent: () => import('./features/login/login').then((m) => m.Login),
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell').then((m) => m.Shell),
    children: [
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'patients',
        children: [
          {
            path: '',
            pathMatch: 'full',
            loadComponent: () =>
              import('./features/patients/patient-list/patient-list').then((m) => m.PatientList),
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./features/patients/patient-form/patient-form').then((m) => m.PatientForm),
          },
          {
            path: ':id',
            pathMatch: 'full',
            loadComponent: () =>
              import('./features/patients/patient-profile/patient-profile').then(
                (m) => m.PatientProfile,
              ),
          },
          {
            path: ':id/edit',
            loadComponent: () =>
              import('./features/patients/patient-form/patient-form').then((m) => m.PatientForm),
          },
          {
            path: ':id/history',
            loadComponent: () =>
              import('./features/patients/patient-history/patient-history').then(
                (m) => m.PatientHistory,
              ),
          },
        ],
      },
      {
        path: 'appointments',
        children: [
          {
            path: '',
            pathMatch: 'full',
            loadComponent: () =>
              import('./features/appointments/appointment-list/appointment-list').then(
                (m) => m.AppointmentList,
              ),
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./features/appointments/appointment-form/appointment-form').then(
                (m) => m.AppointmentForm,
              ),
          },
          {
            path: ':id/edit',
            loadComponent: () =>
              import('./features/appointments/appointment-form/appointment-form').then(
                (m) => m.AppointmentForm,
              ),
          },
          {
            path: ':appointmentId/consultation',
            loadComponent: () =>
              import('./features/consultation/consultation').then((m) => m.Consultation),
          },
          {
            path: ':appointmentId/prescription',
            loadComponent: () =>
              import('./features/prescription/prescription').then((m) => m.PrescriptionPage),
          },
        ],
      },
      {
        path: 'doctors',
        children: [
          {
            path: '',
            pathMatch: 'full',
            loadComponent: () =>
              import('./features/doctors/doctor-list/doctor-list').then((m) => m.DoctorList),
          },
          {
            path: 'new',
            loadComponent: () =>
              import('./features/doctors/doctor-form/doctor-form').then((m) => m.DoctorForm),
          },
          {
            path: ':id/edit',
            loadComponent: () =>
              import('./features/doctors/doctor-form/doctor-form').then((m) => m.DoctorForm),
          },
        ],
      },
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/reports').then((m) => m.Reports),
      },
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings').then((m) => m.Settings),
      },
    ],
  },
  { path: '**', redirectTo: 'dashboard' },
];
