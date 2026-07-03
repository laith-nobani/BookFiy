import React, { useEffect, useState } from "react";
import { apiFetch } from "../api";
import { useAuth } from "../AuthContext";

// TenantId is not sent by the client - EmployeeController reads it from the
// caller's "tenant_id" JWT claim server-side, so create/update forms only
// need the fields the admin actually fills in.
export default function EmployeesPage() {
  const { token } = useAuth();
  const [employees, setEmployees] = useState([]);
  const [loading, setLoading] = useState(true);

  const blank = { firstName: "", lastName: "", jobTitle: "", bio: "", email: "", phoneNumber: "" };
  const [createForm, setCreateForm] = useState(blank);

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(blank);

  const load = () => {
    setLoading(true);
    apiFetch("/Employee", token).then(setEmployees).catch(() => setEmployees([])).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const create = async (e) => {
    e.preventDefault();
    await apiFetch("/Employee", token, { method: "POST", body: JSON.stringify(createForm) });
    setCreateForm(blank);
    load();
  };

  const startEdit = (emp) => {
    setEditingId(emp.id);
    setEditForm({
      firstName: emp.firstName, lastName: emp.lastName, jobTitle: emp.jobTitle,
      bio: emp.bio, email: emp.email, phoneNumber: emp.phoneNumber,
    });
  };

  const saveEdit = async (e) => {
    e.preventDefault();
    // UpdateEmployeeDto also carries Id in the body, alongside the route id.
    await apiFetch(`/Employee/${editingId}`, token, {
      method: "PUT",
      body: JSON.stringify({ id: editingId, ...editForm }),
    });
    setEditingId(null);
    load();
  };

  const remove = async (id) => {
    await apiFetch(`/Employee/${id}`, token, { method: "DELETE" });
    load();
  };

  return (
    <div>
      <h1>Employees</h1>

      <div className="card">
        <h3>Add Employee</h3>
        <form onSubmit={create}>
          <input placeholder="First name" value={createForm.firstName}
            onChange={e => setCreateForm({ ...createForm, firstName: e.target.value })} />
          <input placeholder="Last name" value={createForm.lastName}
            onChange={e => setCreateForm({ ...createForm, lastName: e.target.value })} />
          <input placeholder="Job title" value={createForm.jobTitle}
            onChange={e => setCreateForm({ ...createForm, jobTitle: e.target.value })} />
          <input placeholder="Bio" value={createForm.bio}
            onChange={e => setCreateForm({ ...createForm, bio: e.target.value })} />
          <input placeholder="Email" value={createForm.email}
            onChange={e => setCreateForm({ ...createForm, email: e.target.value })} />
          <input placeholder="Phone number" value={createForm.phoneNumber}
            onChange={e => setCreateForm({ ...createForm, phoneNumber: e.target.value })} />
          <button className="btn" style={{ marginTop: 8 }}>Save</button>
        </form>
      </div>

      {editingId && (
        <div className="card">
          <h3>Edit Employee</h3>
          <form onSubmit={saveEdit}>
            <input placeholder="First name" value={editForm.firstName}
              onChange={e => setEditForm({ ...editForm, firstName: e.target.value })} />
            <input placeholder="Last name" value={editForm.lastName}
              onChange={e => setEditForm({ ...editForm, lastName: e.target.value })} />
            <input placeholder="Job title" value={editForm.jobTitle}
              onChange={e => setEditForm({ ...editForm, jobTitle: e.target.value })} />
            <input placeholder="Bio" value={editForm.bio}
              onChange={e => setEditForm({ ...editForm, bio: e.target.value })} />
            <input placeholder="Email" value={editForm.email}
              onChange={e => setEditForm({ ...editForm, email: e.target.value })} />
            <input placeholder="Phone number" value={editForm.phoneNumber}
              onChange={e => setEditForm({ ...editForm, phoneNumber: e.target.value })} />
            <button className="btn" style={{ marginTop: 8 }}>Save changes</button>
            <button type="button" className="btn" style={{ marginTop: 8, marginLeft: 8, background: "#64748b" }}
              onClick={() => setEditingId(null)}>Cancel</button>
          </form>
        </div>
      )}

      <div className="card">
        {loading ? <p>Loading...</p> : (
          <table>
            <thead><tr><th>Name</th><th>Job title</th><th>Email</th><th>Tenant</th><th></th></tr></thead>
            <tbody>
              {employees.map(emp => (
                <tr key={emp.id}>
                  <td>{emp.firstName} {emp.lastName}</td>
                  <td>{emp.jobTitle}</td>
                  <td>{emp.email}</td>
                  <td>{emp.tenantName}</td>
                  <td>
                    <button className="btn" onClick={() => startEdit(emp)}>Edit</button>{" "}
                    <button className="btn danger" onClick={() => remove(emp.id)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
