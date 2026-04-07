import { API_BASE_URL } from './config';
import { getTokens, saveTokens, clearTokens, isTokenExpired } from './auth';
import type { TokenResponse } from './types';

export class AuthError extends Error {
  constructor() {
    super('Authentication failed');
    this.name = 'AuthError';
  }
}

let refreshPromise: Promise<TokenResponse> | null = null;

async function refreshTokens(currentRefreshToken: string): Promise<TokenResponse> {
  const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken: currentRefreshToken }),
  });

  if (!res.ok) {
    await clearTokens();
    throw new AuthError();
  }

  const tokens: TokenResponse = await res.json();
  await saveTokens(tokens);
  return tokens;
}

async function getValidAccessToken(): Promise<string> {
  const tokens = await getTokens();
  if (!tokens) throw new AuthError();

  if (!isTokenExpired(tokens.expiresAt)) {
    return tokens.accessToken;
  }

  // Mutex: only one refresh at a time
  if (!refreshPromise) {
    refreshPromise = refreshTokens(tokens.refreshToken).finally(() => {
      refreshPromise = null;
    });
  }

  const newTokens = await refreshPromise;
  return newTokens.accessToken;
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
): Promise<T> {
  const accessToken = await getValidAccessToken();

  const headers: Record<string, string> = {
    Authorization: `Bearer ${accessToken}`,
    ...(options.headers as Record<string, string>),
  };

  // Set Content-Type for JSON bodies (skip for FormData/multipart)
  if (options.body && typeof options.body === 'string') {
    headers['Content-Type'] = 'application/json';
  }

  const res = await fetch(`${API_BASE_URL}${path}`, {
    ...options,
    headers,
  });

  // On 401, try refresh once and retry
  if (res.status === 401) {
    const tokens = await getTokens();
    if (!tokens) throw new AuthError();

    try {
      const newTokens = await refreshTokens(tokens.refreshToken);
      const retryRes = await fetch(`${API_BASE_URL}${path}`, {
        ...options,
        headers: {
          ...headers,
          Authorization: `Bearer ${newTokens.accessToken}`,
        },
      });

      if (retryRes.status === 401) {
        await clearTokens();
        throw new AuthError();
      }

      if (retryRes.status === 204) return undefined as T;
      return retryRes.json();
    } catch (e) {
      if (e instanceof AuthError) throw e;
      await clearTokens();
      throw new AuthError();
    }
  }

  if (!res.ok) {
    const text = await res.text().catch(() => '');
    throw new Error(`API error ${res.status}: ${text}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}
