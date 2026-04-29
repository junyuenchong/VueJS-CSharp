import { defineStore } from "pinia";
import { authApi } from "@/features/auth/api/authApi";
import { clearAuth, getEmail, getToken, setAuth } from "@/services/auth";

export const useAuthStore = defineStore("auth", {
    state: () => ({
        token: getToken(),
        view: "login",
        bootstrapping: true,
    }),
    getters: {
        email: () => getEmail(),
    },
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
        setView(next) {
            this.view = next;
        },
        refreshAuth() {
            this.token = getToken();
        },
        async bootstrap() {
            this.bootstrapping = true;
            if (this.token) {
                this.bootstrapping = false;
                return;
            }
            try {
                const { data } = await authApi().refresh();
                setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
                this.refreshAuth();
            } catch {
                // user is not logged in
            } finally {
                this.bootstrapping = false;
            }
        },
        async login(payload) {
            try {
                const { data } = await authApi().login(payload);
                setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
                this.refreshAuth();
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Login failed"));
            }
        },
        async register(payload) {
            try {
                const { data } = await authApi().register(payload);
                setAuth({ token: data.token, email: data.email, csrfToken: data.csrfToken });
                this.refreshAuth();
            } catch (error) {
                throw new Error(this.getErrorMessage(error, "Register failed"));
            }
        },
        async logout() {
            let logoutError = null;
            try {
                await authApi().logout();
            } catch (error) {
                logoutError = new Error(this.getErrorMessage(error, "Logout failed"));
            } finally {
                clearAuth();
                this.token = null;
                this.view = "login";
            }

            if (logoutError) {
                throw logoutError;
            }
        },
    },
});

