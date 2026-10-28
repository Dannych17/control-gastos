import { useState } from "react";
import { request } from "../api";

function hoy() {
  const d = new Date();
  const mes = String(d.getMonth() + 1).padStart(2, "0");
  const dia = String(d.getDate()).padStart(2, "0");
  return `${d.getFullYear()}-${mes}-${dia}`;
}

export default function TransaccionForm({ categorias, onCreada, onError }) {
  const [monto, setMonto] = useState("");
  const [fecha, setFecha] = useState(hoy());
  const [descripcion, setDescripcion] = useState("");
  const [categoriaId, setCategoriaId] = useState("");
  const [guardando, setGuardando] = useState(false);

  const ingresos = categorias.filter((c) => c.tipo === "Ingreso");
  const gastos = categorias.filter((c) => c.tipo === "Gasto");

  async function handleSubmit(e) {
    e.preventDefault();
    setGuardando(true);

    try {
      await request("/transacciones", {
        method: "POST",
        body: JSON.stringify({
          monto: Number(monto),
          fecha,
          descripcion: descripcion || null,
          categoriaId: Number(categoriaId),
        }),
      });
      setMonto("");
      setDescripcion("");
      onCreada(fecha);
    } catch (err) {
      onError(err.message);
    } finally {
      setGuardando(false);
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <div className="form-fila">
        <div>
          <label htmlFor="monto">Monto</label>
          <input
            id="monto"
            type="number"
            step="0.01"
            min="0.01"
            value={monto}
            onChange={(e) => setMonto(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="fecha">Fecha</label>
          <input
            id="fecha"
            type="date"
            value={fecha}
            onChange={(e) => setFecha(e.target.value)}
            required
          />
        </div>
        <div>
          <label htmlFor="categoria">Categoría</label>
          <select
            id="categoria"
            value={categoriaId}
            onChange={(e) => setCategoriaId(e.target.value)}
            required
          >
            <option value="">Selecciona...</option>
            <optgroup label="Ingresos">
              {ingresos.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </optgroup>
            <optgroup label="Gastos">
              {gastos.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.nombre}
                </option>
              ))}
            </optgroup>
          </select>
        </div>
        <div>
          <label htmlFor="descripcion">Descripción</label>
          <input
            id="descripcion"
            value={descripcion}
            maxLength={200}
            onChange={(e) => setDescripcion(e.target.value)}
          />
        </div>
      </div>

      <button className="btn" type="submit" disabled={guardando}>
        {guardando ? "Guardando..." : "Agregar transacción"}
      </button>
    </form>
  );
}
