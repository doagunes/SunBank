import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TransferPayload, TransferService } from '../services/transfer.service';

@Component({
  selector: 'app-transfer-money',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './transfer-money.html',
  styleUrls: ['./transfer-money.css']
})
export class TransferMoneyComponent {
  ibanTo: string = '';
  recipientFirstName: string = '';
  recipientLastName: string = '';
  amount: number | null = null;
  note: string = '';

  isSubmitting = false;
  message = '';

  constructor(private transferService: TransferService) {}

  submitTransfer() {
    if (!this.ibanTo || !this.recipientFirstName || !this.recipientLastName || !this.amount || this.amount <= 0) {
      this.message = 'Please fill in all required fields with valid values.';
      return;
    }

    const senderUserId = Number(localStorage.getItem('userId'));
    if (!senderUserId) {
      this.message = 'User not logged in.';
      return;
    }

    this.isSubmitting = true;
    this.message = '';

    const transferData: TransferPayload = {
      senderUserId,
      recipientIban: this.ibanTo,
      recipientFirstName: this.recipientFirstName,
      recipientLastName: this.recipientLastName,
      amount: this.amount,
      note: this.note
    };

    this.transferService.makeTransfer(transferData).subscribe({
      next: (res: any) => {
        this.message = 'Transfer successful!';
        this.isSubmitting = false;
        // Formu temizle
        this.ibanTo = '';
        this.recipientFirstName = '';
        this.recipientLastName = '';
        this.amount = null;
        this.note = '';
      },
      error: (err: { error: any }) => {
        this.message = 'Transfer failed: ' + (err.error ?? 'Unknown error');
        this.isSubmitting = false;
      }
    });
  }
}

