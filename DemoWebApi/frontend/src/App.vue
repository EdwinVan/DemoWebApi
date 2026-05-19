<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { api, setAuthToken } from "./api/client";

const TOKEN_KEY = "demo_web_api_token";
const tabs = [
  { key: "overview", label: "Overview" },
  { key: "rooms", label: "Rooms" },
  { key: "things", label: "Things" },
  { key: "users", label: "Users" }
];
const colorOptions = ["Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet"];

const activeTab = ref("overview");
const banner = reactive({ type: "info", message: "" });

const authUser = ref(null);
const authLoading = ref(false);
const showLoginPassword = ref(false);
const loginForm = reactive({
  userNameOrEmail: "",
  password: ""
});

const overview = reactive({ health: null, hello: null, loading: false });
const rooms = ref([]);
const things = ref([]);
const users = ref([]);

const loadingRooms = ref(false);
const loadingThings = ref(false);
const loadingUsers = ref(false);
const roomThingsLoading = ref(false);

const roomForm = reactive({ id: null, computerId: null, bedId: null });
const thingForm = reactive({
  sourceId: null,
  color: "Red",
  price: "",
  number: "",
  description: ""
});
const userForm = reactive({ id: null, userName: "", email: "", age: "", password: "", isActive: true });
const showUserPassword = ref(false);

const selectedRoomThings = ref(null);
let clockTimer = null;
const healthClockMs = ref(null);

const isAuthenticated = computed(() => authUser.value !== null);
const visibleTabs = computed(() => (isAuthenticated.value ? tabs : tabs.filter((t) => t.key === "overview")));
const roomCount = computed(() => rooms.value.length);
const thingCount = computed(() => things.value.length);
const userCount = computed(() => users.value.length);
const selectedThingCount = computed(() => selectedRoomThings.value?.things?.length ?? 0);
const passwordRuleText =
  "8-64 chars, include uppercase, lowercase, number and special character, no spaces.";
const passwordChecks = computed(() => {
  const value = userForm.password || "";
  return {
    length: value.length >= 8 && value.length <= 64,
    upper: /[A-Z]/.test(value),
    lower: /[a-z]/.test(value),
    digit: /[0-9]/.test(value),
    special: /[^a-zA-Z0-9]/.test(value),
    noSpace: !/\s/.test(value)
  };
});

function showMessage(message, type = "success") {
  banner.type = type;
  banner.message = message;
}

function clearProtectedData() {
  rooms.value = [];
  things.value = [];
  users.value = [];
  selectedRoomThings.value = null;
}

function isUnauthorizedError(error) {
  const message = String(error?.message ?? "");
  return (
    message.includes("401") ||
    message.toLowerCase().includes("unauthorized") ||
    message.includes("未授权") ||
    message.includes("登录")
  );
}

function logout(showTip = true) {
  authUser.value = null;
  setAuthToken("");
  localStorage.removeItem(TOKEN_KEY);
  clearProtectedData();
  activeTab.value = "overview";
  if (showTip) {
    showMessage("Logged out.", "info");
  }
}

function handleProtectedError(error) {
  if (isUnauthorizedError(error)) {
    logout(false);
    showMessage("Please login first.", "error");
    return;
  }

  showMessage(error.message, "error");
}

function toNullableNumber(value) {
  return value === "" || value === null || value === undefined ? null : Number(value);
}

function toNullableInteger(value) {
  if (value === "" || value === null || value === undefined) {
    return null;
  }
  const parsed = Number(value);
  return Number.isInteger(parsed) ? parsed : NaN;
}

function normalizeUserAge() {
  if (userForm.age === "" || userForm.age === null || userForm.age === undefined) {
    return;
  }

  let age = Number(userForm.age);
  if (!Number.isFinite(age)) {
    userForm.age = "";
    return;
  }

  age = Math.trunc(age);
  age = Math.max(0, Math.min(120, age));
  userForm.age = age;
}

function formatPrice(value) {
  if (value === null || value === undefined) {
    return "-";
  }
  return Number(value).toFixed(2);
}

