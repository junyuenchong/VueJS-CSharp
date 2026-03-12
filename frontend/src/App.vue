<template>
  <main class="container">
    <header class="top">
      <h1>CRUD App</h1>
      <div v-if="token" class="right">
        <span class="email">{{ email }}</span>
        <button @click="logout">Logout</button>
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

onMounted(async () => {
  await bootstrap();
});
</script>

<style>
* {
  box-sizing: border-box;
}
body {
  margin: 0;
  font-family: system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif;
}
.container {
  max-width: 900px;
  margin: 2rem auto;
  padding: 0 1rem;
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
  color: #555;
  font-size: 0.95rem;
}
table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 1rem;
}
th,
td {
  border: 1px solid #ddd;
  padding: 8px;
}
th {
  background: #f7f7f7;
  text-align: left;
}
input,
button {
  padding: 8px;
}
button {
  cursor: pointer;
}
.row {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}
</style>
