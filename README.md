# Patient Management Application

Single-user, web-based Patient Management App for a general physician. See
[`tasks.txt`](tasks.txt) for the full phased task list derived from the BRD.

## Structure

```
WebUI/    Angular 21 single-page app
WebAPI/   .NET 10 Web API (PatientManagement.Api)
```

## Current status

Phase 0 (scaffolding) and the login slice of Phase 1 (authentication) are in
place. Everything else in `tasks.txt` is still pending.

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

The password hash and JWT settings live in
`WebAPI/src/PatientManagement.Api/appsettings.json`. For anything beyond local
development, move `Jwt:Secret` and `DoctorAccount:PasswordHash` into user
secrets / environment variables / a secrets manager instead of committing
them.

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
- Since Phase 1 is single-user, there is no user table — the one doctor
  account is configured in `appsettings.json`.
- A fixed-window rate limiter (5 requests/minute) guards `POST /api/auth/login`.
