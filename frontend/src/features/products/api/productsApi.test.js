import { describe, expect, it, vi } from "vitest";
import { productsApi } from "./productsApi";

describe("productsApi", () => {
    it("calls correct endpoints", async () => {
        const client = {
            get: vi.fn().mockResolvedValue({ data: { items: [], nextCursor: null } }),
            post: vi.fn().mockResolvedValue({ data: {} }),
            put: vi.fn().mockResolvedValue({ data: {} }),
            delete: vi.fn().mockResolvedValue({ data: {} }),
        };

        const p = productsApi(client);
        await p.list({ limit: 20 });
        await p.create({ name: "A" });
        await p.update(1, { id: 1 });
        await p.remove(1);

        expect(client.get).toHaveBeenCalledWith("/api/Products", { params: { limit: 20 } });
        expect(client.post).toHaveBeenCalledWith("/api/Products", { name: "A" });
        expect(client.put).toHaveBeenCalledWith("/api/Products/1", { id: 1 });
        expect(client.delete).toHaveBeenCalledWith("/api/Products/1");
    });
});

