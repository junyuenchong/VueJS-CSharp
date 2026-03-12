<template>
  <section class="card">
    <h2>Login</h2>
    <form @submit.prevent="submit">
      <input v-model="email" type="email" placeholder="Email" required />
      <input v-model="password" type="password" placeholder="Password" required />
      <button type="submit" :disabled="loading">{{ loading ? "..." : "Login" }}</button>
    </form>
    <p class="hint">
      No account?
      <button class="link" type="button" @click="$emit('switch', 'register')">Register</button>
    </p>
  </section>
</template>

<script setup>
import { ref } from "vue";
import { useAuth } from "../composables/useAuth";

const emit = defineEmits(["authed", "switch"]);
const { login } = useAuth();

const email = ref("");
const password = ref("");
const loading = ref(false);

async function submit() {
  loading.value = true;
  try {
    await login({
      email: email.value,
      password: password.value,
    });
    emit("authed");
  } catch (e) {
    const msg = e?.response?.data || e?.message || "Login failed";
    alert(msg);
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
.card {
  max-width: 380px;
  margin: 2rem auto;
  padding: 1rem;
  border: 1px solid #eee;
  border-radius: 10px;
}
form {
  display: grid;
  gap: 0.75rem;
}
input,
button {
  padding: 0.75rem;
  font-size: 1rem;
}
.hint {
  margin-top: 0.75rem;
  color: #555;
}
.link {
  padding: 0;
  border: none;
  background: none;
  color: #007bff;
  cursor: pointer;
}
</style>

