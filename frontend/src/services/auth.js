// Production-ish approach:
// - Access token stays in memory (not persisted) to reduce XSS impact.
// - Refresh token is HttpOnly cookie (server side).
// - CSRF token is stored in sessionStorage so refresh works after page reload.
const EMAIL_KEY = "email";
const CSRF_KEY = "csrf";

let accessToken = null;

export function getToken() {
    return accessToken;
}

export function setAuth({ token, email, csrfToken }) {
    accessToken = token || null;
    sessionStorage.setItem(EMAIL_KEY, email || "");
    if (csrfToken) sessionStorage.setItem(CSRF_KEY, csrfToken);
}

export function clearAuth() {
    accessToken = null;
    sessionStorage.removeItem(EMAIL_KEY);
    sessionStorage.removeItem(CSRF_KEY);
}

export function getEmail() {
    return sessionStorage.getItem(EMAIL_KEY) || "";
}

export function getCsrfToken() {
    return sessionStorage.getItem(CSRF_KEY) || "";
}