function formatDateTime(value) {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return String(value);
  }

  const pad = (n) => String(n).padStart(2, "0");
  const yyyy = date.getFullYear();
  const mm = pad(date.getMonth() + 1);
  const dd = pad(date.getDate());
  const hh = pad(date.getHours());
  const mi = pad(date.getMinutes());
  const ss = pad(date.getSeconds());
  return `${yyyy}-${mm}-${dd} ${hh}:${mi}:${ss}`;
}

function parseDateTimeMs(value) {
  if (!value) {
    return null;
  }
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? null : date.getTime();
}

function clearRoomForm() {
  roomForm.id = null;
  roomForm.computerId = null;
  roomForm.bedId = null;
}

function clearThingForm() {
  thingForm.sourceId = null;
  thingForm.color = "Red";
  thingForm.price = "";
  thingForm.number = "";
  thingForm.description = "";
}

function clearUserForm() {
  userForm.id = null;
  userForm.userName = "";
  userForm.email = "";
  userForm.age = "";
  userForm.password = "";
  showUserPassword.value = false;
  userForm.isActive = true;
}

function validatePassword(password) {
  if (!password) {
    return "Password is required.";
  }
  if (password.length < 8 || password.length > 64) {
    return "Password must be 8-64 characters.";
  }
  if (/\s/.test(password)) {
    return "Password cannot contain spaces.";
  }
  if (!/[A-Z]/.test(password)) {
    return "Password must include at least one uppercase letter.";
  }
  if (!/[a-z]/.test(password)) {
    return "Password must include at least one lowercase letter.";
  }
  if (!/[0-9]/.test(password)) {
    return "Password must include at least one number.";
  }
  if (!/[^a-zA-Z0-9]/.test(password)) {
    return "Password must include at least one special character.";
  }
  return "";
}

function thingOptionLabel(thing) {
  const color = thing.color ?? "NoColor";
  const price = formatPrice(thing.price);
  const number = thing.number ?? "-";
  return `#${thing.id} | ${color} | $${price} | qty ${number}`;
}

async function loadOverview() {
  overview.loading = true;
  try {
    const [health, hello] = await Promise.all([api.getHealth(), api.getHello()]);
    overview.health = health;
    overview.hello = hello;
    healthClockMs.value = parseDateTimeMs(health?.time) ?? Date.now();
  } catch (error) {
    showMessage(error.message, "error");
  } finally {
    overview.loading = false;
  }
}

async function loadCurrentUser() {
  const result = await api.getMe();
  authUser.value = result?.data ?? result?.Data ?? null;
  return authUser.value !== null;
}

async function login() {
  if (!loginForm.userNameOrEmail || !loginForm.password) {
    showMessage("Please input account and password.", "error");
    return;
  }

  authLoading.value = true;
  try {
    const result = await api.login({
      userNameOrEmail: loginForm.userNameOrEmail,
      password: loginForm.password
    });

    const data = result?.data ?? result?.Data ?? {};
    const token = data?.token ?? data?.Token;
    const user = data?.user ?? data?.User ?? null;

    if (!token) {
      throw new Error("Login failed: token missing.");
    }

    setAuthToken(token);
    localStorage.setItem(TOKEN_KEY, token);
    authUser.value = user;

    if (!authUser.value) {
      await loadCurrentUser();
    }

    await refreshAll();
    showMessage(`Welcome, ${authUser.value?.userName ?? "user"}.`, "success");
  } catch (error) {
    logout(false);
    showMessage(error.message, "error");
  } finally {
    authLoading.value = false;
  }
}

async function loadRooms() {
  if (!isAuthenticated.value) {
    rooms.value = [];
    return;
  }

  loadingRooms.value = true;
  try {
    const result = await api.getRooms();
    rooms.value = result.data ?? [];
  } catch (error) {
    handleProtectedError(error);
  } finally {
    loadingRooms.value = false;
  }
}

