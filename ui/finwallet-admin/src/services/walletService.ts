import { requestBlob, requestJson } from './apiClient';
import type { Receipt } from '../types/receipt';
import type { Transaction } from '../types/transaction';
import type { Wallet } from '../types/wallet';

export const walletService = {
  async getWallets(): Promise<Wallet[]> {
    return requestJson<Wallet[]>('/api/wallets');
  },
  async createWallet(currency: string): Promise<Wallet> {
    return requestJson<Wallet>('/api/wallets', {
      method: 'POST',
      body: { currency }
    });
  },
  async getWallet(walletId: string): Promise<Wallet> {
    return requestJson<Wallet>(`/api/wallets/${walletId}`);
  },
  async topUp(walletId: string, amount: number, reference: string): Promise<Transaction> {
    return requestJson<Transaction>(`/api/wallets/${walletId}/topup`, {
      method: 'POST',
      body: { amount, reference }
    });
  },
  async payment(walletId: string, amount: number, reference: string, totpCode: string): Promise<Transaction> {
    return requestJson<Transaction>(`/api/wallets/${walletId}/payment`, {
      method: 'POST',
      headers: { 'X-Totp-Code': totpCode },
      body: { amount, reference }
    });
  },
  async refund(walletId: string, amount: number, reference: string, totpCode: string): Promise<Transaction> {
    return requestJson<Transaction>(`/api/wallets/${walletId}/refund`, {
      method: 'POST',
      headers: { 'X-Totp-Code': totpCode },
      body: { amount, reference }
    });
  },
  async transfer(
    fromWalletId: string,
    toWalletId: string,
    amount: number,
    reference: string,
    totpCode: string
  ): Promise<Transaction[]> {
    return requestJson<Transaction[]>('/api/wallets/transfer', {
      method: 'POST',
      headers: { 'X-Totp-Code': totpCode },
      body: { fromWalletId, toWalletId, amount, reference }
    });
  },
  async getTransactions(walletId: string, page = 1, pageSize = 20): Promise<Transaction[]> {
    return requestJson<Transaction[]>(`/api/wallets/${walletId}/transactions?page=${page}&pageSize=${pageSize}`);
  },
  async generateReceipt(walletId: string, transactionId: string): Promise<Receipt> {
    return requestJson<Receipt>(`/api/wallets/${walletId}/transactions/${transactionId}/receipt`, {
      method: 'POST',
      body: {}
    });
  },
  async exportTransactions(walletId: string, totpCode: string): Promise<Blob> {
    return requestBlob(`/api/wallets/${walletId}/transactions/export`, {
      headers: { 'X-Totp-Code': totpCode }
    });
  }
};
