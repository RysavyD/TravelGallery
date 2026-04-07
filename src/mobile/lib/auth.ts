import * as SecureStore from 'expo-secure-store';
import type { TokenResponse } from './types';

const KEYS = {
  accessToken: 'auth_access_token',
  refreshToken: 'auth_refresh_token',
  expiresAt: 'auth_expires_at',
} as const;

export async function saveTokens(tokens: TokenResponse): Promise<void> {
  await SecureStore.setItemAsync(KEYS.accessToken, tokens.accessToken);
  await SecureStore.setItemAsync(KEYS.refreshToken, tokens.refreshToken);
  await SecureStore.setItemAsync(KEYS.expiresAt, tokens.expiresAt);
}

export async function getTokens(): Promise<TokenResponse | null> {
  const accessToken = await SecureStore.getItemAsync(KEYS.accessToken);
  const refreshToken = await SecureStore.getItemAsync(KEYS.refreshToken);
  const expiresAt = await SecureStore.getItemAsync(KEYS.expiresAt);

  if (!accessToken || !refreshToken || !expiresAt) return null;
  return { accessToken, refreshToken, expiresAt };
}

export async function clearTokens(): Promise<void> {
  await SecureStore.deleteItemAsync(KEYS.accessToken);
  await SecureStore.deleteItemAsync(KEYS.refreshToken);
  await SecureStore.deleteItemAsync(KEYS.expiresAt);
}

export function isTokenExpired(expiresAt: string): boolean {
  const expiry = new Date(expiresAt).getTime();
  const now = Date.now();
  // 60 second buffer before actual expiration
  return now >= expiry - 60_000;
}