async function loadThings() {
  if (!isAuthenticated.value) {
    things.value = [];
    return;
  }

  loadingThings.value = true;
  try {
    const result = await api.getThings();
    things.value = result.data ?? [];
  } catch (error) {
    handleProtectedError(error);
  } finally {
    loadingThings.value = false;
  }
}

async function loadUsers() {
  if (!isAuthenticated.value) {
    users.value = [];
    return;
  }

  loadingUsers.value = true;
  try {
    const result = await api.getUsers();
    users.value = result.data ?? [];
  } catch (error) {
    handleProtectedError(error);
  } finally {
    loadingUsers.value = false;
  }
}

async function submitRoom() {
  try {
    const payload = {
      computerId: toNullableNumber(roomForm.computerId),
      bedId: toNullableNumber(roomForm.bedId)
    };

    if (roomForm.id) {
      await api.updateRoom(roomForm.id, payload);
      showMessage("Room updated.");
    } else {
      await api.createRoom(payload);
      showMessage("Room created.");
    }

    clearRoomForm();
    await loadRooms();
  } catch (error) {
    handleProtectedError(error);
  }
}

async function submitThing() {
  try {
    if (!colorOptions.includes(thingForm.color)) {
      showMessage("Please choose one of the 7 colors.", "error");
      return;
    }

    const integerNumber = toNullableInteger(thingForm.number);
    if (Number.isNaN(integerNumber)) {
      showMessage("Number must be an integer.", "error");
      return;
    }

    const payload = {
      color: thingForm.color || null,
      price: toNullableNumber(thingForm.price),
      number: integerNumber,
      description: thingForm.description || null
    };

    if (thingForm.sourceId !== null) {
      await api.updateThing(thingForm.sourceId, {
        color: payload.color,
        price: payload.price,
        number: payload.number,
        description: payload.description
      });
      showMessage("Thing updated.");
    } else {
      await api.createThing(payload);
      showMessage("Thing created.");
    }

    clearThingForm();
    await loadThings();
  } catch (error) {
    handleProtectedError(error);
  }
}

async function submitUser() {
  try {
    if (!userForm.userName || !userForm.email || userForm.age === "") {
      showMessage("User name, email and age are required.", "error");
      return;
    }

    normalizeUserAge();
    const ageValue = Number(userForm.age);
    if (!Number.isInteger(ageValue) || ageValue < 0 || ageValue > 120) {
      showMessage("Age must be an integer between 0 and 120.", "error");
      return;
    }

    if (userForm.id) {
      if (userForm.password) {
        const passwordError = validatePassword(userForm.password);
        if (passwordError) {
          showMessage(passwordError, "error");
          return;
        }
      }

      await api.updateUser(userForm.id, {
        userName: userForm.userName,
        email: userForm.email,
        age: ageValue,
        isActive: userForm.isActive,
        newPassword: userForm.password || null
      });
      showMessage("User updated.");
    } else {
      const passwordError = validatePassword(userForm.password);
      if (passwordError) {
        showMessage(passwordError, "error");
        return;
      }

      await api.createUser({
        userName: userForm.userName,
        email: userForm.email,
        age: ageValue,
        password: userForm.password
      });
      showMessage("User created.");
    }

    clearUserForm();
    await loadUsers();
  } catch (error) {
    handleProtectedError(error);
  }
}

function editRoom(room) {
  roomForm.id = room.id;
  roomForm.computerId = room.computerId ?? null;
  roomForm.bedId = room.bedId ?? null;
  activeTab.value = "rooms";
}

function editThing(thing) {
  thingForm.sourceId = thing.id;
  thingForm.color = thing.color ?? "Red";
  thingForm.price = thing.price ?? "";
  thingForm.number = thing.number ?? "";
  thingForm.description = thing.description ?? "";
  activeTab.value = "things";
}

function editUser(user) {
  userForm.id = user.id;
  userForm.userName = user.userName;
  userForm.email = user.email;
  userForm.age = user.age;
  userForm.password = "";
  showUserPassword.value = false;
  userForm.isActive = user.isActive;
  activeTab.value = "users";
}

