import { FormEvent, useEffect, useState } from 'react';
import { authService } from '../services/authService';
import { useToast } from '../context/ToastContext';
import { Spinner } from '../components/Spinner';

export function TotpPage() {
  const toast = useToast();
  const [secret, setSecret] = useState('');
  const [otpAuthUri, setOtpAuthUri] = useState('');
  const [code, setCode] = useState('');
  const [isLoadingSecret, setIsLoadingSecret] = useState(false);
  const [isConfirming, setIsConfirming] = useState(false);
  const [message, setMessage] = useState('');
  const [messageTone, setMessageTone] = useState<'success' | 'error' | null>(null);

  useEffect(() => {
    const load = async () => {
      setIsLoadingSecret(true);
      try {
        const result = await authService.setupTotp();
        setSecret(result.secret);
        setOtpAuthUri(result.otpAuthUri);
      } catch {
        setMessage('Khong the lay TOTP secret.');
        setMessageTone('error');
        toast.error('Khong the lay TOTP secret.');
      } finally {
        setIsLoadingSecret(false);
      }
    };

    load();
  }, [toast]);

  const submit = async (event: FormEvent) => {
    event.preventDefault();
    if (code.length < 6) {
      setMessage('Ma TOTP khong hop le.');
      setMessageTone('error');
      return;
    }

    setIsConfirming(true);
    setMessage('');
    try {
      await authService.confirmTotp(code);
      setMessage('Da kich hoat TOTP thanh cong.');
      setMessageTone('success');
      toast.success('Da kich hoat TOTP thanh cong.');
    } catch {
      setMessage('Ma TOTP khong hop le.');
      setMessageTone('error');
      toast.error('Ma TOTP khong hop le.');
    } finally {
      setIsConfirming(false);
    }
  };

  return (
    <div className="mx-auto mt-10 max-w-3xl glass-card rounded-[32px] p-8 shadow-soft fade-in">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-2xl font-semibold">Cau hinh TOTP</h2>
          <p className="mt-2 text-sm text-slate/70">
            Quet QR trong Google Authenticator hoac nhap secret de kich hoat.
          </p>
        </div>
        {isLoadingSecret ? <Spinner /> : null}
      </div>

      <div className="mt-6 rounded-3xl border border-mist bg-white/70 p-5">
        <div className="text-xs uppercase text-slate/60">Secret</div>
        <div className="mt-2 text-lg font-semibold tracking-wider">{secret || 'Dang tai...'}</div>
        {otpAuthUri ? (
          <a className="mt-2 inline-flex text-sm text-sky hover:underline" href={otpAuthUri}>
            Mo otpauth uri
          </a>
        ) : null}
      </div>

      <form onSubmit={submit} className="mt-6 grid gap-4 sm:grid-cols-[1fr_auto]">
        <div>
          <label className="text-sm font-medium">Nhap ma TOTP</label>
          <input
            type="text"
            value={code}
            onChange={(event) => setCode(event.target.value)}
            className="mt-2 w-full rounded-2xl border border-mist bg-white/70 px-4 py-2"
          />
        </div>
        <button
          type="submit"
          disabled={isConfirming}
          className="mt-6 flex h-11 items-center justify-center gap-2 rounded-2xl bg-ink px-6 text-white hover:bg-slate disabled:opacity-60"
        >
          {isConfirming ? <Spinner size="sm" /> : null}
          <span>{isConfirming ? 'Dang xu ly...' : 'Xac nhan'}</span>
        </button>
      </form>

      {message ? (
        <div className={`mt-4 text-sm ${messageTone === 'error' ? 'text-red-600' : 'text-emerald-600'}`}>
          {message}
        </div>
      ) : null}
    </div>
  );
}
