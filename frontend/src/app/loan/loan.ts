import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoanService, Loan } from '../services/loan.service';

@Component({
  selector: 'app-loan',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule], // Gerekli modülleri import ettik
  templateUrl: './loan.html',
  styleUrls: ['./loan.css'],
})
export class LoanComponent implements OnInit {
  userId: number = Number(localStorage.getItem('userId'));
  loanForm: FormGroup; // FormGroup tanımladık
  activeLoans: Loan[] = [];
  loanHistory: Loan[] = [];

  constructor(
    private loanService: LoanService,
    private fb: FormBuilder // FormBuilder inject ettik
  ) {
    // FormGroup'u constructor'da oluşturduk
    this.loanForm = this.fb.group({
      amount: [0, [Validators.required, Validators.min(1)]],
      term: [6, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    this.loadActiveLoans();
    this.loadLoanHistory();
  }

  applyLoan(): void {
    if (this.loanForm.invalid) {
      alert('Lütfen geçerli bir kredi tutarı giriniz.');
      return;
    }

    const formValue = this.loanForm.value;
    this.loanService.applyLoan(this.userId, formValue.amount, formValue.term).subscribe({
      next: () => {
        alert('Kredi başvurusu başarılı!');
        this.loadActiveLoans();
        this.loadLoanHistory();
        this.loanForm.reset({ amount: 0, term: 6 }); // form temizle
      },
      error: (err: any) => {
        alert('Kredi başvurusunda hata oluştu.');
        console.error(err);
      }
    });
  }

  loadActiveLoans(): void {
    this.loanService.getUserLoans(this.userId, true).subscribe({
      next: (loans: Loan[]) => {
        this.activeLoans = loans;
      },
      error: (err: any) => {
        console.error('Aktif krediler alınamadı:', err);
      }
    });
  }

  loadLoanHistory(): void {
    this.loanService.getLoanHistory(this.userId).subscribe({
      next: (loans: Loan[]) => {
        this.loanHistory = loans;
      },
      error: (err: any) => {
        console.error('Kredi geçmişi alınamadı:', err);
      }
    });
  }

  closeLoan(loanId: number): void {
    this.loanService.closeLoan(loanId).subscribe({
      next: () => {
        alert('Kredi başarıyla kapatıldı.');
        this.loadActiveLoans();
        this.loadLoanHistory();
      },
      error: (err: any) => {
        alert('Kredi kapatma işleminde hata oluştu.');
        console.error(err);
      }
    });
  }
}