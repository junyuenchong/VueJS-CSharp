import { computed } from "vue";
import { storeToRefs } from "pinia";
import { useAuthStore } from "@/stores/authStore";

export function useAuth() {
    const store = useAuthStore();

    const { token, view } = storeToRefs(store);
    const email = computed(() => store.email);

    return {
        token,
        email,
        view,
        setView: store.setView,
        refreshAuth: store.refreshAuth,
        bootstrap: store.bootstrap,
        login: store.login,
        register: store.register,
        logout: store.logout,
    };
}
