import { storeToRefs } from "pinia";
import { useProductsStore } from "@/stores/productsStore";

export function useProducts() {
    const store = useProductsStore();

    const { items, nextCursor, loading, editing, search } = storeToRefs(store);

    return {
        items,
        nextCursor,
        loading,
        editing,
        search,
        reload: store.reload,
        loadMore: store.loadMore,
        startEdit: store.startEdit,
        cancelEdit: store.cancelEdit,
        save: store.save,
        remove: store.remove,
    };
}
