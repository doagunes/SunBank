import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './reset-password.html',
  styleUrls: ['./reset-password.css'],
})
export class ResetPasswordComponent {
  http = inject(HttpClient);
  router = inject(Router);

  tc: string = '';
  newPassword: string = '';
  confirmNewPassword: string = '';

  resetPassword() {
    if (!this.isValidTCKN(this.tc)) {
      alert('Invalid Turkish ID number.');
      return;
    }

    if (this.newPassword !== this.confirmNewPassword) {
      alert('The new passwords do not match.');
      return;
    }

    if (!this.isValidPassword(this.newPassword)) {
      alert('Password must be at least 8 characters and contain uppercase letters, lowercase letters and numbers.');
      return;
    }

    const payload = {
      tc: this.tc,
      newPassword: this.newPassword,
    };

    this.http.post('http://localhost:5191/api/reset-password', payload).subscribe({
      next: () => {
        alert('Password reset successfully!');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        alert(err.error || 'Password reset failed.');
        console.error(err);
      },
    });
  }

  isValidTCKN(tc: string): boolean {
    if (!/^[1-9][0-9]{10}$/.test(tc)) return false;
    const digits = tc.split('').map(Number);
    const sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
    const sumEven = digits[1] + digits[3] + digits[5] + digits[7];
    const digit10 = ((sumOdd * 7) - sumEven) % 10;
    const sumFirst10 = digits.slice(0, 10).reduce((a, b) => a + b, 0);
    const digit11 = sumFirst10 % 10;
    return digit10 === digits[9] && digit11 === digits[10];
  }

  isValidPassword(password: string): boolean {
    return (
      password.length >= 8 &&
      /[A-Z]/.test(password) &&
      /[a-z]/.test(password) &&
      /\d/.test(password)
    );
  }
}
