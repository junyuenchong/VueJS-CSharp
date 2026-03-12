import { defineStore } from "pinia";
import { authApi } from "@/features/auth/api/authApi";
import { clearAuth, getEmail, getToken, setAuth } from "@/services/auth";

export const useAuthStore = defineStore("auth", {
    state: () => ({
        token: getToken(),
        view: "login",
    }),
    getters: {
        email: () => getEmail(),
    },
    actions: {
        setView(next) {
            this.view = next;
        },
        refreshAuth() {
            this.token = getToken();
        },
        async bootstrap() {
            if (this.token) return;
            try {
                const { data } = await authApi().refresh();
                setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
                this.refreshAuth();
            } catch {
                // user is not logged in
            }
        },
        async login(payload) {
            const { data } = await authApi().login(payload);
            setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
            this.refreshAuth();
        },
        async register(payload) {
            const { data } = await authApi().register(payload);
            setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
            this.refreshAuth();
        },
        async logout() {
            try {
                await authApi().logout();
            } finally {
                clearAuth();
                this.token = null;
                this.view = "login";
            }
        },
    },
});

