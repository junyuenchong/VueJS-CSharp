import { describe, expect, it, vi } from "vitest";
import { authApi } from "./authApi";

describe("authApi", () => {
    it("calls correct endpoints", async () => {
        const client = {
            post: vi.fn().mockResolvedValue({ data: {} }),
        };

        const a = authApi(client);
        await a.login({ email: "a@b.com", password: "x" });
        await a.register({ email: "a@b.com", password: "x" });
        await a.refresh();
        await a.logout();

        expect(client.post).toHaveBeenNthCalledWith(1, "/api/Auth/login", { email: "a@b.com", password: "x" });
        expect(client.post).toHaveBeenNthCalledWith(2, "/api/Auth/register", { email: "a@b.com", password: "x" });
        expect(client.post).toHaveBeenNthCalledWith(3, "/api/Auth/refresh");
        expect(client.post).toHaveBeenNthCalledWith(4, "/api/Auth/logout");
    });
});

