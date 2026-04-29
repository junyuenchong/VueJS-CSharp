<template>
  <main class="container">
    <header class="top">
      <h1>CRUD App</h1>
      <div v-if="token" class="right">
        <span class="email">{{ email }}</span>
        <button @click="handleLogout">Logout</button>
      </div>
    </header>

    <div v-if="!token">
      <AuthPage />
    </div>

    <div v-else>
      <ProductsPage />
    </div>
  </main>
</template>

<script setup>
import ProductsPage from "./features/products/pages/ProductsPage.vue";
import AuthPage from "./features/auth/pages/AuthPage.vue";
import { onMounted } from "vue";
import { useAuth } from "./features/auth/composables/useAuth";

const { token, email, bootstrap, logout } = useAuth();

function getErrorMessage(error, fallbackMessage) {
  const message =
    error?.response?.data?.detail ||
    error?.response?.data?.message ||
    error?.response?.data?.title ||
    (typeof error?.response?.data === "string" ? error.response.data : null) ||
    error?.message;

  if (typeof message === "string" && message.trim().length > 0) {
    return message;
  }

  return fallbackMessage;
}

async function handleLogout() {
  try {
    await logout();
  } catch (error) {
    alert(getErrorMessage(error, "Logout failed"));
  }
}

onMounted(async () => {
  try {
    await bootstrap();
  } catch (error) {
    alert(getErrorMessage(error, "Failed to initialize app"));
  }
});
</script>

<style>
:root {
  --ui-bg: #fafafa;
  --ui-surface: #ffffff;
  --ui-border: #e5e7eb;
  --ui-muted: #555;
  --ui-primary: #007bff;
  --ui-danger: #b00020;
  --ui-radius-sm: 8px;
  --ui-radius-md: 10px;
  --ui-control-height: 42px;
  --ui-space-1: 0.5rem;
  --ui-space-2: 0.75rem;
  --ui-space-3: 1rem;
}

* {
  box-sizing: border-box;
}
body {
  margin: 0;
  font-family: system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif;
  background: var(--ui-bg);
}
.container {
  max-width: 900px;
  margin: 1rem auto;
  padding: 0 0.875rem 1.5rem;
}
.top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
}
.right {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}
.email {
  color: var(--ui-muted);
  font-size: 0.95rem;
  word-break: break-word;
}
table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}
th,
td {
  border: 1px solid var(--ui-border);
  padding: 8px;
}
th {
  background: #f7f7f7;
  text-align: left;
}
input,
button {
  padding: 8px;
  min-height: var(--ui-control-height);
}
button {
  cursor: pointer;
}
.row {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}

@media (max-width: 640px) {
  .container {
    margin-top: 0.5rem;
    padding: 0 0.75rem 1rem;
  }

  .top {
    flex-direction: column;
    align-items: flex-start;
    gap: 0.5rem;
  }

  .top h1 {
    margin: 0;
    font-size: 1.35rem;
  }

  .right {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
