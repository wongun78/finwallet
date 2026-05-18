import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/authService';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { Spinner } from '../components/Spinner';

export function LoginPage() {
  const navigate = useNavigate();
  const { setSession } = useAuth();
  const toast = useToast();

  const [email, setEmail] = useState('admin@finwallet.local');
  const [password, setPassword] = useState('');
  const [totpCode, setTotpCode] = useState('');
  const [requiresTotp, setRequiresTotp] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');

  const canSubmit = email.length > 4 && password.length >= 8 && (!requiresTotp || totpCode.length >= 6);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (!canSubmit) {
      setErrorMessage('Vui long nhap day du thong tin.');
      return;
    }

    setIsLoading(true);
    setErrorMessage('');

    try {
      const result = await authService.login(email, password, requiresTotp ? totpCode : undefined);
      if (result.requiresTotp) {
        setRequiresTotp(true);
        setIsLoading(false);
        setErrorMessage('Can ma TOTP de tiep tuc.');
        toast.info('Can ma TOTP de tiep tuc.');
        return;
      }

      if (result.accessToken && result.refreshToken) {
        setSession(result.accessToken, result.refreshToken, result.email);
      }

      toast.success('Dang nhap thanh cong.');
      navigate('/');
    } catch (error) {
      const message = (error as Error).message || 'Dang nhap that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="mx-auto mt-10 grid max-w-5xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
      <div className="glass-card rounded-[32px] p-8 shadow-soft fade-in">
        <h2 className="text-xs uppercase tracking-[0.4em] text-slate/60">FinWallet Identity</h2>
        <h1 className="mt-3 text-3xl font-semibold">Dang nhap quan tri</h1>
        <p className="mt-2 text-sm text-slate/80">
          Mo khoa he sinh thai vi tien, bao mat da lop va TOTP.
        </p>

        <form onSubmit={submit} className="mt-6 space-y-4">
          <div>
            <label className="text-sm font-medium">Email</label>
            <input
              type="email"
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="mt-2 w-full rounded-2xl border border-mist bg-white/70 px-4 py-2"
              placeholder="admin@finwallet.local"
            />
          </div>
          <div>
            <label className="text-sm font-medium">Mat khau</label>
            <input
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="mt-2 w-full rounded-2xl border border-mist bg-white/70 px-4 py-2"
            />
          </div>
          {requiresTotp ? (
            <div>
              <label className="text-sm font-medium">Ma TOTP</label>
              <input
                type="text"
                value={totpCode}
                onChange={(event) => setTotpCode(event.target.value)}
                className="mt-2 w-full rounded-2xl border border-mist bg-white/70 px-4 py-2"
                placeholder="123456"
              />
            </div>
          ) : null}

          {errorMessage ? <div className="text-sm text-red-600">{errorMessage}</div> : null}

          <button
            type="submit"
            disabled={!canSubmit || isLoading}
            className="flex w-full items-center justify-center gap-2 rounded-2xl bg-ink py-3 text-white hover:bg-slate disabled:opacity-60"
          >
            {isLoading ? <Spinner size="sm" /> : null}
            <span>{isLoading ? 'Dang xu ly...' : 'Dang nhap'}</span>
          </button>

          <button
            type="button"
            onClick={() => navigate('/totp')}
            className="text-sm text-sky hover:underline"
          >
            Cau hinh TOTP sau khi dang nhap
          </button>
        </form>
      </div>

      <div className="glass-card rounded-[32px] p-8 shadow-soft fade-in">
        <div className="space-y-4">
          <div className="rounded-2xl border border-mist bg-white/60 p-4">
            <div className="text-xs uppercase text-slate/60">Tip</div>
            <div className="mt-2 text-sm text-slate/80">
              Doi mat khau dinh ky va luu token an toan trong trinh duyet tin cay.
            </div>
          </div>
          <div className="rounded-2xl border border-mist bg-white/60 p-4">
            <div className="text-xs uppercase text-slate/60">Session</div>
            <div className="mt-2 text-sm text-slate/80">
              He thong tu dong refresh token neu phien lam viec het han.
            </div>
          </div>
          <div className="rounded-2xl border border-mist bg-white/60 p-4">
            <div className="text-xs uppercase text-slate/60">Zero Trust</div>
            <div className="mt-2 text-sm text-slate/80">
              Moi giao dich nhay cam can TOTP de xac nhan.
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
