import React from "react";
import { Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "./AuthContext";
import { ProtectedRoute } from "./ProtectedRoute";
import { Layout } from "./Layout";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import TenantsPage from "./pages/TenantsPage";
import AdminsPage from "./pages/AdminsPage";
import EmployeesPage from "./pages/EmployeesPage";
import ServicesPage from "./pages/ServicesPage";
import BookingsPage from "./pages/BookingsPage";
import CustomerServicesPage from "./pages/CustomerServicesPage";

function AppRoutes() {
  const { user } = useAuth();

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/" element={<Navigate to={user ? (user.role === "Customer" ? "/services-customer" : "/dashboard") : "/login"} replace />} />

      <Route path="/dashboard" element={
        <ProtectedRoute><Layout><DashboardPage /></Layout></ProtectedRoute>
      } />
      <Route path="/tenants" element={
        <ProtectedRoute allowedRoles={["SuperAdmin"]}><Layout><TenantsPage /></Layout></ProtectedRoute>
      } />
      <Route path="/admins" element={
        <ProtectedRoute allowedRoles={["SuperAdmin"]}><Layout><AdminsPage /></Layout></ProtectedRoute>
      } />
      <Route path="/employees" element={
        <ProtectedRoute allowedRoles={["Admin", "SuperAdmin"]}><Layout><EmployeesPage /></Layout></ProtectedRoute>
      } />
      <Route path="/services" element={
        <ProtectedRoute allowedRoles={["Employee"]}><Layout><ServicesPage /></Layout></ProtectedRoute>
      } />
      <Route path="/bookings" element={
        <ProtectedRoute><Layout><BookingsPage /></Layout></ProtectedRoute>
      } />
      <Route
        path="/services-customer"
        element={
          <ProtectedRoute allowedRoles={["Customer"]}>
            <Layout>
              <CustomerServicesPage />
            </Layout>
          </ProtectedRoute>
        }
      />

      <Route path="*" element={<p style={{ padding: 24 }}>Page not found.</p>} />
    </Routes>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
