<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { api } from "./api/client";

const tabs = [
  { key: "overview", label: "服务总览" },
  { key: "rooms", label: "房间管理" },
  { key: "users", label: "用户管理" }
];

const activeTab = ref("overview");
const banner = reactive({
  type: "info",
  message: ""
});

const overview = reactive({
  health: null,
  hello: null,
  loading: false
});

const rooms = ref([]);
const users = ref([]);
const loadingRooms = ref(false);
const loadingUsers = ref(false);
const roomThingsLoading = ref(false);

const roomForm = reactive({
  id: null,
  computerId: "",
  bedId: ""
});

const userForm = reactive({
  id: null,
  userName: "",
  email: "",
  age: "",
  isActive: true
});

const selectedRoomThings = ref(null);

const roomCount = computed(() => rooms.value.length);
const userCount = computed(() => users.value.length);
const thingCount = computed(() => selectedRoomThings.value?.things?.length ?? 0);

function showMessage(message, type = "success") {
  banner.message = message;
  banner.type = type;
}

function clearRoomForm() {
  roomForm.id = null;
  roomForm.computerId = "";
  roomForm.bedId = "";
}

function clearUserForm() {
  userForm.id = null;
  userForm.userName = "";
  userForm.email = "";
  userForm.age = "";
  userForm.isActive = true;
}

function toNullableNumber(value) {
  return value === "" || value === null || value === undefined ? null : Number(value);
}

async function loadOverview() {
  overview.loading = true;
  try {
    const [health, hello] = await Promise.all([api.getHealth(), api.getHello()]);
    overview.health = health;
    overview.hello = hello;
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
      showMessage("房间更新成功");
    } else {
      await api.createRoom(payload);
      showMessage("房间创建成功");
    }

    clearRoomForm();
    await loadRooms();
  } catch (error) {
    showMessage(error.message, "error");
  }
}

async function submitUser() {
  try {
    if (!userForm.userName || !userForm.email || userForm.age === "") {
      showMessage("请先填写完整的用户信息", "error");
      return;
    }

    if (userForm.id) {
      await api.updateUser(userForm.id, {
        userName: userForm.userName,
        email: userForm.email,
        age: Number(userForm.age),
        isActive: userForm.isActive
      });
      showMessage("用户更新成功");
    } else {
      await api.createUser({
        userName: userForm.userName,
        email: userForm.email,
        age: Number(userForm.age)
      });
      showMessage("用户创建成功");
    }

    clearUserForm();
    await loadUsers();
  } catch (error) {
    showMessage(error.message, "error");
  }
}

function editRoom(room) {
  roomForm.id = room.id;
  roomForm.computerId = room.computerId ?? "";
  roomForm.bedId = room.bedId ?? "";
  activeTab.value = "rooms";
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
    showMessage("房间已删除");
    await loadRooms();
  } catch (error) {
    showMessage(error.message, "error");
  }
}

async function removeUser(id) {
  try {
    await api.deleteUser(id);
    showMessage("用户已删除");
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
    showMessage(`已加载房间 ${id} 的物品`, "info");
  } catch (error) {
    showMessage(error.message, "error");
  } finally {
    roomThingsLoading.value = false;
  }
}

async function refreshAll() {
  await Promise.all([loadOverview(), loadRooms(), loadUsers()]);
}

onMounted(() => {
  refreshAll();
});
</script>

