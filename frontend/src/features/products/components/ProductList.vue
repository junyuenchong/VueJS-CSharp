<template>
  <div class="tableWrap">
    <table>
      <thead>
        <tr>
          <th>Id</th>
          <th>Name</th>
          <th>Price</th>
          <th>Stock</th>
          <th>Description</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="p in items" :key="p.id">
          <td data-label="Id">{{ p.id }}</td>
          <td data-label="Name">{{ p.name }}</td>
          <td data-label="Price">{{ p.price }}</td>
          <td data-label="Stock">{{ p.stock }}</td>
          <td data-label="Description">{{ p.description }}</td>
          <td data-label="Actions">
            <button @click="$emit('edit', p)">Edit</button>
            <button style="margin-left: 5px" @click="$emit('remove', p.id)">Delete</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
defineProps({
  items: { type: Array, default: () => [] },
});

defineEmits(["edit", "remove"]);
</script>

<style scoped>
.tableWrap {
  overflow-x: auto;
}
table {
  width: 100%;
  border-collapse: collapse;
}
th,
td {
  border: 1px solid var(--ui-border);
  padding: 0.6rem;
  vertical-align: top;
}
th {
  color: #000;
  background: #f7f7f7;
  text-align: left;
}

@media (max-width: 700px) {
  thead {
    display: none;
  }

  table,
  tbody,
  tr,
  td {
    display: block;
    width: 100%;
  }

  tr {
    border: 1px solid var(--ui-border);
    border-radius: var(--ui-radius-md);
    margin-bottom: 0.75rem;
    background: var(--ui-surface);
    padding: 0.25rem 0;
  }

  td {
    border: none;
    padding: 0.4rem 0.75rem;
  }

  td::before {
    content: attr(data-label) ": ";
    font-weight: 600;
    color: #333;
  }

  td[data-label="Actions"] button {
    width: auto;
    min-width: 84px;
  }
}
</style>

