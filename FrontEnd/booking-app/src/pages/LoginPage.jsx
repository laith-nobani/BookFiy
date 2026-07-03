import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useAuth } from "../AuthContext";

const EMPTY_GUID = "00000000-0000-0000-0000-000000000000";

export default function LoginPage() {
  const { login, register, confirmRegister, resendOtp } = useAuth();
  const navigate = useNavigate();
  const [mode, setMode] = useState("login"); // "login" | "register" | "confirm"
  const [error, setError] = useState("");
  const [info, setInfo] = useState("");

  const [loginForm, setLoginForm] = useState({ email: "", password: "" });

  // Matches the backend RegisterRequest DTO. TenantId defaults to the
  // all-zero guid since customers don't belong to a tenant.
  const [regForm, setRegForm] = useState({
    userName: "", email: "", password: "",
    firstName: "", lastName: "", phoneNumber: "",
    tenantId:("75C546DC-74C5-45EB-9563-75F9A0485C7B"),
  });
  const [code, setCode] = useState("");

  const submitLogin = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const loggedUser = await login(loginForm.email, loginForm.password);
      if (loggedUser && loggedUser.role === "Customer") {
        navigate("/services-customer");
      } else {
        navigate("/dashboard");
      }
    } catch (err) { setError(err.message); }
  };

  const submitRegister = async (e) => {
    e.preventDefault();
    setError("");
    try {
      await register(regForm);
      setInfo(`We sent a confirmation code to ${regForm.email}`);
      setMode("confirm");
    } catch (err) { setError(err.message); }
  };

  const submitConfirm = async (e) => {
    e.preventDefault();
    setError("");
    try {
      const loggedUser = await confirmRegister(regForm, code);
      if (loggedUser && loggedUser.role === "Customer") {
        navigate("/services-customer");
      } else {
        navigate("/dashboard");
      }
    } catch (err) { setError(err.message); }
  };

  const handleResend = async () => {
    setError(""); setInfo("");
    try { await resendOtp(regForm.email); setInfo("Code resent."); }
    catch (err) { setError(err.message); }
  };

  return (
    <div className="login-box card">
      <h2>{mode === "login" ? "Login" : mode === "register" ? "Create account" : "Enter confirmation code"}</h2>
      {error && <p style={{ color: "red" }}>{error}</p>}
      {info && !error && <p style={{ color: "#2563eb" }}>{info}</p>}

      {mode === "login" && (
        <form onSubmit={submitLogin}>
          <input placeholder="Email" value={loginForm.email}
            onChange={e => setLoginForm({ ...loginForm, email: e.target.value })} />
          <input placeholder="Password" type="password" value={loginForm.password}
            onChange={e => setLoginForm({ ...loginForm, password: e.target.value })} />
          <button className="btn" style={{ width: "100%", marginTop: 8 }}>Log in</button>
        </form>
      )}

      {mode === "register" && (
        <form onSubmit={submitRegister}>
          <input placeholder="First name" value={regForm.firstName}
            onChange={e => setRegForm({ ...regForm, firstName: e.target.value })} />
          <input placeholder="Last name" value={regForm.lastName}
            onChange={e => setRegForm({ ...regForm, lastName: e.target.value })} />
          <input placeholder="Username" value={regForm.userName}
            onChange={e => setRegForm({ ...regForm, userName: e.target.value })} />
          <input placeholder="Email" value={regForm.email}
            onChange={e => setRegForm({ ...regForm, email: e.target.value })} />
          <input placeholder="Phone number" value={regForm.phoneNumber}
            onChange={e => setRegForm({ ...regForm, phoneNumber: e.target.value })} />
          <input placeholder="Password" type="password" value={regForm.password}
            onChange={e => setRegForm({ ...regForm, password: e.target.value })} />
          <button className="btn" style={{ width: "100%", marginTop: 8 }}>Sign up</button>
        </form>
      )}

      {mode === "confirm" && (
        <form onSubmit={submitConfirm}>
          <input placeholder="Confirmation code" value={code} onChange={e => setCode(e.target.value)} />
          <button className="btn" style={{ width: "100%", marginTop: 8 }}>Confirm</button>
          <button type="button" onClick={handleResend}
            style={{ width: "100%", marginTop: 8, background: "none", border: "none", color: "#2563eb", cursor: "pointer" }}>
            Resend code
          </button>
        </form>
      )}

      <p style={{ marginTop: 12, fontSize: 14 }}>
        {mode === "login" ? (
          <>No account?{" "}
            <a href="#" onClick={e => { e.preventDefault(); setMode("register"); setError(""); setInfo(""); }}>Register</a>
          </>
        ) : (
          <>Have an account?{" "}
            <a href="#" onClick={e => { e.preventDefault(); setMode("login"); setError(""); setInfo(""); }}>Log in</a>
          </>
        )}
      </p>
    </div>
  );
}