<template>
  <div class="shell">
    <aside class="sidebar">
      <div class="brand">
        <p class="eyebrow">Vue + Web API</p>
        <h1>Demo Console</h1>
        <p class="brand-copy">前后端分离的管理台，直接联调你的 ASP.NET Core API。</p>
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
        <h3>快速统计</h3>
        <div class="mini-stats">
          <div>
            <span>房间</span>
            <strong>{{ roomCount }}</strong>
          </div>
          <div>
            <span>用户</span>
            <strong>{{ userCount }}</strong>
          </div>
          <div>
            <span>已选物品</span>
            <strong>{{ thingCount }}</strong>
          </div>
        </div>
        <button class="ghost-button" @click="refreshAll">刷新全部数据</button>
      </div>
    </aside>

    <main class="content">
      <section class="hero">
        <div>
          <p class="eyebrow">Operations Center</p>
          <h2>把接口直接变成可操作的管理界面</h2>
          <p class="hero-copy">
            当前前端覆盖健康检查、房间管理、房间物品查看和用户管理，适合本地开发联调。
          </p>
        </div>

        <div v-if="banner.message" class="banner" :class="banner.type">
          {{ banner.message }}
        </div>
      </section>

      <section v-if="activeTab === 'overview'" class="panel-grid">
        <article class="panel feature">
          <div class="panel-header">
            <div>
              <p class="eyebrow">服务健康</p>
              <h3>系统状态</h3>
            </div>
            <span class="badge" :class="{ healthy: overview.health?.status === 'Healthy' }">
              {{ overview.health?.status ?? "加载中" }}
            </span>
          </div>
          <div class="metric-row">
            <div class="metric-card">
              <span>版本</span>
              <strong>{{ overview.health?.version ?? "-" }}</strong>
            </div>
            <div class="metric-card">
              <span>时间</span>
              <strong>{{ overview.health?.time ?? "-" }}</strong>
            </div>
          </div>
        </article>

        <article class="panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">欢迎信息</p>
              <h3>Hello 接口</h3>
            </div>
          </div>
          <p class="callout">{{ overview.hello?.hello ?? "等待接口响应..." }}</p>
        </article>

        <article class="panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">数据面板</p>
              <h3>当前资源数量</h3>
            </div>
          </div>
          <div class="metric-row">
            <div class="metric-card accent-blue">
              <span>房间</span>
              <strong>{{ roomCount }}</strong>
            </div>
            <div class="metric-card accent-gold">
              <span>用户</span>
              <strong>{{ userCount }}</strong>
            </div>
          </div>
        </article>
      </section>

      <section v-if="activeTab === 'rooms'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">Rooms</p>
              <h3>{{ roomForm.id ? `编辑房间 #${roomForm.id}` : "新建房间" }}</h3>
            </div>
            <button class="ghost-button" @click="clearRoomForm">清空表单</button>
          </div>

          <div class="form-grid">
            <label>
              <span>Computer Id</span>
              <input v-model="roomForm.computerId" type="number" placeholder="例如 101" />
            </label>
            <label>
              <span>Bed Id</span>
              <input v-model="roomForm.bedId" type="number" placeholder="例如 202" />
            </label>
          </div>

          <div class="form-actions">
            <button class="primary-button" @click="submitRoom">
              {{ roomForm.id ? "保存修改" : "创建房间" }}
            </button>
            <button class="ghost-button" @click="loadRooms">刷新房间列表</button>
          </div>
        </article>

        <article class="panel list-panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">Room List</p>
              <h3>房间列表</h3>
            </div>
            <span class="badge">{{ loadingRooms ? "加载中" : `${roomCount} 条` }}</span>
          </div>

          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Id</th>
                  <th>Computer Id</th>
                  <th>Bed Id</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="room in rooms" :key="room.id">
                  <td>#{{ room.id }}</td>
                  <td>{{ room.computerId ?? "-" }}</td>
                  <td>{{ room.bedId ?? "-" }}</td>
                  <td class="actions">
                    <button class="tiny-button" @click="inspectRoomThings(room.id)">看物品</button>
                    <button class="tiny-button" @click="editRoom(room)">编辑</button>
                    <button class="tiny-button danger" @click="removeRoom(room.id)">删除</button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </article>

        <article class="panel detail-panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">Room Things</p>
              <h3>房间内物品</h3>
            </div>
            <span class="badge">{{ roomThingsLoading ? "查询中" : `${thingCount} 件` }}</span>
          </div>

          <div v-if="selectedRoomThings" class="thing-stack">
            <div class="selection-caption">
              房间 #{{ selectedRoomThings.room.id }} · computerId={{ selectedRoomThings.room.computerId ?? "-" }} ·
              bedId={{ selectedRoomThings.room.bedId ?? "-" }}
            </div>

            <div v-if="selectedRoomThings.things.length" class="thing-grid">
              <div v-for="thing in selectedRoomThings.things" :key="thing.id" class="thing-card">
                <div class="thing-top">
                  <strong>Thing #{{ thing.id }}</strong>
                  <span>{{ thing.color || "未设置颜色" }}</span>
                </div>
                <div class="thing-meta">
                  <span>价格</span>
                  <strong>{{ thing.price ?? "-" }}</strong>
                </div>
                <div class="thing-meta">
                  <span>数量</span>
                  <strong>{{ thing.number ?? "-" }}</strong>
                </div>
              </div>
            </div>
            <div v-else class="empty-state">这个房间目前没有查到关联物品。</div>
          </div>

          <div v-else class="empty-state">点击房间列表里的“看物品”即可查看该房间关联的 thing。</div>
        </article>
      </section>

      <section v-if="activeTab === 'users'" class="workspace">
        <article class="panel form-panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">Users</p>
              <h3>{{ userForm.id ? `编辑用户 #${userForm.id}` : "新建用户" }}</h3>
            </div>
            <button class="ghost-button" @click="clearUserForm">清空表单</button>
          </div>

          <div class="form-grid">
            <label>
              <span>用户名</span>
              <input v-model="userForm.userName" type="text" placeholder="请输入用户名" />
            </label>
            <label>
              <span>邮箱</span>
              <input v-model="userForm.email" type="email" placeholder="name@example.com" />
            </label>
            <label>
              <span>年龄</span>
              <input v-model="userForm.age" type="number" placeholder="18" />
            </label>
            <label class="switch-field">
              <span>是否启用</span>
              <input v-model="userForm.isActive" type="checkbox" />
            </label>
          </div>

          <div class="form-actions">
            <button class="primary-button" @click="submitUser">
              {{ userForm.id ? "保存修改" : "创建用户" }}
            </button>
            <button class="ghost-button" @click="loadUsers">刷新用户列表</button>
          </div>
        </article>

        <article class="panel list-panel">
          <div class="panel-header">
            <div>
              <p class="eyebrow">User List</p>
              <h3>用户列表</h3>
            </div>
            <span class="badge">{{ loadingUsers ? "加载中" : `${userCount} 条` }}</span>
          </div>

          <div class="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Id</th>
                  <th>用户名</th>
                  <th>邮箱</th>
                  <th>年龄</th>
                  <th>状态</th>
                  <th>操作</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="user in users" :key="user.id">
                  <td>#{{ user.id }}</td>
                  <td>{{ user.userName }}</td>
                  <td>{{ user.email }}</td>
                  <td>{{ user.age }}</td>
                  <td>
                    <span class="status-dot" :class="{ off: !user.isActive }"></span>
                    {{ user.isActive ? "启用" : "停用" }}
                  </td>
                  <td class="actions">
                    <button class="tiny-button" @click="editUser(user)">编辑</button>
                    <button class="tiny-button danger" @click="removeUser(user.id)">删除</button>
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
