<template>
  <section>
    <h2>Products</h2>
    <p v-if="errorMessage" class="error">{{ errorMessage }}</p>

    <ProductForm :product="editing" @save="save" @cancel="cancelEdit" />

    <ProductSearch v-model="search" @search="reload" />

    <ProductList :items="items" @edit="edit" @remove="removeWithConfirm" />

    <div class="pager">
      <button :disabled="loading || !nextCursor" @click="loadMore">
        {{ loading ? "..." : "Load more" }}
      </button>
    </div>
  </section>
</template>

<script setup>
import ProductForm from "../components/ProductForm.vue";
import ProductSearch from "../components/ProductSearch.vue";
import ProductList from "../components/ProductList.vue";
import { useProducts } from "../composables/useProducts";

const {
  items,
  nextCursor,
  loading,
  errorMessage,
  editing,
  search,
  reload,
  loadMore,
  startEdit,
  cancelEdit,
  save,
  remove,
} = useProducts();

function edit(p) {
  startEdit(p);
}

async function removeWithConfirm(id) {
  if (!confirm("Delete this product?")) return;
  await remove(id);
}

reload();
</script>

<style scoped>
.pager {
  margin-top: 1rem;
}
.error {
  color: #b00020;
  margin: 0.5rem 0;
}
</style>

