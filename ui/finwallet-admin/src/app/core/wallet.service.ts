import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Wallet } from '../models/wallet.model';
import { Transaction } from '../models/transaction.model';
import { Receipt } from '../models/receipt.model';

@Injectable({ providedIn: 'root' })
export class WalletService {
  constructor(private http: HttpClient, private api: ApiService) {}

  getWallets(): Observable<Wallet[]> {
    return this.http.get<Wallet[]>(`${this.api.baseUrl}/api/wallets`);
  }

  createWallet(currency: string): Observable<Wallet> {
    return this.http.post<Wallet>(`${this.api.baseUrl}/api/wallets`, { currency });
  }

  getWallet(walletId: string): Observable<Wallet> {
    return this.http.get<Wallet>(`${this.api.baseUrl}/api/wallets/${walletId}`);
  }

  topUp(walletId: string, amount: number, reference: string): Observable<Transaction> {
    return this.http.post<Transaction>(`${this.api.baseUrl}/api/wallets/${walletId}/topup`, {
      amount,
      reference
    });
  }

  payment(walletId: string, amount: number, reference: string, totpCode: string): Observable<Transaction> {
    const headers = new HttpHeaders({ 'X-Totp-Code': totpCode });
    return this.http.post<Transaction>(
      `${this.api.baseUrl}/api/wallets/${walletId}/payment`,
      { amount, reference },
      { headers }
    );
  }

  refund(walletId: string, amount: number, reference: string, totpCode: string): Observable<Transaction> {
    const headers = new HttpHeaders({ 'X-Totp-Code': totpCode });
    return this.http.post<Transaction>(
      `${this.api.baseUrl}/api/wallets/${walletId}/refund`,
      { amount, reference },
      { headers }
    );
  }

  transfer(fromWalletId: string, toWalletId: string, amount: number, reference: string, totpCode: string): Observable<Transaction[]> {
    const headers = new HttpHeaders({ 'X-Totp-Code': totpCode });
    return this.http.post<Transaction[]>(
      `${this.api.baseUrl}/api/wallets/transfer`,
      { fromWalletId, toWalletId, amount, reference },
      { headers }
    );
  }

  getTransactions(walletId: string, page = 1, pageSize = 20): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(
      `${this.api.baseUrl}/api/wallets/${walletId}/transactions?page=${page}&pageSize=${pageSize}`
    );
  }

  generateReceipt(walletId: string, transactionId: string): Observable<Receipt> {
    return this.http.post<Receipt>(
      `${this.api.baseUrl}/api/wallets/${walletId}/transactions/${transactionId}/receipt`,
      {}
    );
  }

  exportTransactions(walletId: string, totpCode: string): Observable<Blob> {
    const headers = new HttpHeaders({ 'X-Totp-Code': totpCode });
    return this.http.get(
      `${this.api.baseUrl}/api/wallets/${walletId}/transactions/export`,
      { headers, responseType: 'blob' }
    );
  }
}
