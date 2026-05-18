import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatDialog } from '@angular/material/dialog';
import { Observable, map } from 'rxjs';
import { ConfirmDialogComponent, ConfirmDialogData } from '../shared/confirm-dialog/confirm-dialog.component';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly defaultDuration = 4000;

  constructor(private snackBar: MatSnackBar, private dialog: MatDialog) {}

  success(message: string): void {
    this.snackBar.open(message, 'Dong', {
      duration: this.defaultDuration,
      panelClass: ['toast-success']
    });
  }

  error(message: string): void {
    this.snackBar.open(message, 'Dong', {
      duration: this.defaultDuration,
      panelClass: ['toast-error']
    });
  }

  info(message: string): void {
    this.snackBar.open(message, 'Dong', {
      duration: this.defaultDuration,
      panelClass: ['toast-info']
    });
  }

  confirm(
    title: string,
    message: string,
    confirmText = 'Xac nhan',
    cancelText = 'Huy'
  ): Observable<boolean> {
    const data: ConfirmDialogData = { title, message, confirmText, cancelText };
    return this.dialog
      .open(ConfirmDialogComponent, { width: '360px', data })
      .afterClosed()
      .pipe(map((result) => !!result));
  }
}