async function removeRoom(id) {
  try {
    await api.deleteRoom(id);
    if (selectedRoomThings.value?.room?.id === id) {
      selectedRoomThings.value = null;
    }
    showMessage("Room deleted.");
    await loadRooms();
  } catch (error) {
    handleProtectedError(error);
  }
}

async function removeThing(id) {
  try {
    await api.deleteThing(id);
    if (thingForm.sourceId === id) {
      clearThingForm();
    }
    showMessage("Thing deleted.");
    await loadThings();
  } catch (error) {
    handleProtectedError(error);
  }
}

async function removeUser(id) {
  try {
    await api.deleteUser(id);
    showMessage("User deleted.");
    await loadUsers();
  } catch (error) {
    handleProtectedError(error);
  }
}

async function inspectRoomThings(id) {
  roomThingsLoading.value = true;
  try {
    const result = await api.getRoomThings(id);
    selectedRoomThings.value = result.data;
    showMessage(`Loaded things for room #${id}.`, "info");
  } catch (error) {
    handleProtectedError(error);
  } finally {
    roomThingsLoading.value = false;
  }
}

async function refreshAll() {
  await loadOverview();
  if (!isAuthenticated.value) {
    clearProtectedData();
    return;
  }

  await Promise.all([loadRooms(), loadThings(), loadUsers()]);
}

function startLocalClock() {
  if (clockTimer) {
    return;
  }

  clockTimer = setInterval(() => {
    if (healthClockMs.value !== null) {
      healthClockMs.value += 1000;
    }
  }, 1000);
}

function stopLocalClock() {
  if (!clockTimer) {
    return;
  }
  clearInterval(clockTimer);
  clockTimer = null;
}

onMounted(async () => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    try {
      setAuthToken(token);
      await loadCurrentUser();
    } catch {
      logout(false);
    }
  }

  await refreshAll();
  startLocalClock();
});

onUnmounted(() => {
  stopLocalClock();
});
</script>

