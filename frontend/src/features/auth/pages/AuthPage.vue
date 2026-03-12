<template>
  <section>
    <Login v-if="view === 'login'" @authed="refreshAuth" @switch="setView($event)" />
    <Register v-else @authed="refreshAuth" @switch="setView($event)" />
  </section>
</template>

<script setup>
import Login from "../components/Login.vue";
import Register from "../components/Register.vue";
import { useAuth } from "../composables/useAuth";

const { view, setView, refreshAuth } = useAuth();
</script>

