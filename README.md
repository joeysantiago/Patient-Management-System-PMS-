# Patient Management Application

Single-user, web-based Patient Management App for a general physician. See
[`tasks.txt`](tasks.txt) for the full phased task list derived from the BRD.

## Structure

```
WebUI/    Angular 21 single-page app
WebAPI/   .NET 10 Web API (PatientManagement.Api)
```

## Current status

Core Phase 1 flows are in place: patient/appointment/doctor management,
consultation with vitals/complaints/diagnosis/medications, printable and
PDF-exportable prescriptions, patient history, CSV/PDF export, a reports page,
dashboard stats, global search, clinic settings, change password, automated
backups, and database encryption at rest. See `tasks.txt` for the full phased
task list; remaining out-of-scope items are noted there.

## Running locally

### WebAPI

```
cd WebAPI
dotnet run --project src/PatientManagement.Api --urls http://localhost:5050
```

Swagger/OpenAPI is available at `/openapi/v1.json` in Development.

Default doctor login (change before any real use):

- Username: `doctor`
- Password: `Doctor@123`

The initial password hash and JWT settings live in
`WebAPI/src/PatientManagement.Api/appsettings.json`. On first run, that
`DoctorAccount` config seeds a row in the database, which becomes the source
of truth from then on (e.g. after using the in-app "Change Password" screen
under Settings). For anything beyond local development, move `Jwt:Secret`
and `Sqlite:EncryptionKey` into user secrets / environment variables / a
secrets manager instead of committing them — the app throws on startup if
either is missing.

The SQLite database is encrypted at rest via SQLCipher
(`Sqlite:EncryptionKey`). This means the raw `.db` file cannot be opened by a
plain SQLite tool without that key. Changing the key requires recreating the
database (there's no in-place rekey step here) — back up first if that
matters.

### WebUI

```
cd WebUI
npm start
```

Serves on `http://localhost:4200` and points at the API via
`src/environments/environment.development.ts` (`http://localhost:5050/api`).

## Notes

- Auth is JWT-based; the token is stored in `localStorage` and attached to
  outgoing requests by an HTTP interceptor.
- Since Phase 1 is single-user, there is one `DoctorAccount` row (seeded from
  `appsettings.json` on first run, see above) rather than a full user table.
- A fixed-window rate limiter (5 requests/minute) guards `POST /api/auth/login`.
- Database backups run automatically on a schedule (`Backup:IntervalHours`,
  default 24h) and are written to `WebAPI/src/PatientManagement.Api/Backups/`;
  a manual backup can also be triggered from the Settings page. Restoring is
  a manual step (stop the app, copy a backup file back over the live `.db`).
