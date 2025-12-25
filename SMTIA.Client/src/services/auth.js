const TOKEN_KEY = 'smtia_token';

function base64UrlDecode(input) {
  // JWT uses base64url (RFC 7515): '-' -> '+', '_' -> '/', pad with '='
  const base64 = input.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
  try {
    // atob expects Latin1; JWT payload is JSON ASCII/UTF-8 -> safe enough for typical claims
    return atob(padded);
  } catch {
    return null;
  }
}

export function getToken() {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    console.log('[Auth] Token bulundu, uzunluk:', token.length);
  } else {
    console.warn('[Auth] Token bulunamadı, localStorage key:', TOKEN_KEY);
  }
  return token;
}

export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function getJwtPayload(token) {
  if (!token) return null;
  const parts = token.split('.');
  if (parts.length !== 3) return null;
  const decoded = base64UrlDecode(parts[1]);
  if (!decoded) return null;
  try {
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

export function getUserFromToken(token) {
  const payload = getJwtPayload(token);
  if (!payload) return null;

  // Backend claims: Id, Name, Email, UserName, roles as ClaimTypes.Role
  const rolesRaw =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
    payload['role'] ??
    [];
  const roles = Array.isArray(rolesRaw) ? rolesRaw : [rolesRaw].filter(Boolean);

  return {
    id: payload.Id || payload.id || null,
    name: payload.Name || payload.name || null,
    email: payload.Email || payload.email || null,
    userName: payload.UserName || payload.userName || null,
    roles
  };
}

export function isTokenExpired(token) {
  const payload = getJwtPayload(token);
  if (!payload || !payload.exp) return false;
  const expMs = payload.exp * 1000;
  return Date.now() >= expMs;
}


