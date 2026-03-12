import { api as defaultApi } from "@/services/api";

export function productsApi(client = defaultApi) {
    return {
        list: (params) => client.get("/api/Products", { params }),
        create: (payload) => client.post("/api/Products", payload),
        update: (id, payload) => client.put(`/api/Products/${id}`, payload),
        remove: (id) => client.delete(`/api/Products/${id}`),
    };
}

