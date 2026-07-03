// Point this at your real BookFiy API.
export const BASE_URL = "https://localhost:7202/api";

export async function apiFetch(path, token, options = {}) {
  const headers = { "Content-Type": "application/json", ...options.headers };
  if (token) headers["Authorization"] = `Bearer ${token}`;
  const res = await fetch(`${BASE_URL}${path}`, { ...options, headers });
  console.log("apiFetch", path, res.status, res.statusText,token);
  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new Error(body.message || body.title || (typeof body === "string" ? body : "Request failed"));
  }
  if (res.status === 204) return null;
  return res.json().catch(() => null);
}

export function getUserIdFromToken(token) {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    console.log("getUserIdFromToken", token, payload);
    return (
      payload["nameid"] ||
      payload["sub"] ||
      payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ||
      payload["userId"] ||
      payload["id"] ||
      null
    );
  } catch {
    console.error("getUserIdFromToken", token);
    return null;
  }
}

export function getTenantIdFromToken(token) {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    console.log("getTenantIdFromToken", token, payload);
    return (
      payload["tenantId"] ||
      payload["tenant"] ||
      payload["TenantId"] ||
      payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/tenantid"] ||
      null
    );
  } catch {
    console.error("getTenantIdFromToken", token);
    return null;
  }
}
