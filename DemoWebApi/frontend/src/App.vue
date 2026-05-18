<script setup>
import { computed, onMounted, onUnmounted, reactive, ref } from "vue";
import { api } from "./api/client";

const tabs = [
  { key: "overview", label: "Overview" },
  { key: "rooms", label: "Rooms" },
  { key: "things", label: "Things" },
  { key: "users", label: "Users" }
];
const colorOptions = ["Red", "Orange", "Yellow", "Green", "Blue", "Indigo", "Violet"];

const activeTab = ref("overview");
const banner = reactive({ type: "info", message: "" });

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
const userForm = reactive({ id: null, userName: "", email: "", age: "", isActive: true });

const selectedRoomThings = ref(null);
let clockTimer = null;
const healthClockMs = ref(null);

const roomCount = computed(() => rooms.value.length);
const thingCount = computed(() => things.value.length);
const userCount = computed(() => users.value.length);
const selectedThingCount = computed(() => selectedRoomThings.value?.things?.length ?? 0);

function showMessage(message, type = "success") {
  banner.type = type;
  banner.message = message;
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
  userForm.isActive = true;
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

async function loadRooms() {
  loadingRooms.value = true;
  try {
    const result = await api.getRooms();
    rooms.value = result.data ?? [];
  } catch (error) {
    showMessage(error.message, "error");
  } finally {
    loadingRooms.value = false;
  }
}

async function loadThings() {
  loadingThings.value = true;
  try {
    const result = await api.getThings();
    things.value = result.data ?? [];
  } catch (error) {
    showMessage(error.message, "error");
  } finally {
    loadingThings.value = false;
  }
}

async function loadUsers() {
  loadingUsers.value = true;
  try {
    const result = await api.getUsers();
    users.value = result.data ?? [];
  } catch (error) {
    showMessage(error.message, "error");
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
    showMessage(error.message, "error");
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
    showMessage(error.message, "error");
  }
}

async function submitUser() {
  try {
    if (!userForm.userName || !userForm.email || userForm.age === "") {
      showMessage("User name, email and age are required.", "error");
      return;
    }

    if (userForm.id) {
      await api.updateUser(userForm.id, {
        userName: userForm.userName,
        email: userForm.email,
        age: Number(userForm.age),
        isActive: userForm.isActive
      });
      showMessage("User updated.");
    } else {
      await api.createUser({
        userName: userForm.userName,
        email: userForm.email,
        age: Number(userForm.age)
      });
      showMessage("User created.");
    }

    clearUserForm();
    await loadUsers();
  } catch (error) {
    showMessage(error.message, "error");
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
    showMessage(error.message, "error");
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
    showMessage(error.message, "error");
  }
}

async function removeUser(id) {
  try {
    await api.deleteUser(id);
    showMessage("User deleted.");
    await loadUsers();
  } catch (error) {
    showMessage(error.message, "error");
  }
}

async function inspectRoomThings(id) {
  roomThingsLoading.value = true;
  try {
    const result = await api.getRoomThings(id);
    selectedRoomThings.value = result.data;
    showMessage(`Loaded things for room #${id}.`, "info");
  } catch (error) {
    showMessage(error.message, "error");
  } finally {
    roomThingsLoading.value = false;
  }
}

async function refreshAll() {
  await Promise.all([loadOverview(), loadRooms(), loadThings(), loadUsers()]);
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

onMounted(() => {
  refreshAll();
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
          v-for="tab in tabs"
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
    </aside>

    <main class="content">
      <section class="hero">
        <div>
          <p class="eyebrow">Operations</p>
          <h2>Manage rooms, room-things relation, things and users</h2>
          <p class="hero-copy">Now includes full CRUD for `thing`, plus room-to-things inspection.</p>
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

      <section v-if="activeTab === 'rooms'" class="workspace">
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

      <section v-if="activeTab === 'things'" class="workspace">
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

      <section v-if="activeTab === 'users'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div><p class="eyebrow">Users</p><h3>{{ userForm.id ? `Edit #${userForm.id}` : "Create User" }}</h3></div>
            <button class="ghost-button" @click="clearUserForm">Clear</button>
          </div>
          <div class="form-grid">
            <label><span>User Name</span><input v-model="userForm.userName" type="text" /></label>
            <label><span>Email</span><input v-model="userForm.email" type="email" /></label>
            <label><span>Age</span><input v-model="userForm.age" type="number" /></label>
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
