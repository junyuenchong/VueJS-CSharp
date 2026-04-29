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
        getErrorMessage(error, fallbackMessage) {
            const message =
                error?.response?.data?.detail ||
                error?.response?.data?.message ||
                error?.response?.data?.title ||
                (typeof error?.response?.data === "string" ? error.response.data : null) ||
                error?.message;

            if (typeof message === "string" && message.trim().length > 0) {
                return message;
            }

            return fallbackMessage;
        },
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
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Failed to load products"));
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
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Failed to load more products"));
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
            try {
                if (payload.id) {
                    await productsApi().update(payload.id, payload);
                } else {
                    await productsApi().create(payload);
                }
                this.editing = null;
                await this.reload();
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Failed to save product"));
            }
        },
        async remove(id) {
            try {
                await productsApi().remove(id);
                await this.reload();
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Failed to delete product"));
            }
        },
    },
});

