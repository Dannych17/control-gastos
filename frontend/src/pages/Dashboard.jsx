import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  ResponsiveContainer,
} from "recharts";
import { request, clearSession } from "../api";
import TransaccionForm from "../components/TransaccionForm";

const moneda = new Intl.NumberFormat("es-EC", {
  style: "currency",
  currency: "USD",
});

function mesActual() {
  const hoy = new Date();
  return `${hoy.getFullYear()}-${String(hoy.getMonth() + 1).padStart(2, "0")}`;
}

export default function Dashboard() {
  const navigate = useNavigate();
  const nombre = localStorage.getItem("nombre");
  const [periodo, setPeriodo] = useState(mesActual());
  const [resumen, setResumen] = useState(null);
  const [transacciones, setTransacciones] = useState([]);
  const [categorias, setCategorias] = useState([]);
  const [version, setVersion] = useState(0);
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    request("/categorias")
      .then(setCategorias)
      .catch(() => {});
  }, []);

  useEffect(() => {
    if (!periodo) return;
    const [anio, mes] = periodo.split("-");
    const query = `anio=${anio}&mes=${Number(mes)}`;
    let activo = true;

    Promise.all([
      request(`/transacciones/resumen?${query}`),
      request(`/transacciones?${query}`),
    ])
      .then(([datosResumen, lista]) => {
        if (!activo) return;
        setResumen(datosResumen);
        setTransacciones(lista);
        setError("");
      })
      .catch((err) => {
        if (!activo) return;
        if (err.message === "Error 401") {
          clearSession();
          navigate("/login");
          return;
        }
        setError(err.message);
      })
      .finally(() => {
        if (activo) setCargando(false);
      });

    return () => {
      activo = false;
    };
  }, [periodo, version, navigate]);

  function recargar() {
    setVersion((v) => v + 1);
  }

  function cerrarSesion() {
    clearSession();
    navigate("/login");
  }

  function alCrear(fecha) {
    setPeriodo(fecha.slice(0, 7));
    recargar();
  }

  async function eliminar(id) {
    if (!window.confirm("¿Eliminar esta transacción?")) return;
    try {
      await request(`/transacciones/${id}`, { method: "DELETE" });
      recargar();
    } catch (err) {
      setError(err.message);
    }
  }

  async function crearCategoriasEjemplo() {
    const ejemplo = [
      { nombre: "Sueldo", tipo: "Ingreso", color: "#2ecc71" },
      { nombre: "Comida", tipo: "Gasto", color: "#e67e22" },
      { nombre: "Transporte", tipo: "Gasto", color: "#3498db" },
    ];
    try {
      for (const c of ejemplo) {
        await request("/categorias", {
          method: "POST",
          body: JSON.stringify(c),
        });
      }
      setCategorias(await request("/categorias"));
    } catch (err) {
      setError(err.message);
    }
  }

  const gastos = resumen?.gastosPorCategoria ?? [];

  return (
    <div className="dashboard">
      <header className="topbar">
        <h1>💰 Control de Gastos</h1>
        <div className="topbar-user">
          <span>Hola, {nombre}</span>
          <button className="btn-outline" onClick={cerrarSesion}>
            Cerrar sesión
          </button>
        </div>
      </header>

      <div className="periodo">
        <label htmlFor="periodo">Mes</label>
        <input
          id="periodo"
          type="month"
          value={periodo}
          onChange={(e) => setPeriodo(e.target.value)}
        />
      </div>

      {error && <div className="error">{error}</div>}

      {cargando && !resumen && <p>Cargando...</p>}

      {resumen && (
        <>
          <section className="tarjetas">
            <div className="tarjeta ingreso">
              <span>Ingresos</span>
              <strong>{moneda.format(resumen.totalIngresos)}</strong>
            </div>
            <div className="tarjeta gasto">
              <span>Gastos</span>
              <strong>{moneda.format(resumen.totalGastos)}</strong>
            </div>
            <div
              className={`tarjeta balance ${resumen.balance >= 0 ? "positivo" : "negativo"}`}
            >
              <span>Balance</span>
              <strong>{moneda.format(resumen.balance)}</strong>
            </div>
          </section>

          <div className="grid-2">
            <section className="panel">
              <h2>Gastos por categoría</h2>
              {gastos.length === 0 ? (
                <p className="vacio">No hay gastos en este mes.</p>
              ) : (
                <ResponsiveContainer width="100%" height={320}>
                  <PieChart>
                    <Pie
                      data={gastos}
                      dataKey="total"
                      nameKey="categoria"
                      innerRadius={60}
                      outerRadius={110}
                      paddingAngle={2}
                    >
                      {gastos.map((g) => (
                        <Cell key={g.categoria} fill={g.color} />
                      ))}
                    </Pie>
                    <Tooltip formatter={(valor) => moneda.format(valor)} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              )}
            </section>

            <section className="panel">
              <h2>Nueva transacción</h2>
              {categorias.length === 0 ? (
                <div>
                  <p className="vacio">Aún no tienes categorías.</p>
                  <button className="btn" onClick={crearCategoriasEjemplo}>
                    Crear categorías de ejemplo
                  </button>
                </div>
              ) : (
                <TransaccionForm
                  categorias={categorias}
                  onCreada={alCrear}
                  onError={setError}
                />
              )}
            </section>
          </div>

          <section className="panel">
            <h2>Transacciones del mes</h2>
            {transacciones.length === 0 ? (
              <p className="vacio">No hay transacciones en este mes.</p>
            ) : (
              <ul className="lista">
                {transacciones.map((t) => (
                  <li key={t.id}>
                    <div>
                      <strong>{t.descripcion || t.categoriaNombre}</strong>
                      <small>
                        {t.fecha.slice(0, 10)} · {t.categoriaNombre}
                      </small>
                    </div>
                    <div className="lista-derecha">
                      <span
                        className={`monto ${t.tipo === "Ingreso" ? "ingreso" : "gasto"}`}
                      >
                        {t.tipo === "Ingreso" ? "+" : "-"}
                        {moneda.format(t.monto)}
                      </span>
                      <button
                        className="btn-borrar"
                        onClick={() => eliminar(t.id)}
                        aria-label="Eliminar transacción"
                      >
                        🗑️
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </div>
  );
}
