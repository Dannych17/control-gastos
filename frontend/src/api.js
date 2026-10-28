const API_URL = import.meta.env.VITE_API_URL ?? "https://localhost:7125/api";

export function getToken() {
  return localStorage.getItem("token");
}

export function saveSession(data) {
  localStorage.setItem("token", data.token);
  localStorage.setItem("nombre", data.nombre);
}

export function clearSession() {
  localStorage.removeItem("token");
  localStorage.removeItem("nombre");
}

function extraerMensaje(data, status) {
  if (typeof data === "string" && data) return data;
  if (data?.errors) return Object.values(data.errors).flat().join(" ");
  return `Error ${status}`;
}

export async function request(path, options = {}) {
  const token = getToken();
  let response;

  try {
    response = await fetch(`${API_URL}${path}`, {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    });
  } catch {
    throw new Error(
      "No se pudo conectar con el servidor. ¿Está corriendo la API?",
    );
  }

  const text = await response.text();
  let data = null;
  try {
    data = text ? JSON.parse(text) : null;
  } catch {
    data = text;
  }

  if (!response.ok) {
    throw new Error(extraerMensaje(data, response.status));
  }

  return data;
}
