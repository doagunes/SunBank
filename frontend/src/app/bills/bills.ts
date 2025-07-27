import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Bill, BillService } from '../services/bill.service';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-bills',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, HttpClientModule],
  templateUrl: './bills.html',
  styleUrls: ['./bills.css'],
})
export class BillsComponent implements OnInit {
  bills: Bill[] = [];
  userId = 7; // Örnek, burayı dinamik yapabilirsin

  constructor(private billService: BillService) {}

  ngOnInit(): void {
    this.loadBills();
  }

  loadBills(): void {
    this.billService.getUserBills(this.userId).subscribe(data => {
      this.bills = data;
    });
  }

  payBill(id: number): void {
    this.billService.payBill(id).subscribe(() => {
      alert('Fatura ödendi!');
      this.loadBills();
    });
  }
}

