import { FormEvent, useEffect, useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { walletService } from '../services/walletService';
import type { Transaction } from '../types/transaction';
import type { Wallet } from '../types/wallet';
import { useToast } from '../context/ToastContext';
import { useConfirm } from '../context/ConfirmContext';
import { Spinner } from '../components/Spinner';
import { compactId, formatNumber } from '../utils/format';

interface ChartBar {
  label: string;
  value: number;
}

export function DashboardPage() {
  const toast = useToast();
  const confirmDialog = useConfirm();

  const [wallets, setWallets] = useState<Wallet[]>([]);
  const [transactions, setTransactions] = useState<Transaction[]>([]);
  const [selectedWallet, setSelectedWallet] = useState<Wallet | null>(null);
  const [chartBars, setChartBars] = useState<ChartBar[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const [isCreateWalletLoading, setIsCreateWalletLoading] = useState(false);
  const [isTopupLoading, setIsTopupLoading] = useState(false);
  const [isPaymentLoading, setIsPaymentLoading] = useState(false);
  const [isRefundLoading, setIsRefundLoading] = useState(false);
  const [isTransferLoading, setIsTransferLoading] = useState(false);
  const [isExportLoading, setIsExportLoading] = useState(false);
  const [receiptLoadingId, setReceiptLoadingId] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState('');

  const [currency, setCurrency] = useState('VND');
  const [topupAmount, setTopupAmount] = useState(0);
  const [topupReference, setTopupReference] = useState('TOPUP');
  const [paymentAmount, setPaymentAmount] = useState(0);
  const [paymentReference, setPaymentReference] = useState('PAY');
  const [paymentTotp, setPaymentTotp] = useState('');
  const [refundAmount, setRefundAmount] = useState(0);
  const [refundReference, setRefundReference] = useState('REFUND');
  const [refundTotp, setRefundTotp] = useState('');
  const [transferFromWalletId, setTransferFromWalletId] = useState('');
  const [transferToWalletId, setTransferToWalletId] = useState('');
  const [transferAmount, setTransferAmount] = useState(0);
  const [transferReference, setTransferReference] = useState('TRANSFER');
  const [transferTotp, setTransferTotp] = useState('');
  const [exportTotp, setExportTotp] = useState('');

  const stats = useMemo(
    () => [
      { label: 'So du', value: selectedWallet ? formatNumber(selectedWallet.balance) : '--' },
      { label: 'Currency', value: selectedWallet?.currency ?? '--' },
      { label: 'Trang thai', value: selectedWallet?.status ?? '--' }
    ],
    [selectedWallet]
  );

  useEffect(() => {
    const load = async () => {
      setIsLoading(true);
      try {
        const data = await walletService.getWallets();
        setWallets(data);
        if (data.length > 0) {
          setSelectedWallet(data[0]);
          setTransferFromWalletId(data[0].walletId);
          await loadTransactions(data[0].walletId);
        }
      } catch (error) {
        const message = (error as Error).message || 'Khong the tai danh sach vi.';
        setErrorMessage(message);
        toast.error(message);
      } finally {
        setIsLoading(false);
      }
    };

    load();
  }, [toast]);

  const buildChart = (items: Transaction[]) => {
    const recent = [...items].slice(0, 6).reverse();
    setChartBars(
      recent.map((item) => ({
        label: item.type,
        value: Math.min(Math.abs(item.amount), 200000)
      }))
    );
  };

  const loadTransactions = async (walletId: string) => {
    try {
      const items = await walletService.getTransactions(walletId);
      setTransactions(items);
      buildChart(items);
    } catch {
      const message = 'Khong the tai giao dich.';
      setErrorMessage(message);
      toast.error(message);
    }
  };

  const selectWallet = (wallet: Wallet) => {
    setSelectedWallet(wallet);
    setTransferFromWalletId(wallet.walletId);
    loadTransactions(wallet.walletId);
  };

  const createWallet = async (event: FormEvent) => {
    event.preventDefault();
    if (!currency || currency.length < 3) {
      setErrorMessage('Currency khong hop le.');
      return;
    }

    setIsCreateWalletLoading(true);
    try {
      const wallet = await walletService.createWallet(currency);
      setWallets((prev) => [wallet, ...prev]);
      selectWallet(wallet);
      toast.success('Tao vi thanh cong.');
    } catch {
      const message = 'Khong the tao vi.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsCreateWalletLoading(false);
    }
  };

  const topUp = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedWallet || topupAmount <= 0) {
      return;
    }

    setIsTopupLoading(true);
    try {
      await walletService.topUp(selectedWallet.walletId, topupAmount, topupReference);
      await refreshSelected();
      toast.success('TopUp thanh cong.');
    } catch {
      const message = 'Topup that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsTopupLoading(false);
    }
  };

  const payment = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedWallet || paymentAmount <= 0 || paymentTotp.length < 6) {
      return;
    }

    const confirmed = await confirmDialog.confirm(
      'Xac nhan thanh toan',
      `Thanh toan ${formatNumber(paymentAmount)} ${selectedWallet.currency}?`
    );
    if (!confirmed) {
      return;
    }

    setIsPaymentLoading(true);
    try {
      await walletService.payment(selectedWallet.walletId, paymentAmount, paymentReference, paymentTotp);
      await refreshSelected();
      toast.success('Payment thanh cong.');
    } catch {
      const message = 'Payment that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsPaymentLoading(false);
    }
  };

  const refund = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedWallet || refundAmount <= 0 || refundTotp.length < 6) {
      return;
    }

    setIsRefundLoading(true);
    try {
      await walletService.refund(selectedWallet.walletId, refundAmount, refundReference, refundTotp);
      await refreshSelected();
      toast.success('Refund thanh cong.');
    } catch {
      const message = 'Refund that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsRefundLoading(false);
    }
  };

  const transfer = async (event: FormEvent) => {
    event.preventDefault();
    if (!transferFromWalletId || !transferToWalletId || transferAmount <= 0 || transferTotp.length < 6) {
      return;
    }

    const confirmed = await confirmDialog.confirm(
      'Xac nhan chuyen tien',
      `Chuyen ${formatNumber(transferAmount)} sang vi ${transferToWalletId}?`
    );
    if (!confirmed) {
      return;
    }

    setIsTransferLoading(true);
    try {
      await walletService.transfer(
        transferFromWalletId,
        transferToWalletId,
        transferAmount,
        transferReference,
        transferTotp
      );
      await refreshSelected();
      toast.success('Transfer thanh cong.');
    } catch {
      const message = 'Transfer that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsTransferLoading(false);
    }
  };

  const exportTransactions = async (event: FormEvent) => {
    event.preventDefault();
    if (!selectedWallet || exportTotp.length < 6) {
      return;
    }

    setIsExportLoading(true);
    try {
      const blob = await walletService.exportTransactions(selectedWallet.walletId, exportTotp);
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = `transactions-${selectedWallet.walletId}.xlsx`;
      link.click();
      window.URL.revokeObjectURL(url);
      toast.success('Da export Excel.');
    } catch {
      const message = 'Export that bai.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setIsExportLoading(false);
    }
  };

  const downloadReceipt = async (tx: Transaction) => {
    if (!selectedWallet) {
      return;
    }

    setReceiptLoadingId(tx.transactionId);
    try {
      const result = await walletService.generateReceipt(selectedWallet.walletId, tx.transactionId);
      window.open(result.url, '_blank');
      toast.success('Da tao receipt.');
    } catch {
      const message = 'Khong the tao receipt.';
      setErrorMessage(message);
      toast.error(message);
    } finally {
      setReceiptLoadingId(null);
    }
  };

  const refreshSelected = async () => {
    if (!selectedWallet) {
      return;
    }

    try {
      const wallet = await walletService.getWallet(selectedWallet.walletId);
      setSelectedWallet(wallet);
      await loadTransactions(wallet.walletId);
    } catch {
      const message = 'Khong the lam moi vi.';
      setErrorMessage(message);
      toast.error(message);
    }
  };

  return (
    <div className="grid gap-6 lg:grid-cols-[320px_1fr]">
      <aside className="space-y-4">
        <div className="glass-card rounded-[28px] p-5 shadow-soft fade-in">
          <div className="flex items-center justify-between">
            <h3 className="text-lg font-semibold">Wallets</h3>
            <span className="text-xs text-slate/60">{wallets.length} vi</span>
          </div>
          <div className="mt-4 space-y-3">
            {isLoading ? (
              <div className="rounded-2xl border border-mist p-4 text-sm text-slate/60">Dang tai vi...</div>
            ) : null}
            {wallets.map((wallet) => (
              <button
                key={wallet.walletId}
                className={`w-full rounded-2xl border border-mist p-4 text-left transition ${
                  wallet.walletId === selectedWallet?.walletId
                    ? 'bg-ink text-white shadow-soft'
                    : 'hover:border-ink'
                }`}
                onClick={() => selectWallet(wallet)}
              >
                <div className="text-xs uppercase tracking-[0.2em] opacity-80">{wallet.currency}</div>
                <div className="mt-1 text-xl font-semibold">{formatNumber(wallet.balance)}</div>
                <div className="text-xs opacity-70">{wallet.status}</div>
              </button>
            ))}
          </div>
        </div>

        <div className="glass-card rounded-[28px] p-5 shadow-soft fade-in">
          <h3 className="text-lg font-semibold">Tao vi moi</h3>
          <form onSubmit={createWallet} className="mt-4 flex gap-2">
            <input
              className="flex-1 rounded-2xl border border-mist px-3 py-2"
              value={currency}
              onChange={(event) => setCurrency(event.target.value)}
              placeholder="VND"
            />
            <button
              className="flex items-center gap-2 rounded-2xl bg-ink px-4 text-white"
              disabled={isCreateWalletLoading}
            >
              {isCreateWalletLoading ? <Spinner size="sm" /> : null}
              <span>{isCreateWalletLoading ? 'Dang tao...' : 'Tao'}</span>
            </button>
          </form>
        </div>
      </aside>

      <section className="space-y-6">
        <div className="glass-card rounded-[32px] p-6 shadow-soft fade-in">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-2xl font-semibold">Dashboard</h2>
              <p className="text-xs text-slate/60">
                {selectedWallet ? `Wallet ${compactId(selectedWallet.walletId)}` : 'Chon vi de bat dau'}
              </p>
            </div>
          </div>

          <div className="mt-5 grid gap-4 md:grid-cols-3">
            {stats.map((stat) => (
              <div key={stat.label} className="rounded-2xl border border-mist bg-white/70 p-4">
                <div className="text-xs uppercase text-slate/60">{stat.label}</div>
                <div className="mt-2 text-2xl font-semibold">{stat.value}</div>
              </div>
            ))}
          </div>

          <div className="mt-6">
            <div className="text-xs uppercase text-slate/60">Transaction trend</div>
            <div className="mt-3 flex h-32 items-end gap-2">
              {chartBars.length === 0 ? (
                <div className="text-sm text-slate/60">Chua co giao dich.</div>
              ) : (
                chartBars.map((bar) => (
                  <div
                    key={`${bar.label}-${bar.value}`}
                    className="flex-1 rounded-xl bg-sky/70"
                    style={{ height: `${bar.value / 1000}px` }}
                    title={bar.label}
                  />
                ))
              )}
            </div>
          </div>
        </div>

        <div className="grid gap-6 lg:grid-cols-2">
          <div className="glass-card rounded-[28px] p-5 shadow-soft">
            <h3 className="text-lg font-semibold">TopUp</h3>
            <form onSubmit={topUp} className="mt-4 space-y-3">
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                type="number"
                value={topupAmount}
                onChange={(event) => setTopupAmount(Number(event.target.value))}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={topupReference}
                onChange={(event) => setTopupReference(event.target.value)}
              />
              <button
                className="flex w-full items-center justify-center gap-2 rounded-2xl bg-ink py-2 text-white"
                disabled={isTopupLoading}
              >
                {isTopupLoading ? <Spinner size="sm" /> : null}
                <span>{isTopupLoading ? 'Dang xu ly...' : 'TopUp'}</span>
              </button>
            </form>
          </div>

          <div className="glass-card rounded-[28px] p-5 shadow-soft">
            <h3 className="text-lg font-semibold">Payment</h3>
            <form onSubmit={payment} className="mt-4 space-y-3">
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                type="number"
                value={paymentAmount}
                onChange={(event) => setPaymentAmount(Number(event.target.value))}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={paymentReference}
                onChange={(event) => setPaymentReference(event.target.value)}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={paymentTotp}
                onChange={(event) => setPaymentTotp(event.target.value)}
                placeholder="TOTP"
              />
              <button
                className="flex w-full items-center justify-center gap-2 rounded-2xl bg-ink py-2 text-white"
                disabled={isPaymentLoading}
              >
                {isPaymentLoading ? <Spinner size="sm" /> : null}
                <span>{isPaymentLoading ? 'Dang xu ly...' : 'Pay'}</span>
              </button>
            </form>
          </div>

          <div className="glass-card rounded-[28px] p-5 shadow-soft">
            <h3 className="text-lg font-semibold">Refund</h3>
            <form onSubmit={refund} className="mt-4 space-y-3">
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                type="number"
                value={refundAmount}
                onChange={(event) => setRefundAmount(Number(event.target.value))}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={refundReference}
                onChange={(event) => setRefundReference(event.target.value)}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={refundTotp}
                onChange={(event) => setRefundTotp(event.target.value)}
                placeholder="TOTP"
              />
              <button
                className="flex w-full items-center justify-center gap-2 rounded-2xl bg-ink py-2 text-white"
                disabled={isRefundLoading}
              >
                {isRefundLoading ? <Spinner size="sm" /> : null}
                <span>{isRefundLoading ? 'Dang xu ly...' : 'Refund'}</span>
              </button>
            </form>
          </div>

          <div className="glass-card rounded-[28px] p-5 shadow-soft">
            <h3 className="text-lg font-semibold">Transfer</h3>
            <form onSubmit={transfer} className="mt-4 space-y-3">
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={transferFromWalletId}
                onChange={(event) => setTransferFromWalletId(event.target.value)}
                placeholder="From Wallet"
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={transferToWalletId}
                onChange={(event) => setTransferToWalletId(event.target.value)}
                placeholder="To Wallet"
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                type="number"
                value={transferAmount}
                onChange={(event) => setTransferAmount(Number(event.target.value))}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={transferReference}
                onChange={(event) => setTransferReference(event.target.value)}
              />
              <input
                className="w-full rounded-2xl border border-mist px-3 py-2"
                value={transferTotp}
                onChange={(event) => setTransferTotp(event.target.value)}
                placeholder="TOTP"
              />
              <button
                className="flex w-full items-center justify-center gap-2 rounded-2xl bg-ink py-2 text-white"
                disabled={isTransferLoading}
              >
                {isTransferLoading ? <Spinner size="sm" /> : null}
                <span>{isTransferLoading ? 'Dang xu ly...' : 'Transfer'}</span>
              </button>
            </form>
          </div>
        </div>

        <div className="glass-card rounded-[32px] p-6 shadow-soft">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h3 className="text-lg font-semibold">Transactions</h3>
              <p className="text-xs text-slate/60">Theo doi lich su giao dich theo vi</p>
            </div>
            <Link className="text-sm text-sky hover:underline" to="/totp">
              Cau hinh TOTP
            </Link>
          </div>

          <form onSubmit={exportTransactions} className="mt-4 flex flex-wrap gap-2">
            <input
              className="rounded-2xl border border-mist px-3 py-2"
              value={exportTotp}
              onChange={(event) => setExportTotp(event.target.value)}
              placeholder="TOTP"
            />
            <button
              className="flex items-center gap-2 rounded-2xl bg-ink px-4 py-2 text-white"
              disabled={isExportLoading}
            >
              {isExportLoading ? <Spinner size="sm" /> : null}
              <span>{isExportLoading ? 'Dang export...' : 'Export Excel'}</span>
            </button>
          </form>

          <div className="mt-4 overflow-x-auto">
            <table className="min-w-full text-sm">
              <thead className="text-left text-slate/70">
                <tr>
                  <th className="py-2">Type</th>
                  <th>Amount</th>
                  <th>Balance</th>
                  <th>Ref</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {transactions.map((tx) => (
                  <tr key={tx.transactionId} className="border-t border-mist">
                    <td className="py-2">{tx.type}</td>
                    <td>{formatNumber(tx.amount)}</td>
                    <td>{formatNumber(tx.balanceAfter)}</td>
                    <td>{tx.reference}</td>
                    <td>
                      <button
                        className="flex items-center gap-2 text-sky hover:underline"
                        onClick={() => downloadReceipt(tx)}
                        disabled={receiptLoadingId === tx.transactionId}
                      >
                        {receiptLoadingId === tx.transactionId ? <Spinner size="sm" /> : null}
                        <span>Receipt</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {errorMessage ? <div className="text-sm text-red-600">{errorMessage}</div> : null}
      </section>
    </div>
  );
}
