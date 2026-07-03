import React, { useEffect, useState } from "react";
import { apiFetch } from "../api";
import { useAuth } from "../AuthContext";

export default function CustomerServicesPage() {
  const { token, user } = useAuth();

  const [services, setServices] = useState([]);
  const [selected, setSelected] = useState(null);
  const [time, setTime] = useState("");
  const [notes, setNotes] = useState("");

  const load = async () => {
    const data = await apiFetch("/services", token);
    setServices(data);
  };

  useEffect(() => {
    load();
  }, []);

  const book = async (e) => {
    e.preventDefault();
    const userId = user?.userId || localStorage.getItem("userId");
    const tenantId = selected.tenantId || user?.tenantId || "75C546DC-74C5-45EB-9563-75F9A0485C7B";

    try {
      await apiFetch("/Booking", token, {
        method: "POST",
        body: JSON.stringify({
          tenantId: tenantId,
          serviceId: selected.id,
          userId: userId,
          startTime: new Date(time).toISOString(),
        }),
      });

      alert("Booked successfully");
      setSelected(null);
      setTime("");
      setNotes("");
    } catch (err) {
      alert("Failed to book: " + err.message);
    }
  };

  return (
    <div>
      <h1>Available Services</h1>

      {/* GRID (Uber/Fresha style) */}
      <div style={{
        display: "grid",
        gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
        gap: 16
      }}>
        {services.map(s => (
          <div key={s.id} className="card">
            <h3>{s.name}</h3>
            <p>{s.description}</p>
            <p>⏱ {s.durationMinutes} min</p>
            <p>💰 ${s.price}</p>

            <button className="btn" onClick={() => setSelected(s)}>
              Book
            </button>
          </div>
        ))}
      </div>

      {/* MODAL */}
      {selected && (
        <div className="modal">
          <div className="card">
            <h2>Book {selected.name}</h2>

            <form onSubmit={book}>
              <input
                type="datetime-local"
                value={time}
                onChange={e => setTime(e.target.value)}
                required
              />

              <input
                placeholder="Notes"
                value={notes}
                onChange={e => setNotes(e.target.value)}
              />

              <button className="btn">Confirm</button>
              <button type="button" onClick={() => setSelected(null)}>
                Cancel
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}