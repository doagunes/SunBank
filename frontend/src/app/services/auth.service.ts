import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

export interface SignupData {
  tc: string;
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
}

export interface LoginData {
  tc: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private signupUrl = 'http://localhost:5191/api/signup';
  private loginUrl = 'http://localhost:5191/api/login';

  constructor(private http: HttpClient) {}

  signup(data: SignupData): Observable<any> {
    return this.http.post(this.signupUrl, data);
  }

  login(data: LoginData): Observable<any> {
    return this.http.post<any>(this.loginUrl, data).pipe(
      tap((res) => {
        if (res.token) {
          localStorage.setItem('token', res.token);
        }
        if (res.userId !== undefined && res.userId !== null) {
          localStorage.setItem('userId', res.userId.toString());
        }
        localStorage.setItem('isLoggedIn', 'true');
      })
    );
  }
  
  
  logout() {
    localStorage.removeItem('isLoggedIn');
  }

  isLoggedIn(): boolean {
    return localStorage.getItem('isLoggedIn') === 'true';
  }
}
