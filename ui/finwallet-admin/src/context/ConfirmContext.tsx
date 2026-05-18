import { createContext, useContext, useMemo, useState } from 'react';

interface ConfirmState {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  resolve: (value: boolean) => void;
}

interface ConfirmContextValue {
  confirm: (title: string, message: string, confirmText?: string, cancelText?: string) => Promise<boolean>;
}

const ConfirmContext = createContext<ConfirmContextValue | null>(null);

export function ConfirmProvider({ children }: { children: React.ReactNode }) {
  const [dialog, setDialog] = useState<ConfirmState | null>(null);

  const confirm = (title: string, message: string, confirmText = 'Xac nhan', cancelText = 'Huy') => {
    return new Promise<boolean>((resolve) => {
      setDialog({ title, message, confirmText, cancelText, resolve });
    });
  };

  const close = (result: boolean) => {
    dialog?.resolve(result);
    setDialog(null);
  };

  const value = useMemo(() => ({ confirm }), []);

  return (
    <ConfirmContext.Provider value={value}>
      {children}
      {dialog ? (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-ink/60 px-4">
          <div className="glass-card w-full max-w-sm rounded-3xl p-6 shadow-soft">
            <h3 className="text-lg font-semibold">{dialog.title}</h3>
            <p className="mt-2 text-sm text-slate/80">{dialog.message}</p>
            <div className="mt-5 flex items-center justify-end gap-3">
              <button
                onClick={() => close(false)}
                className="rounded-full border border-mist px-4 py-2 text-sm font-medium text-slate hover:border-ink"
              >
                {dialog.cancelText}
              </button>
              <button
                onClick={() => close(true)}
                className="rounded-full bg-ink px-4 py-2 text-sm font-medium text-white hover:bg-slate"
              >
                {dialog.confirmText}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </ConfirmContext.Provider>
  );
}

export function useConfirm() {
  const context = useContext(ConfirmContext);
  if (!context) {
    throw new Error('useConfirm must be used within ConfirmProvider');
  }
  return context;
}
