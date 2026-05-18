import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { authService } from '../services/authService';
import { Spinner } from './Spinner';

export function Header() {
  const { email, isLoggedIn, clearSession } = useAuth();
  const { info } = useToast();
  const navigate = useNavigate();
  const [isLoggingOut, setIsLoggingOut] = useState(false);

  const logout = async () => {
    setIsLoggingOut(true);
    try {
      await authService.logout();
    } catch {
      info('Da dang xuat khoi thiet bi.');
    } finally {
      clearSession();
      navigate('/login');
      setIsLoggingOut(false);
    }
  };

  return (
    <header className="relative flex flex-wrap items-center justify-between gap-4 px-5 py-6 md:px-10">
      <div>
        <div className="text-xs uppercase tracking-[0.3em] text-slate/60">FinWallet</div>
        <h1 className="text-2xl font-semibold tracking-tight md:text-3xl">FIN WALLET Command</h1>
      </div>
      {isLoggedIn ? (
        <div className="flex flex-wrap items-center gap-3 text-sm">
          <span className="rounded-full bg-white/70 px-4 py-1.5 shadow-soft">{email}</span>
          <button
            onClick={logout}
            className="flex items-center gap-2 rounded-full bg-ink px-4 py-2 text-white hover:bg-slate"
            disabled={isLoggingOut}
          >
            {isLoggingOut ? <Spinner size="sm" /> : null}
            <span>{isLoggingOut ? 'Dang xu ly...' : 'Logout'}</span>
          </button>
        </div>
      ) : (
        <div className="text-xs text-slate/60">Secure access • TOTP ready</div>
      )}
    </header>
  );
}
