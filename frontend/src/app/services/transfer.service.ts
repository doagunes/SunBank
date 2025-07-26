import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface TransferPayload {
    senderUserId: number;
    recipientIban: string;
    recipientFirstName: string;
    recipientLastName: string;
    amount: number;
    note?: string;
  }
  
  @Injectable({
    providedIn: 'root'
  })
  export class TransferService {
    private apiUrl = 'http://localhost:5191/api/transfer';
  
    constructor(private http: HttpClient) {}
  
    makeTransfer(payload: TransferPayload): Observable<any> {
      return this.http.post(this.apiUrl, payload);
    }
  }
  
