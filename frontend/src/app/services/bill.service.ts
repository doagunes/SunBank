import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Bill {
  id: number;
  name: string;
  amount: number;
  dueDate: string; // Backend'den JSON string olarak gelir
  isPaid: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class BillService {
  private apiUrl = 'http://localhost:5191/api/bills'; 

  constructor(private http: HttpClient) {}

  getUserBills(userId: number): Observable<Bill[]> {
    return this.http.get<Bill[]>(`http://localhost:5191/api/users/${userId}/bills`);
  }  

  payBill(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/pay`, {});
  }
}
