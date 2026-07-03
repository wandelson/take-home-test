# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.1.6.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.

---

## Implementation (added)

Loan Management SPA — login, loan table, create loan, record payment.

### Prerequisites

```bash
npm install
```

### Local dev (with backend on :5000)

```bash
npm start
```

Open http://localhost:4200/login — credentials: **admin / admin123**

API calls use `/api` proxied to `http://localhost:5000` via `proxy.conf.json`.

### Tests

```bash
npm test -- --no-watch --browsers=ChromeHeadlessNoSandbox
```

7 unit tests (services + components).

### Docker

Production build served by nginx. Image: `frontend/Dockerfile`. See root [README.md](../README.md) for `docker compose up --build`.

### Structure

```
src/app/
  features/login/     # Sign-in
  features/loans/     # Table, create loan, payments
  services/           # HTTP clients (auth, loans)
  guards/             # Route protection
  interceptors/       # Credentials, logging, errors
  core/logging/       # LoggerService
```
