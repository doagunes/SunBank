import { Component, OnInit } from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Bill, BillService } from '../services/bill.service';
import { HttpClientModule } from '@angular/common/http';

@Component({
  selector: 'app-bills',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe, HttpClientModule, RouterModule],
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

  // Overview Methods
  getTotalBills(): number {
    return this.bills.reduce((total, bill) => total + bill.amount, 0);
  }

  getPendingBills(): number {
    return this.bills.filter(bill => !bill.isPaid).length;
  }

  getPaidBills(): number {
    return this.bills.filter(bill => bill.isPaid).length;
  }

  getOverdueBills(): number {
    const today = new Date();
    return this.bills.filter(bill => 
      !bill.isPaid && new Date(bill.dueDate) < today
    ).length;
  }

  // Bill Card Methods
  getBillStatusClass(bill: Bill): string {
    if (bill.isPaid) return 'paid';
    const today = new Date();
    const dueDate = new Date(bill.dueDate);
    if (dueDate < today) return 'overdue';
    return 'pending';
  }

  getBillIcon(bill: Bill): string {
    // You can customize this based on bill type or name
    const billTypes = {
      'electricity': '⚡',
      'water': '💧',
      'gas': '🔥',
      'internet': '🌐',
      'rent': '🏠',
      'insurance': '🛡️',
      'phone': '📱',
      'default': '📄'
    };
    
    const name = bill.name?.toLowerCase() || '';
    for (const [type, icon] of Object.entries(billTypes)) {
      if (name.includes(type)) return icon;
    }
    return billTypes.default;
  }

  getBillTitle(bill: Bill): string {
    return bill.name || 'Bill Payment';
  }

  getBillDescription(bill: Bill): string {
    return bill.name || 'No description available';
  }
}

