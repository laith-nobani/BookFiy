import React, { useEffect, useState } from "react";
import { apiFetch } from "../api";
import { useAuth } from "../AuthContext";

// Small reusable pattern: fetch a list from `path`, show a table, and a
// create form built from `fields`. Used where create+delete both exist
// and the response shape is simple (currently just Tenants).
export default function ResourcePage({ title, path, fields, columns }) {
  const { token } = useAuth();
  const [items, setItems] = useState([]);
  const [form, setForm] = useState(Object.fromEntries(fields.map(f => [f.name, ""])));
  const [loading, setLoading] = useState(true);

  const load = () => {
    setLoading(true);
    apiFetch(path, token).then(setItems).catch(() => setItems([])).finally(() => setLoading(false));
  };

  useEffect(load, []);

  const create = async (e) => {
    e.preventDefault();
    await apiFetch(path, token, { method: "POST", body: JSON.stringify(form) });
    setForm(Object.fromEntries(fields.map(f => [f.name, ""])));
    load();
  };

  const remove = async (id) => {
    await apiFetch(`${path}/${id}`, token, { method: "DELETE" });
    load();
  };

  return (
    <div>
      <h1>{title}</h1>

      <div className="card">
        <h3>Add {title.slice(0, -1)}</h3>
        <form onSubmit={create}>
          {fields.map(f => (
            <input
              key={f.name}
              placeholder={f.label}
              value={form[f.name]}
              onChange={e => setForm({ ...form, [f.name]: e.target.value })}
            />
          ))}
          <button className="btn" style={{ marginTop: 8 }}>Save</button>
        </form>
      </div>

      <div className="card">
        {loading ? <p>Loading...</p> : (
          <table>
            <thead><tr>{columns.map(c => <th key={c.key}>{c.label}</th>)}<th></th></tr></thead>
            <tbody>
              {items.map(item => (
                <tr key={item.id}>
                  {columns.map(c => <td key={c.key}>{item[c.key]}</td>)}
                  <td><button className="btn danger" onClick={() => remove(item.id)}>Delete</button></td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
