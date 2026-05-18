export interface Transaction {
  transactionId: string;
  walletId: string;
  type: string;
  status: string;
  amount: number;
  balanceBefore: number;
  balanceAfter: number;
  reference: string;
  counterpartyWalletId?: string | null;
  createdAtUtc: string;
}
