import React, { useEffect, useState } from "react";
import { apiFetch, getUserIdFromToken } from "../api";
import { useAuth } from "../AuthContext";

// Notes on the real API (BookingController):
// - There is NO tenant-wide "get all bookings" route - only scoped by
//   employee (/bookings/empoloyee/{employeeId}) or by user (/bookings/user/{userId}).
// - DeleteBooking has no {bookingId} route segment, so it's bound from the
//   query string: DELETE /bookings?bookingId=...
// - CreateBookingDto wasn't provided, so the create payload below is a
//   best guess (serviceId, employeeId, tenantId, startTime, notes) -
//   adjust the field names in `book()` to match your real DTO.
// - UpdateBookingDto exists (StartTime, StatusId, Notes) but no PUT action
//   was shown on the controller, so `saveEdit()` assumes PUT /bookings/{id}
//   - confirm/add that route on the backend if it's missing.
const STATUS_LABELS = { 1: "Pending", 2: "Confirmed", 3: "Cancelled", 4: "Completed" };

export default function BookingsPage() {
  const { token, user } = useAuth();
  const isCustomer = user.role === "Customer";

  const [bookings, setBookings] = useState([]);
  const [loading, setLoading] = useState(false);
  const [hasMore, setHasMore] = useState(false);

  // Filtering / paging / sorting - shared by both the employee-scoped and
  // user-scoped list endpoints, which both take the same query params.
  const [filters, setFilters] = useState({ from: "", to: "", page: 1, pageSize: 10, sort: "asc" });

  // Staff (Employee/Admin/SuperAdmin) must supply which employee's
  // bookings to view, since there's no tenant-wide list endpoint.
  const [employeeId, setEmployeeId] = useState("");

  // Customer's own user id, pulled from the JWT. Falls back to manual
  // entry if the claim name doesn't match what's in your token.
  const [userId, setUserId] = useState(() => getUserIdFromToken(token) || "");

  const [createForm, setCreateForm] = useState({ serviceId: "", startTime: "", notes: "" });

  const [editingId, setEditingId] = useState(null);
  const [editForm, setEditForm] = useState({ startTime: "", statusId: "", notes: "" });

  const buildQuery = () => {
    const params = new URLSearchParams();
    if (filters.from) params.set("from", filters.from);
    if (filters.to) params.set("to", filters.to);
    params.set("page", String(filters.page));
    params.set("pageSize", String(filters.pageSize));
    params.set("sort", filters.sort);
    return params.toString();
  };

  const load = () => {
    const scopeId = isCustomer ? userId : userId;
    if (!scopeId) return;
    const path = isCustomer ? `/Booking/user/${scopeId}` : `/Booking/empoloyee/${scopeId}`;
    setLoading(true);
    apiFetch(`${path}?${buildQuery()}`, token)
      .then(data => {
        const list = Array.isArray(data) ? data : (data?.items ?? data?.data ?? []);
        setBookings(list);
        setHasMore(list.length === filters.pageSize);
      })
      .catch(() => setBookings([]))
      .finally(() => setLoading(false));
  };

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(load, [filters.page, filters.sort]);

  const applyFilters = () => { setFilters(f => ({ ...f, page: 1 })); load(); };
  const clearFilters = () => setFilters({ from: "", to: "", page: 1, pageSize: 10, sort: "asc" });

  const book = async (e) => {
    e.preventDefault();
    const finalUserId = userId || user?.userId || localStorage.getItem("userId");
    const tenantId = user?.tenantId || "75C546DC-74C5-45EB-9563-75F9A0485C7B";
    try {
      await apiFetch("/Booking", token, {
        method: "POST",
        body: JSON.stringify({
          serviceId: createForm.serviceId,
          userId: finalUserId,
          tenantId: tenantId,
          startTime: new Date(createForm.startTime).toISOString(),
        }),
      });
      alert("Booked successfully");
      setCreateForm({ serviceId: "", startTime: "", notes: "" });
      load();
    } catch (err) {
      alert("Failed to book: " + err.message);
    }
  };

  const cancel = async (id) => {
    if (!confirm("Cancel this booking?")) return;
    await apiFetch(`/Booking?bookingId=${id}`, token, { method: "DELETE" });
    load();
  };

  const startEdit = (b) => {
    setEditingId(b.id);
    setEditForm({
      startTime: b.bookingDate ? new Date(b.bookingDate).toISOString().slice(0, 16) : "",
      statusId: b.statusId ?? "",
      notes: "",
    });
  };

  const saveEdit = async (e) => {
    e.preventDefault();
    await apiFetch(`/Booking/${editingId}`, token, {
      method: "PUT",
      body: JSON.stringify({
        startTime: editForm.startTime ? new Date(editForm.startTime).toISOString() : null,
        statusId: editForm.statusId ? Number(editForm.statusId) : null,
        notes: editForm.notes || null,
      }),
    });
    setEditingId(null);
    load();
  };

  return (
    <div>
      <h1>Bookings</h1>

      {isCustomer ? (
        <div className="card">
          <h3>Book a service</h3>
          <form onSubmit={book}>
            <input placeholder="Service ID (guid)" value={createForm.serviceId}
              onChange={e => setCreateForm({ ...createForm, serviceId: e.target.value })} />
            <input type="datetime-local" value={createForm.startTime}
              onChange={e => setCreateForm({ ...createForm, startTime: e.target.value })} />
            <input placeholder="Notes (optional)" value={createForm.notes}
              onChange={e => setCreateForm({ ...createForm, notes: e.target.value })} />
            <button className="btn" style={{ marginTop: 8 }}>Book</button>
          </form>
        </div>
      ) :
        null
      }

      {isCustomer && !userId && (
        <div className="card">
          <p style={{ fontSize: 13, color: "#b45309" }}>Couldn't read your user ID from the token automatically - enter it manually.</p>
          <input placeholder="Your User ID (guid)" value={userId} onChange={e => setUserId(e.target.value)} />
          <button className="btn" style={{ marginTop: 8 }} onClick={load}>Load my bookings</button>
        </div>
      )}

      {/* Filters */}
      <div className="card" style={{ display: "flex", flexWrap: "wrap", gap: 12, alignItems: "flex-end" }}>
        <div style={{ flex: "1 1 160px" }}>
          <label style={{ fontSize: 12, color: "#555" }}>From</label>
          <input type="date" value={filters.from} onChange={e => setFilters({ ...filters, from: e.target.value })} />
        </div>
        <div style={{ flex: "1 1 160px" }}>
          <label style={{ fontSize: 12, color: "#555" }}>To</label>
          <input type="date" value={filters.to} onChange={e => setFilters({ ...filters, to: e.target.value })} />
        </div>
        <div style={{ flex: "1 1 140px" }}>
          <label style={{ fontSize: 12, color: "#555" }}>Sort</label>
          <select value={filters.sort} onChange={e => setFilters({ ...filters, sort: e.target.value })}>
            <option value="asc">Oldest first</option>
            <option value="desc">Newest first</option>
          </select>
        </div>
        <div style={{ flex: "1 1 100px" }}>
          <label style={{ fontSize: 12, color: "#555" }}>Page size</label>
          <select value={filters.pageSize} onChange={e => setFilters({ ...filters, pageSize: Number(e.target.value) })}>
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </select>
        </div>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="btn" onClick={applyFilters}>Apply</button>
          <button className="btn" style={{ background: "#64748b" }} onClick={clearFilters}>Clear</button>
        </div>
      </div>

      {editingId && (
        <div className="card">
          <h3>Update booking</h3>
          <form onSubmit={saveEdit}>
            <input type="datetime-local" value={editForm.startTime}
              onChange={e => setEditForm({ ...editForm, startTime: e.target.value })} />
            <select value={editForm.statusId} onChange={e => setEditForm({ ...editForm, statusId: e.target.value })}>
              <option value="">Keep current status</option>
              {Object.entries(STATUS_LABELS).map(([id, label]) => <option key={id} value={id}>{label}</option>)}
            </select>
            <input placeholder="Notes" value={editForm.notes}
              onChange={e => setEditForm({ ...editForm, notes: e.target.value })} />
            <button className="btn" style={{ marginTop: 8 }}>Save changes</button>
            <button type="button" className="btn" style={{ marginTop: 8, marginLeft: 8, background: "#64748b" }}
              onClick={() => setEditingId(null)}>Cancel edit</button>
          </form>
        </div>
      )}

      <div className="card">
        {loading ? <p>Loading...</p> : bookings.length === 0 ? <p>No bookings found.</p> : (
          <table>
            <thead>
              <tr>
                <th>Customer</th><th>Date</th><th>Status</th><th></th>
              </tr>
            </thead>
            <tbody>
              {bookings.map(b => (
                <tr key={b.id}>
                  <td>
                    <div>{b.userName}</div>
                    <div style={{ fontSize: 12, color: "#777" }}>{b.userEmail}</div>
                  </td>
                  <td>{b.bookingDate ? new Date(b.bookingDate).toLocaleString() : "-"}</td>
                  <td><span className="badge">{b.statusName || STATUS_LABELS[b.statusId] || b.statusId}</span></td>
                  <td>
                    {!isCustomer && <button className="btn" onClick={() => startEdit(b)}>Update</button>}{" "}
                    <button className="btn danger" onClick={() => cancel(b.id)}>Cancel</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
        <p style={{ fontSize: 13, color: "#555" }}>Page {filters.page}</p>
        <div style={{ display: "flex", gap: 8 }}>
          <button className="btn" disabled={filters.page === 1}
            onClick={() => setFilters(f => ({ ...f, page: Math.max(1, f.page - 1) }))}>Prev</button>
          <button className="btn" disabled={!hasMore}
            onClick={() => setFilters(f => ({ ...f, page: f.page + 1 }))}>Next</button>
        </div>
      </div>
    </div>
  );
}
