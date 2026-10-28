import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { request, saveSession } from "../api";

export default function Login() {
  const [modo, setModo] = useState("login");
  const [nombre, setNombre] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [cargando, setCargando] = useState(false);
  const navigate = useNavigate();

  const esRegistro = modo === "register";

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");
    setCargando(true);

    try {
      const body = esRegistro
        ? { nombre, email, password }
        : { email, password };
      const data = await request(`/auth/${modo}`, {
        method: "POST",
        body: JSON.stringify(body),
      });
      saveSession(data);
      navigate("/");
    } catch (err) {
      setError(err.message);
    } finally {
      setCargando(false);
    }
  }

  return (
    <div className="auth-page">
      <form className="card" onSubmit={handleSubmit}>
        <h1>💰 Control de Gastos</h1>
        <p className="subtitle">
          {esRegistro ? "Crea tu cuenta" : "Inicia sesión para continuar"}
        </p>

        {error && <div className="error">{error}</div>}

        {esRegistro && (
          <>
            <label htmlFor="nombre">Nombre</label>
            <input
              id="nombre"
              value={nombre}
              onChange={(e) => setNombre(e.target.value)}
              required
            />
          </>
        )}

        <label htmlFor="email">Correo</label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />

        <label htmlFor="password">Contraseña</label>
        <input
          id="password"
          type="password"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          minLength={esRegistro ? 6 : undefined}
          required
        />

        <button className="btn" type="submit" disabled={cargando}>
          {cargando ? "Un momento..." : esRegistro ? "Registrarme" : "Entrar"}
        </button>

        <p className="switch">
          {esRegistro ? "¿Ya tienes cuenta? " : "¿No tienes cuenta? "}
          <button
            type="button"
            className="btn-link"
            onClick={() => {
              setError("");
              setModo(esRegistro ? "login" : "register");
            }}
          >
            {esRegistro ? "Inicia sesión" : "Regístrate"}
          </button>
        </p>
      </form>
    </div>
  );
}
