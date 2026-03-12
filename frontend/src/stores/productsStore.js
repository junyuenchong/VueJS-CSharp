import { defineStore } from "pinia";
import { productsApi } from "@/features/products/api/productsApi";

export const useProductsStore = defineStore("products", {
    state: () => ({
        items: [],
        nextCursor: null,
        loading: false,
        editing: null,
        search: "",
    }),
    actions: {
        async reload() {
            this.loading = true;
            try {
                const { data } = await productsApi().list({
                    limit: 20,
                    cursor: null,
                    search: this.search || null,
                });
                this.items = data.items || [];
                this.nextCursor = data.nextCursor || null;
            } finally {
                this.loading = false;
            }
        },
        async loadMore() {
            if (!this.nextCursor) return;
            this.loading = true;
            try {
                const { data } = await productsApi().list({
                    limit: 20,
                    cursor: this.nextCursor,
                    search: this.search || null,
                });
                this.items = [...this.items, ...(data.items || [])];
                this.nextCursor = data.nextCursor || null;
            } finally {
                this.loading = false;
            }
        },
        startEdit(p) {
            this.editing = { ...p };
        },
        cancelEdit() {
            this.editing = null;
        },
        async save(payload) {
            if (payload.id) {
                await productsApi().update(payload.id, payload);
            } else {
                await productsApi().create(payload);
            }
            this.editing = null;
            await this.reload();
        },
        async remove(id) {
            await productsApi().remove(id);
            await this.reload();
        },
    },
});

