import React from "react";
import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

const NAV = [
  { to: "/dashboard", label: "Dashboard", roles: ["SuperAdmin", "Admin", "Employee", "Customer"] },
  { to: "/tenants", label: "Tenants", roles: ["SuperAdmin"] },
  { to: "/admins", label: "Admins", roles: ["SuperAdmin"] },
  { to: "/employees", label: "Employees", roles: ["Admin", "SuperAdmin"] },
  { to: "/services", label: "Services", roles: ["Employee"] },
  { to: "/services-customer", label: "Book Services", roles: ["Customer"] },
  { to: "/bookings", label: "Bookings", roles: ["Customer", "Employee"] },
];

export function Layout({ children }) {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  const items = NAV.filter(n => n.roles.includes(user.role));

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <div className="app">
      <div className="sidebar">
        <h2>Booking App</h2>
        {items.map(n => (
          <NavLink key={n.to} to={n.to} className={({ isActive }) => (isActive ? "active" : "")}>
            {n.label}
          </NavLink>
        ))}
        <button onClick={handleLogout} style={{ marginTop: 20, color: "#f87171" }}>Log out</button>
      </div>
      <div className="main">{children}</div>
    </div>
  );
}
