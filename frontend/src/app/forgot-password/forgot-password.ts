import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.html',
  styleUrls: ['./forgot-password.css'],
  standalone: true,
  imports: [CommonModule, FormsModule],
})
export class ForgotPasswordComponent {
  tc: string = '';
  email: string = '';
  emailSent = false;

  constructor(private http: HttpClient) {}

  submitRequest() {
    const payload = {
      tc: this.tc,
      email: this.email,
    };

    this.http.post('http://localhost:5191/api/forgot-password', payload).subscribe({
      next: () => {
        this.emailSent = true;
      },
      error: (err) => {
        alert(err.error || 'An error occurred.');
        console.error(err);
      },
    });
  }
}
