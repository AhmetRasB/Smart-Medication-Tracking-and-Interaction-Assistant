const TOKEN_KEY = 'smtia_token';

export function getToken() {
  const token = localStorage.getItem(TOKEN_KEY);
  if (!token) return null;
  
  // Basic format check: JWT should have 3 parts separated by dots
  const parts = token.trim().split('.');
  if (parts.length !== 3) {
    clearToken();
    return null;
  }
  
  return token.trim();
}

export function setToken(token) {
  if (!token || typeof token !== 'string') return;
  
  const cleanToken = token.trim();
  const parts = cleanToken.split('.');
  
  // Only save if valid JWT format
  if (parts.length === 3) {
    localStorage.setItem(TOKEN_KEY, cleanToken);
  }
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function getJwtPayload(token) {
  if (!token) return null;
  
  const parts = token.split('.');
  if (parts.length !== 3) return null;
  
  try {
    // Decode base64url
    const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/');
    const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
    const decoded = atob(padded);
    return JSON.parse(decoded);
  } catch {
    return null;
  }
}

export function getUserFromToken(token) {
  const payload = getJwtPayload(token);
  if (!payload) return null;

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
