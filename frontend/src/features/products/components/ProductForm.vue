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
  gap: var(--ui-space-2);
  margin-top: var(--ui-space-3);
}
.grid {
  display: grid;
  grid-template-columns: 1fr 1fr 1fr;
  gap: var(--ui-space-1);
}
input,
textarea,
button {
  width: 100%;
  border-radius: var(--ui-radius-sm);
  padding: var(--ui-space-2);
  min-height: var(--ui-control-height);
}
textarea {
  min-height: 80px;
}
.actions {
  display: flex;
  gap: var(--ui-space-1);
  flex-wrap: wrap;
}
.secondary {
  background: #f3f3f3;
}
@media (max-width: 600px) {
  .grid {
    grid-template-columns: 1fr;
  }

  .actions {
    flex-direction: column;
  }
}
</style>

