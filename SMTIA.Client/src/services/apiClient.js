import { getToken, clearToken } from './auth';

const API_BASE = (process.env.REACT_APP_API_BASE_URL || '').replace(/\/$/, '');

async function parseJsonSafe(response) {
  const text = await response.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return text;
  }
}

export async function apiRequest(path, options = {}) {
  const {
    method = 'GET',
    body,
    headers = {},
    auth = true
  } = options;

  const url = `${API_BASE}${path}`;
  const finalHeaders = { ...headers };

  if (auth) {
    const token = getToken();
    if (token) {
      finalHeaders.Authorization = `Bearer ${token}`;
    } else {
      const error = new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
      error.status = 401;
      throw error;
    }
  }

  let finalBody = body;
  if (body && typeof body === 'object' && !(body instanceof FormData)) {
    finalHeaders['Content-Type'] = finalHeaders['Content-Type'] || 'application/json';
    finalBody = JSON.stringify(body);
  }

  const res = await fetch(url, {
    method,
    headers: finalHeaders,
    body: finalBody,
    redirect: 'follow'
  });

  if (res.status === 401) {
    clearToken();
    const error = new Error('Oturum süreniz dolmuş. Lütfen tekrar giriş yapın.');
    error.status = 401;
    throw error;
  }
  
  if (res.status === 429) {
    const error = new Error('Çok fazla deneme yaptınız. Lütfen 1 saat sonra tekrar deneyin.');
    error.status = 429;
    throw error;
  }

  const data = await parseJsonSafe(res);

  if (!res.ok) {
    if (data && typeof data === 'object' && 'isSuccessful' in data) {
      return data;
    }

    const message =
      (data && data.message) ||
      (data && data.title) ||
      (typeof data === 'string' ? data : null) ||
      `Request failed (${res.status})`;
    const error = new Error(message);
    error.status = res.status;
    error.data = data;
    throw error;
  }

  return data;
}

export function unwrapResult(result) {
  if (!result || typeof result !== 'object') return result;
  if ('isSuccessful' in result) {
    if (result.isSuccessful) return result.data;
    const errMsg = Array.isArray(result.errorMessages) 
      ? result.errorMessages.join(', ') 
      : (result.errorMessages || 'İşlem başarısız');
    const error = new Error(errMsg);
    error.data = result;
    throw error;
  }
  return result;
}
