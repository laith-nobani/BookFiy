# Booking App (React + Vite)

Plain JS React app (no TypeScript, no UI library) that talks to the BookFiy API.

## Setup

```bash
npm install
```

Then edit `src/api.js` and set `BASE_URL` to your real API address:

```js
export const BASE_URL = "https://localhost:7202/api";
```

## Run (dev server, hot reload)

```bash
npm run dev
```

Opens at http://localhost:5173

## Build for production

```bash
npm run build
npm run preview   # serves the built dist/ folder locally to test it
```

## Structure

```
src/
  api.js              # BASE_URL + apiFetch + JWT helper
  AuthContext.jsx      # login / register / confirm-register / logout
  ProtectedRoute.jsx   # redirects to /login if not authenticated
  Layout.jsx            # sidebar (role-based nav) + page shell
  App.jsx                # routes
  main.jsx               # entry point
  pages/
    LoginPage.jsx        # login + 2-step OTP registration
    DashboardPage.jsx
    TenantsPage.jsx       # SuperAdmin
    AdminsPage.jsx         # SuperAdmin
    EmployeesPage.jsx       # Admin & SuperAdmin
    ServicesPage.jsx         # Employee
    BookingsPage.jsx          # everyone (behavior differs by role)
    ResourcePage.jsx           # small reusable list+create component
```

## Notes / assumptions to double check against your backend

- `CreateBookingDto` wasn't provided when this was built, so the booking
  create payload is a guess (`serviceId`, `employeeId`, `tenantId`,
  `startTime`, `notes`). Check `book()` in `BookingsPage.jsx`.
- The booking update (`PUT /bookings/{id}`) route wasn't shown on the
  controller you shared, only `UpdateBookingDto`. Confirm that route exists
  on the backend, or adjust `saveEdit()` in `BookingsPage.jsx`.
- Customers can't call `GET /services` (Employee-only), so there's no
  in-app service browser yet - the booking form takes a raw Service ID.
- The customer's own user ID isn't in the login response, so it's read
  from common JWT claim names client-side (`getUserIdFromToken` in
  `api.js`). If that doesn't match your token, there's a manual fallback
  input on the Bookings page.
