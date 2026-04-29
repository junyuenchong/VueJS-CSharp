import axios from "axios";
import { clearAuth, getCsrfToken, getToken, setAuth } from "./auth";

const apiBase = import.meta.env.VITE_API_URL || "http://localhost:5000";

export const api = axios.create({
  baseURL: apiBase,
  // Needed for refresh-token HttpOnly cookie calls
  withCredentials: true,
});

async function refreshAccessToken() {
  const csrf = getCsrfToken();

  const headers = csrf ? { "X-CSRF-TOKEN": csrf } : {};

  // Be resilient to brief upstream restarts (nginx may return 502/503).
  const maxAttempts = 5;
  for (let attempt = 1; attempt <= maxAttempts; attempt++) {
    try {
      const { data } = await api.post("/api/Auth/refresh", {}, { headers });
      setAuth({
        token: data.token,
        email: data.email,
        csrfToken: data.csrfToken,
      });
      return data.token;
    } catch (e) {
      const status = e?.response?.status;
      const isTransient =
        !e?.response || status === 502 || status === 503 || status === 504;

      if (!isTransient || attempt === maxAttempts) {
        throw e;
      }

      const delayMs = 250 * Math.pow(2, attempt - 1);
      await new Promise((r) => setTimeout(r, delayMs));
    }
  }
}

api.interceptors.request.use((config) => {
  const token = getToken();
  if (token) {
    config.headers = config.headers || {};
    config.headers.Authorization = `Bearer ${token}`;
  }

  // Attach CSRF header for refresh/logout (cookie-based endpoints)
  const csrf = getCsrfToken();
  if (csrf) {
    config.headers = config.headers || {};
    config.headers["X-CSRF-TOKEN"] = csrf;
  }
  return config;
});

let isRefreshing = false;
let refreshWaiters = [];

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const original = error?.config;
    const status = error?.response?.status;

    // Don't attempt refresh for auth endpoints themselves
    if (!original || original.url?.includes("/api/Auth/")) {
      return Promise.reject(error);
    }

    if (status === 401) {
      // Prevent infinite loops
      if (original._retry) {
        clearAuth();
        return Promise.reject(error);
      }
      original._retry = true;

      if (isRefreshing) {
        await new Promise((resolve, reject) => {
          refreshWaiters.push({ resolve, reject });
        });
      } else {
        isRefreshing = true;
        try {
          await refreshAccessToken();
          refreshWaiters.forEach((w) => w.resolve());
        } catch (e) {
          refreshWaiters.forEach((w) => w.reject(e));
          clearAuth();
          throw e;
        } finally {
          isRefreshing = false;
          refreshWaiters = [];
        }
      }

      return api(original);
    }

    return Promise.reject(error);
  },
);
