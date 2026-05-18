export interface Wallet {
  walletId: string;
  userId: string;
  currency: string;
  balance: number;
  status: string;
  createdAtUtc: string;
  updatedAtUtc: string;
}
