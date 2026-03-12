import { api as defaultApi } from "@/services/api";

export function authApi(client = defaultApi) {
    return {
        login: (payload) => client.post("/api/Auth/login", payload),
        register: (payload) => client.post("/api/Auth/register", payload),
        refresh: () => client.post("/api/Auth/refresh"),
        logout: () => client.post("/api/Auth/logout"),
    };
}