<template>
  <div class="shell">
    <aside class="sidebar">
      <div class="brand">
        <p class="eyebrow">Vue + ASP.NET Core</p>
        <h1>Demo Console</h1>
        <p class="brand-copy">Frontend and backend are separated. This console calls API endpoints directly.</p>
      </div>

      <nav class="nav">
        <button
          v-for="tab in visibleTabs"
          :key="tab.key"
          class="nav-link"
          :class="{ active: activeTab === tab.key }"
          @click="activeTab = tab.key"
        >
          {{ tab.label }}
        </button>
      </nav>

      <div class="sidebar-panel">
        <h3>Quick Stats</h3>
        <div class="mini-stats">
          <div><span>Rooms</span><strong>{{ roomCount }}</strong></div>
          <div><span>Things</span><strong>{{ thingCount }}</strong></div>
          <div><span>Users</span><strong>{{ userCount }}</strong></div>
        </div>
        <button class="ghost-button" @click="refreshAll">Refresh all</button>
      </div>

      <div class="sidebar-panel auth-box">
        <h3>Account</h3>
        <div v-if="!isAuthenticated" class="auth-form">
          <label>
            <span>Account</span>
            <input v-model="loginForm.userNameOrEmail" type="text" placeholder="User name or email" />
          </label>
          <label>
            <span>Password</span>
            <div class="input-with-action">
              <input
                v-model="loginForm.password"
                :type="showLoginPassword ? 'text' : 'password'"
                placeholder="Password"
              />
              <button
                class="input-action icon-action"
                type="button"
                :aria-label="showLoginPassword ? 'Hide password' : 'Show password'"
                @click="showLoginPassword = !showLoginPassword"
              >
                <svg viewBox="0 0 24 24" aria-hidden="true">
                  <path
                    d="M2.2 12C4.3 8.4 7.8 6 12 6s7.7 2.4 9.8 6c-2.1 3.6-5.6 6-9.8 6s-7.7-2.4-9.8-6z"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.8"
                    stroke-linejoin="round"
                  />
                  <circle cx="12" cy="12" r="3" fill="none" stroke="currentColor" stroke-width="1.8" />
                  <line
                    v-if="showLoginPassword"
                    x1="5"
                    y1="19"
                    x2="19"
                    y2="5"
                    stroke="currentColor"
                    stroke-width="1.8"
                    stroke-linecap="round"
                  />
                </svg>
              </button>
            </div>
          </label>
          <button class="primary-button" :disabled="authLoading" @click="login">
            {{ authLoading ? "Signing in..." : "Login" }}
          </button>
        </div>
        <div v-else class="auth-user">
          <p><strong>{{ authUser.userName }}</strong></p>
          <p class="auth-meta">{{ authUser.email }}</p>
          <button class="ghost-button logout-button" @click="logout()">Logout</button>
        </div>
      </div>
    </aside>

    <main class="content">
      <section class="hero">
        <div>
          <p class="eyebrow">Operations</p>
          <h2>Manage rooms, room-things relation, things and users</h2>
          <p class="hero-copy">
            <template v-if="isAuthenticated">You are logged in. Rooms, Things and Users are available.</template>
            <template v-else>Please login first. Without login, only Overview and Quick Stats are visible.</template>
          </p>
        </div>
        <div v-if="banner.message" class="banner" :class="banner.type">{{ banner.message }}</div>
      </section>

      <section v-if="activeTab === 'overview'" class="panel-grid">
        <article class="panel feature">
          <div class="panel-header">
            <div><p class="eyebrow">Health</p><h3>Service Status</h3></div>
            <span class="badge" :class="{ healthy: overview.health?.status === 'Healthy' }">
              {{ overview.health?.status ?? "Loading" }}
            </span>
          </div>
          <div class="metric-row">
            <div class="metric-card"><span>Version</span><strong>{{ overview.health?.version ?? "-" }}</strong></div>
            <div class="metric-card"><span>Time</span><strong>{{ formatDateTime(healthClockMs ?? overview.health?.time) }}</strong></div>
          </div>
        </article>

        <article class="panel">
          <div class="panel-header"><div><p class="eyebrow">Hello</p><h3>Greeting endpoint</h3></div></div>
          <p class="callout">{{ overview.hello?.hello ?? "Waiting for response..." }}</p>
        </article>
      </section>

      <section v-if="isAuthenticated && activeTab === 'rooms'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Rooms</p><h3>{{ roomForm.id ? `Edit #${roomForm.id}` : "Create Room" }}</h3></div>
            <button class="ghost-button" @click="clearRoomForm">Clear</button>
          </div>
          <div class="form-grid">
            <label>
              <span>Computer Thing</span>
              <select v-model="roomForm.computerId">
                <option :value="null">None</option>
                <option v-for="thing in things" :key="`computer-${thing.id}`" :value="thing.id">
                  {{ thingOptionLabel(thing) }}
                </option>
              </select>
            </label>
            <label>
              <span>Bed Thing</span>
              <select v-model="roomForm.bedId">
                <option :value="null">None</option>
                <option v-for="thing in things" :key="`bed-${thing.id}`" :value="thing.id">
                  {{ thingOptionLabel(thing) }}
                </option>
              </select>
            </label>
          </div>
          <div class="form-actions">
            <button class="primary-button" @click="submitRoom">{{ roomForm.id ? "Save" : "Create" }}</button>
            <button class="ghost-button" @click="loadRooms">Reload</button>
          </div>
        </article>

        <article class="panel list-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Room List</p><h3>Rooms</h3></div>
            <span class="badge">{{ loadingRooms ? "Loading" : `${roomCount} rows` }}</span>
          </div>
          <div class="table-wrap">
            <table>
              <thead><tr><th>Id</th><th>Computer Id</th><th>Bed Id</th><th>Actions</th></tr></thead>
              <tbody>
                <tr v-for="room in rooms" :key="room.id">
                  <td>#{{ room.id }}</td>
                  <td>{{ room.computerId ?? "-" }}</td>
                  <td>{{ room.bedId ?? "-" }}</td>
                  <td class="actions">
                    <button class="tiny-button" @click="inspectRoomThings(room.id)">Inspect things</button>
                    <button class="tiny-button" @click="editRoom(room)">Edit</button>
                    <button class="tiny-button danger" @click="removeRoom(room.id)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </article>

        <article class="panel detail-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Room Things</p><h3>Current room relation</h3></div>
            <span class="badge">{{ roomThingsLoading ? "Loading" : `${selectedThingCount} thing(s)` }}</span>
          </div>
          <div v-if="selectedRoomThings" class="thing-stack">
            <div class="selection-caption">
              Room #{{ selectedRoomThings.room.id }} | computerId={{ selectedRoomThings.room.computerId ?? "-" }} |
              bedId={{ selectedRoomThings.room.bedId ?? "-" }}
            </div>
            <div v-if="selectedRoomThings.things.length" class="thing-grid">
              <div v-for="thing in selectedRoomThings.things" :key="thing.id" class="thing-card">
                <div class="thing-top"><strong>Thing #{{ thing.id }}</strong><span>{{ thing.color || "No color" }}</span></div>
                <div class="thing-meta"><span>Price</span><strong>{{ formatPrice(thing.price) }}</strong></div>
                <div class="thing-meta"><span>Number</span><strong>{{ thing.number ?? "-" }}</strong></div>
                <div class="thing-meta"><span>Description</span><strong>{{ thing.description ?? "-" }}</strong></div>
              </div>
            </div>
            <div v-else class="empty-state">No linked thing found for this room.</div>
          </div>
          <div v-else class="empty-state">Click "Inspect things" in room list.</div>
        </article>
      </section>

      <section v-if="isAuthenticated && activeTab === 'things'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Things</p><h3>{{ thingForm.sourceId !== null ? `Edit #${thingForm.sourceId}` : "Create Thing" }}</h3></div>
            <button class="ghost-button" @click="clearThingForm">Clear</button>
          </div>
          <div class="form-grid">
            <label>
              <span>Color</span>
              <select v-model="thingForm.color">
                <option v-for="color in colorOptions" :key="color" :value="color">{{ color }}</option>
              </select>
            </label>
            <label><span>Price</span><input v-model="thingForm.price" type="number" step="0.01" /></label>
            <label><span>Number</span><input v-model="thingForm.number" type="number" step="1" /></label>
            <label style="grid-column: 1 / -1"><span>Description</span><input v-model="thingForm.description" type="text" /></label>
          </div>
          <div class="form-actions">
            <button class="primary-button" @click="submitThing">{{ thingForm.sourceId !== null ? "Save" : "Create" }}</button>
            <button class="ghost-button" @click="loadThings">Reload</button>
          </div>
        </article>

        <article class="panel list-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Thing List</p><h3>Things</h3></div>
            <span class="badge">{{ loadingThings ? "Loading" : `${thingCount} rows` }}</span>
          </div>
          <div class="table-wrap">
            <table>
              <thead><tr><th>Id</th><th>Color</th><th>Price</th><th>Number</th><th>Description</th><th>Actions</th></tr></thead>
              <tbody>
                <tr v-for="thing in things" :key="thing.id">
                  <td>#{{ thing.id }}</td>
                  <td>{{ thing.color ?? "-" }}</td>
                  <td>{{ formatPrice(thing.price) }}</td>
                  <td>{{ thing.number ?? "-" }}</td>
                  <td>{{ thing.description ?? "-" }}</td>
                  <td class="actions">
                    <button class="tiny-button" @click="editThing(thing)">Edit</button>
                    <button class="tiny-button danger" @click="removeThing(thing.id)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </article>
      </section>

      <section v-if="isAuthenticated && activeTab === 'users'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Users</p><h3>{{ userForm.id ? `Edit #${userForm.id}` : "Create User" }}</h3></div>
            <button class="ghost-button" @click="clearUserForm">Clear</button>
          </div>
          <div class="form-grid">
            <label><span>User Name</span><input v-model="userForm.userName" type="text" /></label>
            <label><span>Email</span><input v-model="userForm.email" type="email" /></label>
            <label class="age-field full-row">
              <span>Age</span>
              <div class="input-with-action">
                <input
                  v-model="userForm.age"
                  class="age-input"
                  type="number"
                  min="0"
                  max="120"
                  step="1"
                  inputmode="numeric"
                  placeholder="18"
                  @blur="normalizeUserAge"
                />
                <span class="input-addon">years</span>
              </div>
            </label>
            <label class="full-row">
              <span>{{ userForm.id ? "New Password (Optional)" : "Password" }}</span>
              <div class="input-with-action">
                <input
                  v-model="userForm.password"
                  :type="showUserPassword ? 'text' : 'password'"
                  :placeholder="userForm.id ? 'Leave empty to keep unchanged' : 'Enter password'"
                />
                <button
                  class="input-action icon-action"
                  type="button"
                  :aria-label="showUserPassword ? 'Hide password' : 'Show password'"
                  @click="showUserPassword = !showUserPassword"
                >
                  <svg viewBox="0 0 24 24" aria-hidden="true">
                    <path
                      d="M2.2 12C4.3 8.4 7.8 6 12 6s7.7 2.4 9.8 6c-2.1 3.6-5.6 6-9.8 6s-7.7-2.4-9.8-6z"
                      fill="none"
                      stroke="currentColor"
                      stroke-width="1.8"
                      stroke-linejoin="round"
                    />
                    <circle cx="12" cy="12" r="3" fill="none" stroke="currentColor" stroke-width="1.8" />
                    <line
                      v-if="showUserPassword"
                      x1="5"
                      y1="19"
                      x2="19"
                      y2="5"
                      stroke="currentColor"
                      stroke-width="1.8"
                      stroke-linecap="round"
                    />
                  </svg>
                </button>
              </div>
              <small class="hint">{{ passwordRuleText }}</small>
              <div class="password-rules" v-if="userForm.password">
                <span :class="{ ok: passwordChecks.length }">8-64 chars</span>
                <span :class="{ ok: passwordChecks.upper }">uppercase</span>
                <span :class="{ ok: passwordChecks.lower }">lowercase</span>
                <span :class="{ ok: passwordChecks.digit }">number</span>
                <span :class="{ ok: passwordChecks.special }">special</span>
                <span :class="{ ok: passwordChecks.noSpace }">no spaces</span>
              </div>
            </label>
            <label class="switch-field"><span>Is Active</span><input v-model="userForm.isActive" type="checkbox" /></label>
          </div>
          <div class="form-actions">
            <button class="primary-button" @click="submitUser">{{ userForm.id ? "Save" : "Create" }}</button>
            <button class="ghost-button" @click="loadUsers">Reload</button>
          </div>
        </article>

        <article class="panel list-panel">
          <div class="panel-header">
            <div><p class="eyebrow">User List</p><h3>Users</h3></div>
            <span class="badge">{{ loadingUsers ? "Loading" : `${userCount} rows` }}</span>
          </div>
          <div class="table-wrap">
            <table>
              <thead><tr><th>Id</th><th>Name</th><th>Email</th><th>Age</th><th>Status</th><th>Actions</th></tr></thead>
              <tbody>
                <tr v-for="user in users" :key="user.id">
                  <td>#{{ user.id }}</td>
                  <td>{{ user.userName }}</td>
                  <td>{{ user.email }}</td>
                  <td>{{ user.age }}</td>
                  <td><span class="status-dot" :class="{ off: !user.isActive }"></span>{{ user.isActive ? "Active" : "Inactive" }}</td>
                  <td class="actions">
                    <button class="tiny-button" @click="editUser(user)">Edit</button>
                    <button class="tiny-button danger" @click="removeUser(user.id)">Delete</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </article>
      </section>
    </main>
  </div>
</template>
