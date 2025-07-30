import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface Loan {
  id: number;
  userId: number;
  amount: number;
  applicationDate: string;
  isApproved: boolean;
  isActive: boolean;
  term: number; // ✅ Vade bilgisi
}

@Injectable({
  providedIn: 'root'
})
export class LoanService {
  private baseUrl = 'http://localhost:5191/api/loan'; 

  constructor(private http: HttpClient) {}

  applyLoan(userId: number, amount: number, term: number): Observable<any> {
    const body = { userId, amount, term };
    return this.http.post(`${this.baseUrl}/apply`, body);
  }

  getUserLoans(userId: number, active?: boolean): Observable<any> {
    let params = new HttpParams();
    if (active !== undefined) {
      params = params.set('active', active);
    }
    return this.http.get(`${this.baseUrl}/user/${userId}`, { params });
  }

  getLoanHistory(userId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/${userId}`);
  }

  closeLoan(loanId: number): Observable<any> {
    return this.http.put(`${this.baseUrl}/close/${loanId}`, null);
  }
}
