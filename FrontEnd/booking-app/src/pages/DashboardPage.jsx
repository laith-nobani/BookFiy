import React from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../AuthContext";

export default function DashboardPage() {
  const { user } = useAuth();
  const isCustomer = user.role === "Customer";

  return (
    <div>
      <h1>Dashboard</h1>
      <div className="card">
        <h2>Welcome, {user.fullName}!</h2>
        <p>Role: <span className="badge">{user.role}</span></p>
        <p>Username: {user.userName}</p>
        {user.tenantId && <p>Tenant ID: {user.tenantId}</p>}

        {isCustomer && (
          <div style={{ marginTop: 24, padding: 16, backgroundColor: "#f8fafc", border: "1px solid #e2e8f0", borderRadius: 8 }}>
            <h3>Ready to Book?</h3>
            <p style={{ margin: "8px 0 16px 0", color: "#475569" }}>
              Explore our services and schedule your appointments easily.
            </p>
            <Link to="/services-customer" className="btn" style={{ display: "inline-block", textDecoration: "none" }}>
              Book Services
            </Link>
          </div>
        )}
      </div>
    </div>
  );
}
