import React, { createContext, useContext, useState, useEffect, useCallback } from 'react';
import { API_BASE_URL } from '@/lib/config';
import { saveTokens, getTokens, clearTokens, isTokenExpired } from '@/lib/auth';
import type { TokenResponse } from '@/lib/types';

interface AuthContextValue {
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    (async () => {
      try {
        const tokens = await getTokens();
        if (!tokens) {
          setIsAuthenticated(false);
          return;
        }

        if (!isTokenExpired(tokens.expiresAt)) {
          setIsAuthenticated(true);
          return;
        }

        // Try silent refresh
        const res = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ refreshToken: tokens.refreshToken }),
        });

        if (res.ok) {
          const newTokens: TokenResponse = await res.json();
          await saveTokens(newTokens);
          setIsAuthenticated(true);
        } else {
          await clearTokens();
          setIsAuthenticated(false);
        }
      } catch {
        setIsAuthenticated(false);
      } finally {
        setIsLoading(false);
      }
    })();
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password }),
    });

    if (!res.ok) {
      const text = await res.text().catch(() => '');
      throw new Error(res.status === 401 ? 'Nesprávné přihlašovací údaje' : `Chyba: ${text}`);
    }

    const tokens: TokenResponse = await res.json();
    await saveTokens(tokens);
    setIsAuthenticated(true);
  }, []);

  const logout = useCallback(async () => {
    try {
      const tokens = await getTokens();
      if (tokens) {
        await fetch(`${API_BASE_URL}/api/auth/logout`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${tokens.accessToken}`,
          },
          body: JSON.stringify({ refreshToken: tokens.refreshToken }),
        });
      }
    } catch {
      // Ignore logout API errors
    } finally {
      await clearTokens();
      setIsAuthenticated(false);
    }
  }, []);

  return (
    <AuthContext.Provider value={{ isAuthenticated, isLoading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used within AuthProvider');
  return ctx;
}
