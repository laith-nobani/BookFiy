import React, { createContext, useContext, useState } from "react";
import { apiFetch, getUserIdFromToken, getTenantIdFromToken } from "./api";


const AuthContext = createContext(null);
export const useAuth = () => useContext(AuthContext);

export function normalizeRole(role) {
  if (!role) return "";

  const normalized = String(role).trim().toLowerCase().replace(/[_\s-]+/g, "");

  if (["superadmin", "superadministrator", "super", "superuser"].includes(normalized)) return "SuperAdmin";
  if (["admin", "administrator"].includes(normalized)) return "Admin";
  if (["employee", "staff"].includes(normalized)) return "Employee";
  if (["user", "customer", "client"].includes(normalized)) return "Customer";

  return String(role).trim();
}

export function AuthProvider({ children }) {
  const [token, setToken] = useState(null);
  const [user, setUser] = useState(null); // { userName, fullName, role, tenantId }

  const login = async (email, password) => {
    const data = await apiFetch("/auth/login", null, {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    setToken(data.token);
    const userId = getUserIdFromToken(data.token) || "";
    localStorage.setItem("userId", userId);
    const tenantId = data.tenantId || getTenantIdFromToken(data.token) || "";
    const normalizedUser = {
      userId: userId,
      userName: data.username,
      fullName: data.fullName,
      role: normalizeRole(data.roleName || data.role),
      tenantId: tenantId,
    };
    setUser(normalizedUser);
    return normalizedUser;
  };

  const register = (form) =>
    apiFetch("/auth/register", null, {
      method: "POST",
      body: JSON.stringify(form),
    });

  const confirmRegister = async (form, code) => {
    await apiFetch("/auth/register/confirm", null, {
      method: "POST",
      body: JSON.stringify({ ...form, code }),
    });
    return await login(form.email, form.password);
  };

  const resendOtp = (email) =>
    apiFetch(`/auth/register/otp/resend?email=${encodeURIComponent(email)}`, null, { method: "POST" });

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem("userId");
  };

  return (
    <AuthContext.Provider value={{ token, user, login, register, confirmRegister, resendOtp, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
