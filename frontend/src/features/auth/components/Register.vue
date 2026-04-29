<template>
  <section class="card">
    <h2>Register</h2>
    <form @submit.prevent="submit">
      <input v-model="email" type="email" placeholder="Email" required />
      <input v-model="password" type="password" placeholder="Password (min 6)" required />
      <button type="submit" :disabled="loading">{{ loading ? "..." : "Register" }}</button>
    </form>
    <p class="hint">
      Have an account?
      <button class="link" type="button" @click="$emit('switch', 'login')">Login</button>
    </p>
  </section>
</template>

<script setup>
import { ref } from "vue";
import { useAuth } from "../composables/useAuth";

const emit = defineEmits(["authed", "switch"]);
const { register } = useAuth();

const email = ref("");
const password = ref("");
const loading = ref(false);

function getErrorMessage(error) {
  const message =
    error?.response?.data?.detail ||
    error?.response?.data?.message ||
    error?.response?.data?.title ||
    (typeof error?.response?.data === "string" ? error.response.data : null) ||
    error?.message;

  if (typeof message === "string" && message.trim().length > 0) {
    return message;
  }

  return "Register failed";
}

async function submit() {
  loading.value = true;
  try {
    await register({
      email: email.value,
      password: password.value,
    });
    emit("authed");
  } catch (e) {
    alert(getErrorMessage(e));
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.card {
  width: 100%;
  max-width: 420px;
  margin: var(--ui-space-3) auto;
  padding: var(--ui-space-3);
  border: 1px solid var(--ui-border);
  border-radius: var(--ui-radius-md);
  background: var(--ui-surface);
}
form {
  display: grid;
  gap: var(--ui-space-2);
}
input,
button {
  width: 100%;
  padding: var(--ui-space-2);
  font-size: 1rem;
  border-radius: var(--ui-radius-sm);
  min-height: var(--ui-control-height);
}
.hint {
  margin-top: var(--ui-space-2);
  color: var(--ui-muted);
}
.link {
  padding: 0;
  border: none;
  background: none;
  color: var(--ui-primary);
  cursor: pointer;
}

@media (max-width: 640px) {
  .card {
    margin: var(--ui-space-1) auto;
    padding: 0.875rem;
  }
}
</style>

