import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NewsService {

  private baseUrl = 'http://localhost:5191/api/news'; 

  constructor(private http: HttpClient) {}

  getBusinessNews(): Observable<any> {
    return this.http.get(`${this.baseUrl}/business`);
  }
}
