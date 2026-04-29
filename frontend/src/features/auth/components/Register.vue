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

