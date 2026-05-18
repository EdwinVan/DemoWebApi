const API_BASE = (import.meta.env.VITE_API_BASE_URL || "").replace(/\/$/, "");

async function request(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {})
    },
    ...options
  });

  const isJson = response.headers.get("content-type")?.includes("application/json");
  const payload = isJson ? await response.json() : null;

  if (!response.ok) {
    const message = payload?.message || payload?.Message || `请求失败: ${response.status}`;
    throw new Error(message);
  }

  return payload;
}

export const api = {
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
  }
};
