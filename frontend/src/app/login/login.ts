import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
})
export class LoginComponent {
  tc: string = '';
  password: string = '';
  showPassword: boolean = false;

  private authService = inject(AuthService);
  private router = inject(Router);

  togglePasswordVisibility() {
    this.showPassword = !this.showPassword;
  }

  login() {
    const payload = {
      tc: this.tc,
      password: this.password,
    };
  
    this.authService.login(payload).subscribe({
      next: () => {
        alert('Login successful!');
        this.router.navigate(['/dashboard']);
      },
      error: (err: any) => {
        alert('Login failed');
        console.error(err);
      },
    });
  }
  

  onForgotPassword(event: Event) {
    event.preventDefault();
    this.router.navigate(['/forgot-password']);
  }
}
