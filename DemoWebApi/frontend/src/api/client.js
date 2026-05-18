const API_BASE = (import.meta.env.VITE_API_BASE_URL || "").replace(/\/$/, "");

let authToken = "";

export function setAuthToken(token) {
  authToken = token || "";
}

async function request(path, options = {}) {
  const headers = {
    "Content-Type": "application/json",
    ...(options.headers || {})
  };

  if (authToken) {
    headers.Authorization = `Bearer ${authToken}`;
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers
  });

  const isJson = response.headers.get("content-type")?.includes("application/json");
  const payload = isJson ? await response.json() : null;

  if (!response.ok) {
    const message = payload?.message || payload?.Message || `Request failed: ${response.status}`;
    throw new Error(message);
  }

  return payload;
}

export const api = {
  login(data) {
    return request("/api/Auth/login", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },
  getMe() {
    return request("/api/Auth/me");
  },
  getHealth() {
    return request("/api/Home/health");
  },
  getHello() {
    return request("/api/Hello/hello");
  },
  getUsers() {
    return request("/api/Users");
  },
  createUser(data) {
    return request("/api/Users", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },
  updateUser(id, data) {
    return request(`/api/Users/${id}`, {
      method: "PUT",
      body: JSON.stringify(data)
    });
  },
  deleteUser(id) {
    return request(`/api/Users/${id}`, {
      method: "DELETE"
    });
  },
  getRooms() {
    return request("/api/Rooms");
  },
  createRoom(data) {
    return request("/api/Rooms", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },
  updateRoom(id, data) {
    return request(`/api/Rooms/${id}`, {
      method: "PUT",
      body: JSON.stringify(data)
    });
  },
  deleteRoom(id) {
    return request(`/api/Rooms/${id}`, {
      method: "DELETE"
    });
  },
  getRoomThings(id) {
    return request(`/api/Rooms/${id}/things`);
  },
  getThings() {
    return request("/api/Things");
  },
  createThing(data) {
    return request("/api/Things", {
      method: "POST",
      body: JSON.stringify(data)
    });
  },
  updateThing(id, data) {
    return request(`/api/Things/${id}`, {
      method: "PUT",
      body: JSON.stringify(data)
    });
  },
  deleteThing(id) {
    return request(`/api/Things/${id}`, {
      method: "DELETE"
    });
  }
};
