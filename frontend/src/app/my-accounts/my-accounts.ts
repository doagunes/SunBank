import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccountService } from '../services/account.service';

@Component({
  selector: 'app-my-accounts',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './my-accounts.html',
  styleUrls: ['./my-accounts.css'],
})

export class MyAccountsComponent implements OnInit {
  iban: string | null = null;
  balance: number = 0;

  constructor(private accountService: AccountService) {}

  ngOnInit() {
    const userId = localStorage.getItem('userId');
    if (userId) {
      this.accountService.getAccountByUserId(+userId).subscribe({
        next: (data: { iban: string | null; balance: number; }) => {
          this.iban = data.iban;
          this.balance = data.balance;
        },
        error: (err: any) => {
          console.error('Hesap bilgisi alınamadı:', err);
        }
      });
    }
  }
}
