<template>
  <div class="toolbar">
    <input
      :value="modelValue"
      placeholder="Search name/description..."
      @input="onInput"
    />
  </div>
</template>

<script setup>
const props = defineProps({
  modelValue: { type: String, default: "" },
});

const emit = defineEmits(["update:modelValue", "search"]);

function onInput(event) {
  const value = event.target.value;
  emit("update:modelValue", value);
  emit("search", value);
}
</script>

<style scoped>
.toolbar {
  margin-top: var(--ui-space-3);
}

.toolbar input {
  width: 100%;
  padding: var(--ui-space-2);
  border-radius: var(--ui-radius-sm);
  min-height: var(--ui-control-height);
}
</style>

