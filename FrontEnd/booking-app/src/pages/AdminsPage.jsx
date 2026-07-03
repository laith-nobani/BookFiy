import React, { useEffect, useState } from "react";
import { apiFetch } from "../api";
import { useAuth } from "../AuthContext";

// AdminDto only returns { id, name, email, role } - no delete endpoint
// exists on AdminController, only create (POST) and update (PUT).
export default function AdminsPage() {
  const { token } = useAuth();
  const [admins, setAdmins] = useState([]);
  const [loading, setLoading] = useState(true);

  const blankCreate = { userName: "", email: "", firstName: "", lastName: "", phoneNumber: "", tenantId: "" };
  const [createForm, setCreateForm] = useState(blankCreate);

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ firstName: "", lastName: "", email: "", phoneNumber: "", userName: "" });

  const load = () => {
    setLoading(true);
    apiFetch("/Admin", token).then(setAdmins).catch(() => setAdmins([])).finally(() => setLoading(false));
  };
  useEffect(load, []);

  const create = async (e) => {
    e.preventDefault();
    await apiFetch("/Admin", token, { method: "POST", body: JSON.stringify(createForm) });
    setCreateForm(blankCreate);
    load();
  };

  const startEdit = (a) => {
    // AdminDto only gives a combined "name" - split it as a starting point,
    // the SuperAdmin can correct it before saving.
    const [firstName = "", ...rest] = (a.name || "").split(" ");
    setEditingId(a.id);
    setEditForm({ firstName, lastName: rest.join(" "), email: a.email, phoneNumber: "", userName: "" });
  };

  const saveEdit = async (e) => {
    e.preventDefault();
    await apiFetch(`/Admin/${editingId}`, token, { method: "PUT", body: JSON.stringify(editForm) });
    setEditingId(null);
    load();
  };

  return (
    <div>
      <h1>Admins</h1>

      <div className="card">
        <h3>Add Admin</h3>
        <form onSubmit={create}>
          <input placeholder="First name" value={createForm.firstName}
            onChange={e => setCreateForm({ ...createForm, firstName: e.target.value })} />
          <input placeholder="Last name" value={createForm.lastName}
            onChange={e => setCreateForm({ ...createForm, lastName: e.target.value })} />
          <input placeholder="Username" value={createForm.userName}
            onChange={e => setCreateForm({ ...createForm, userName: e.target.value })} />
          <input placeholder="Email" value={createForm.email}
            onChange={e => setCreateForm({ ...createForm, email: e.target.value })} />
          <input placeholder="Phone number" value={createForm.phoneNumber}
            onChange={e => setCreateForm({ ...createForm, phoneNumber: e.target.value })} />
          <input placeholder="Tenant ID (guid)" value={createForm.tenantId}
            onChange={e => setCreateForm({ ...createForm, tenantId: e.target.value })} />
          <button className="btn" style={{ marginTop: 8 }}>Save</button>
        </form>
      </div>

      {editingId && (
        <div className="card">
          <h3>Edit Admin</h3>
          <form onSubmit={saveEdit}>
            <input placeholder="First name" value={editForm.firstName}
              onChange={e => setEditForm({ ...editForm, firstName: e.target.value })} />
            <input placeholder="Last name" value={editForm.lastName}
              onChange={e => setEditForm({ ...editForm, lastName: e.target.value })} />
            <input placeholder="Username" value={editForm.userName}
              onChange={e => setEditForm({ ...editForm, userName: e.target.value })} />
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
            <thead><tr><th>Name</th><th>Email</th><th>Role</th><th></th></tr></thead>
            <tbody>
              {admins.map(a => (
                <tr key={a.id}>
                  <td>{a.name}</td>
                  <td>{a.email}</td>
                  <td>{a.role}</td>
                  <td><button className="btn" onClick={() => startEdit(a)}>Edit</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
