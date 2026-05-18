import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { authEvents } from '../services/authEvents';
import { storage } from '../services/storage';

interface AuthContextValue {
  email: string | null;
  isLoggedIn: boolean;
  setSession: (accessToken: string, refreshToken: string, email: string) => void;
  clearSession: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [email, setEmail] = useState<string | null>(storage.getEmail());
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(!!storage.getAccessToken());

  const setSession = (accessToken: string, refreshToken: string, sessionEmail: string) => {
    storage.setSession(accessToken, refreshToken, sessionEmail);
    setEmail(sessionEmail);
    setIsLoggedIn(true);
  };

  const clearSession = () => {
    storage.clear();
    setEmail(null);
    setIsLoggedIn(false);
  };

  useEffect(() => {
    return authEvents.onSessionCleared(() => {
      clearSession();
    });
  }, []);

  const value = useMemo(
    () => ({
      email,
      isLoggedIn,
      setSession,
      clearSession
    }),
    [email, isLoggedIn]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
