import React, { useState,useEffect } from "react";
import { apiFetch } from "../api";
import { useAuth } from "../AuthContext";

// Employee only - ServicesController is [Authorize(Roles = Roles.Employee)].
// GET is GET /services/{employeeId} (employeeId is a path segment, not a
// query param), so an employee needs to know their own Employee ID to list
// their services. Create/Update/Delete are otherwise standard.
export default function ServicesPage() {
  const { token } = useAuth();
  const [services, setServices] = useState([]);
  const [loading, setLoading] = useState(false);

  const blank = { name: "", description: "", durationMinutes: "", price: "" };
  const [createForm, setCreateForm] = useState(blank);
  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState(blank);

  const load = () => {
    var employeeId = localStorage.getItem("userId")
    setLoading(true);
    apiFetch(`/services/employee/${employeeId}`, token).then(setServices).catch(() => setServices([])).finally(() => setLoading(false));
  };

  const create = async (e) => {
    e.preventDefault();
    var employeeId = localStorage.getItem("userId")

    await apiFetch("/services", token, {
      method: "POST",
      body: JSON.stringify({ ...createForm, employeeId }),
    });
    setCreateForm(blank);
    load();
  };

 useEffect(() => {
  const employeeId = localStorage.getItem("userId");

  if (!employeeId) return;

  load();
}, []);

  const startEdit = (s) => {
    setEditingId(s.id);
    setEditForm({ name: s.name, description: s.description, durationMinutes: s.durationMinutes, price: s.price });
  };

  const saveEdit = async (e) => {
    e.preventDefault();
    await apiFetch(`/services/${editingId}`, token, { method: "PUT", body: JSON.stringify(editForm) });
    setEditingId(null);
    load();
  };

  const remove = async (id) => {
    await apiFetch(`/services/${id}`, token, { method: "DELETE" });
    load();
  };

  return (
    <div>
      <h1>Services</h1>

      <div className="card">
        <h3>Add Service</h3>
        <form onSubmit={create}>
          <input placeholder="Service name" value={createForm.name}
            onChange={e => setCreateForm({ ...createForm, name: e.target.value })} />
          <input placeholder="Description" value={createForm.description}
            onChange={e => setCreateForm({ ...createForm, description: e.target.value })} />
          <input placeholder="Duration (min)" value={createForm.durationMinutes}
            onChange={e => setCreateForm({ ...createForm, durationMinutes: e.target.value })} />
          <input placeholder="Price" value={createForm.price}
            onChange={e => setCreateForm({ ...createForm, price: e.target.value })} />
          <button className="btn" style={{ marginTop: 8 }}>Save</button>
        </form>
      </div>

      {editingId && (
        <div className="card">
          <h3>Edit Service</h3>
          <form onSubmit={saveEdit}>
            <input placeholder="Service name" value={editForm.name}
              onChange={e => setEditForm({ ...editForm, name: e.target.value })} />
            <input placeholder="Description" value={editForm.description}
              onChange={e => setEditForm({ ...editForm, description: e.target.value })} />
            <input placeholder="Duration (min)" value={editForm.durationMinutes}
              onChange={e => setEditForm({ ...editForm, durationMinutes: e.target.value })} />
            <input placeholder="Price" value={editForm.price}
              onChange={e => setEditForm({ ...editForm, price: e.target.value })} />
            <button className="btn" style={{ marginTop: 8 }}>Save changes</button>
            <button type="button" className="btn" style={{ marginTop: 8, marginLeft: 8, background: "#64748b" }}
              onClick={() => setEditingId(null)}>Cancel</button>
          </form>
        </div>
      )}

      <div className="card">
        {loading ? <p>Loading...</p> : services.length === 0 ? <p>No services loaded yet.</p> : (
          <table>
            <thead><tr><th>Name</th><th>Duration</th><th>Price</th><th></th></tr></thead>
            <tbody>
              {services.map(s => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>{s.durationMinutes} min</td>
                  <td>{s.price}</td>
                  <td>
                    <button className="btn" onClick={() => startEdit(s)}>Edit</button>{" "}
                    <button className="btn danger" onClick={() => remove(s.id)}>Delete</button>
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
