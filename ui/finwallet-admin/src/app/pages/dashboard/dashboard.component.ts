import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { WalletService } from '../../core/wallet.service';
import { NotificationService } from '../../core/notification.service';
import { Wallet } from '../../models/wallet.model';
import { Transaction } from '../../models/transaction.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, MatProgressSpinnerModule],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {
  wallets: Wallet[] = [];
  transactions: Transaction[] = [];
  selectedWallet?: Wallet;
  isLoading = false;
  isCreateWalletLoading = false;
  isTopupLoading = false;
  isPaymentLoading = false;
  isRefundLoading = false;
  isTransferLoading = false;
  isExportLoading = false;
  receiptLoadingId: string | null = null;
  errorMessage = '';
  chartBars: { label: string; value: number }[] = [];

  createWalletForm = this.fb.group({
    currency: ['VND', [Validators.required, Validators.minLength(3)]]
  });

  topupForm = this.fb.group({
    amount: [0, [Validators.required, Validators.min(1)]],
    reference: ['TOPUP']
  });

  paymentForm = this.fb.group({
    amount: [0, [Validators.required, Validators.min(1)]],
    reference: ['PAY'],
    totpCode: ['', [Validators.required, Validators.minLength(6)]]
  });

  refundForm = this.fb.group({
    amount: [0, [Validators.required, Validators.min(1)]],
    reference: ['REFUND'],
    totpCode: ['', [Validators.required, Validators.minLength(6)]]
  });

  transferForm = this.fb.group({
    fromWalletId: ['', [Validators.required]],
    toWalletId: ['', [Validators.required]],
    amount: [0, [Validators.required, Validators.min(1)]],
    reference: ['TRANSFER'],
    totpCode: ['', [Validators.required, Validators.minLength(6)]]
  });

  exportForm = this.fb.group({
    totpCode: ['', [Validators.required, Validators.minLength(6)]]
  });

  constructor(
    private fb: FormBuilder,
    private walletService: WalletService,
    private notify: NotificationService
  ) {
    this.loadWallets();
  }

  loadWallets(): void {
    this.isLoading = true;
    this.walletService.getWallets().subscribe({
      next: (items) => {
        this.wallets = items;
        this.selectedWallet = items[0];
        if (this.selectedWallet) {
          this.transferForm.patchValue({ fromWalletId: this.selectedWallet.walletId });
          this.loadTransactions(this.selectedWallet.walletId);
        }
        this.isLoading = false;
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err?.error?.detail || 'Khong the tai danh sach vi.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  selectWallet(wallet: Wallet): void {
    this.selectedWallet = wallet;
    this.transferForm.patchValue({ fromWalletId: wallet.walletId });
    this.loadTransactions(wallet.walletId);
  }

  loadTransactions(walletId: string): void {
    this.walletService.getTransactions(walletId).subscribe({
      next: (items) => {
        this.transactions = items;
        this.buildChart();
      },
      error: () => {
        this.errorMessage = 'Khong the tai giao dich.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  buildChart(): void {
    const recent = [...this.transactions].slice(0, 6).reverse();
    this.chartBars = recent.map((item) => ({
      label: item.type,
      value: Math.min(Math.abs(item.amount), 200000)
    }));
  }

  createWallet(): void {
    if (this.createWalletForm.invalid) {
      return;
    }

    this.isCreateWalletLoading = true;
    this.walletService.createWallet(this.createWalletForm.getRawValue().currency!).subscribe({
      next: (wallet) => {
        this.isCreateWalletLoading = false;
        this.wallets = [wallet, ...this.wallets];
        this.selectWallet(wallet);
        this.notify.success('Tao vi thanh cong.');
      },
      error: () => {
        this.isCreateWalletLoading = false;
        this.errorMessage = 'Khong the tao vi.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  topUp(): void {
    if (!this.selectedWallet || this.topupForm.invalid) {
      return;
    }

    const { amount, reference } = this.topupForm.getRawValue();
    this.isTopupLoading = true;
    this.walletService.topUp(this.selectedWallet.walletId, amount!, reference!).subscribe({
      next: () => {
        this.isTopupLoading = false;
        this.refreshSelected();
        this.notify.success('TopUp thanh cong.');
      },
      error: () => {
        this.isTopupLoading = false;
        this.errorMessage = 'Topup that bai.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  payment(): void {
    if (!this.selectedWallet || this.paymentForm.invalid) {
      return;
    }

    const { amount, reference, totpCode } = this.paymentForm.getRawValue();
    const currency = this.selectedWallet.currency;
    this.notify
      .confirm('Xac nhan thanh toan', `Thanh toan ${amount} ${currency}?`)
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.isPaymentLoading = true;
        this.walletService.payment(this.selectedWallet!.walletId, amount!, reference!, totpCode!).subscribe({
          next: () => {
            this.isPaymentLoading = false;
            this.refreshSelected();
            this.notify.success('Payment thanh cong.');
          },
          error: () => {
            this.isPaymentLoading = false;
            this.errorMessage = 'Payment that bai.';
            this.notify.error(this.errorMessage);
          }
        });
      });
  }

  refund(): void {
    if (!this.selectedWallet || this.refundForm.invalid) {
      return;
    }

    const { amount, reference, totpCode } = this.refundForm.getRawValue();
    this.isRefundLoading = true;
    this.walletService.refund(this.selectedWallet.walletId, amount!, reference!, totpCode!).subscribe({
      next: () => {
        this.isRefundLoading = false;
        this.refreshSelected();
        this.notify.success('Refund thanh cong.');
      },
      error: () => {
        this.isRefundLoading = false;
        this.errorMessage = 'Refund that bai.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  transfer(): void {
    if (this.transferForm.invalid) {
      return;
    }

    const { fromWalletId, toWalletId, amount, reference, totpCode } = this.transferForm.getRawValue();
    this.notify
      .confirm('Xac nhan chuyen tien', `Chuyen ${amount} sang vi ${toWalletId}?`)
      .subscribe((confirmed) => {
        if (!confirmed) {
          return;
        }

        this.isTransferLoading = true;
        this.walletService.transfer(fromWalletId!, toWalletId!, amount!, reference!, totpCode!).subscribe({
          next: () => {
            this.isTransferLoading = false;
            this.refreshSelected();
            this.notify.success('Transfer thanh cong.');
          },
          error: () => {
            this.isTransferLoading = false;
            this.errorMessage = 'Transfer that bai.';
            this.notify.error(this.errorMessage);
          }
        });
      });
  }

  downloadReceipt(tx: Transaction): void {
    if (!this.selectedWallet) {
      return;
    }

    this.receiptLoadingId = tx.transactionId;
    this.walletService.generateReceipt(this.selectedWallet.walletId, tx.transactionId).subscribe({
      next: (result) => {
        this.receiptLoadingId = null;
        window.open(result.url, '_blank');
        this.notify.success('Da tao receipt.');
      },
      error: () => {
        this.receiptLoadingId = null;
        this.errorMessage = 'Khong the tao receipt.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  exportTransactions(): void {
    if (!this.selectedWallet || this.exportForm.invalid) {
      return;
    }

    const totpCode = this.exportForm.getRawValue().totpCode!;
    this.isExportLoading = true;
    this.walletService.exportTransactions(this.selectedWallet.walletId, totpCode).subscribe({
      next: (blob) => {
        this.isExportLoading = false;
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `transactions-${this.selectedWallet!.walletId}.xlsx`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.notify.success('Da export Excel.');
      },
      error: () => {
        this.isExportLoading = false;
        this.errorMessage = 'Export that bai.';
        this.notify.error(this.errorMessage);
      }
    });
  }

  refreshSelected(): void {
    if (!this.selectedWallet) {
      return;
    }

    this.walletService.getWallet(this.selectedWallet.walletId).subscribe({
      next: (wallet) => {
        this.selectedWallet = wallet;
        this.loadTransactions(wallet.walletId);
      }
    });
  }
}
