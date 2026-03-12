<template>
  <form class="form" @submit.prevent="submit">
    <div class="grid">
      <input v-model="model.name" placeholder="Name" required />
      <input v-model.number="model.price" type="number" placeholder="Price" min="0" step="0.01" required />
      <input v-model.number="model.stock" type="number" placeholder="Stock" min="0" required />
    </div>
    <textarea v-model="model.description" placeholder="Description"></textarea>

    <div class="actions">
      <button type="submit">{{ isEdit ? "Update" : "Create" }}</button>
      <button v-if="isEdit" type="button" class="secondary" @click="$emit('cancel')">Cancel</button>
    </div>
  </form>
</template>

<script setup>
import { computed, reactive, watch } from "vue";

const props = defineProps({
  product: { type: Object, default: null },
});

const emit = defineEmits(["save", "cancel"]);

const model = reactive({
  id: 0,
  name: "",
  price: 0,
  stock: 0,
  description: "",
});

const isEdit = computed(() => !!props.product?.id);

watch(
  () => props.product,
  (p) => {
    model.id = p?.id || 0;
    model.name = p?.name || "";
    model.price = p?.price ?? 0;
    model.stock = p?.stock ?? 0;
    model.description = p?.description || "";
  },
  { immediate: true }
);

function submit() {
  emit("save", { ...model });
}
</script>

<style scoped>
.form {
  display: grid;
  gap: 0.75rem;
  margin-top: 1rem;
}
.grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: 0.5rem;
}
textarea {
  min-height: 80px;
}
.actions {
  display: flex;
  gap: 0.5rem;
  flex-wrap: wrap;
}
.secondary {
  background: #f3f3f3;
}
@media (max-width: 600px) {
  .grid {
    grid-template-columns: 1fr;
  }
}
</style>

