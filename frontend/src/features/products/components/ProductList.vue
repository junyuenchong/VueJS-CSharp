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
          <td>{{ p.id }}</td>
          <td>{{ p.name }}</td>
          <td>{{ p.price }}</td>
          <td>{{ p.stock }}</td>
          <td>{{ p.description }}</td>
          <td>
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
th {
  color: #000;
}
</style>

