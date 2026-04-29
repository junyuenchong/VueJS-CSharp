import { computed, ref } from "vue";
import { useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/vue-query";
import { productsApi } from "@/features/products/api/productsApi";

export function useProducts() {
    const queryClient = useQueryClient();
    const search = ref("");
    const editing = ref(null);
    const errorMessage = ref("");
    const api = productsApi();

    function getErrorMessage(error, fallbackMessage) {
        const message =
            error?.response?.data?.detail ||
            error?.response?.data?.message ||
            error?.response?.data?.title ||
            error?.message;

        if (typeof message === "string" && message.trim().length > 0) {
            return message;
        }

        return fallbackMessage;
    }

    const productsQuery = useInfiniteQuery({
        queryKey: computed(() => ["products", search.value.trim()]),
        initialPageParam: null,
        queryFn: async ({ pageParam }) => {
            try {
                errorMessage.value = "";
                const res = await api.list({
                    limit: 20,
                    cursor: pageParam,
                    search: search.value.trim() || null,
                });
                return res.data;
            } catch (error) {
                errorMessage.value = getErrorMessage(error, "Failed to load products.");
                return { items: [], nextCursor: null };
            }
        },
        getNextPageParam: (lastPage) => lastPage?.nextCursor ?? undefined,
    });

    const saveMutation = useMutation({
        mutationFn: async (payload) => {
            if (payload.id) {
                await api.update(payload.id, payload);
                return;
            }
            await api.create(payload);
        },
        onSuccess: async () => {
            editing.value = null;
            await queryClient.invalidateQueries({ queryKey: ["products"] });
        },
    });

    const removeMutation = useMutation({
        mutationFn: (id) => api.remove(id),
        onSuccess: async () => {
            await queryClient.invalidateQueries({ queryKey: ["products"] });
        },
    });

    const items = computed(() =>
        productsQuery.data.value?.pages?.flatMap((page) => page.items || []) || []
    );

    const nextCursor = computed(() => {
        const pages = productsQuery.data.value?.pages || [];
        const lastPage = pages[pages.length - 1];
        return lastPage?.nextCursor ?? null;
    });

    const loading = computed(
        () =>
            productsQuery.isLoading.value ||
            productsQuery.isFetching.value ||
            productsQuery.isFetchingNextPage.value ||
            saveMutation.isPending.value ||
            removeMutation.isPending.value
    );

    function startEdit(product) {
        editing.value = { ...product };
    }

    function cancelEdit() {
        editing.value = null;
    }

    async function reload() {
        try {
            errorMessage.value = "";
            await queryClient.invalidateQueries({ queryKey: ["products"] });
        } catch (error) {
            errorMessage.value = getErrorMessage(error, "Failed to reload products.");
        }
    }

    async function loadMore() {
        if (!productsQuery.hasNextPage.value) return;
        try {
            errorMessage.value = "";
            await productsQuery.fetchNextPage();
        } catch (error) {
            errorMessage.value = getErrorMessage(error, "Failed to load more products.");
        }
    }

    async function save(payload) {
        try {
            errorMessage.value = "";
            await saveMutation.mutateAsync(payload);
        } catch (error) {
            errorMessage.value = getErrorMessage(error, "Failed to save product.");
        }
    }

    async function remove(id) {
        try {
            errorMessage.value = "";
            await removeMutation.mutateAsync(id);
        } catch (error) {
            errorMessage.value = getErrorMessage(error, "Failed to delete product.");
        }
    }

    return {
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
    };
}
