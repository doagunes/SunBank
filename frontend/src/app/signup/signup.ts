import { Component, ViewChild, ElementRef, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http'; 

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './signup.html',
  styleUrls: ['./signup.css'],
})
export class SignupComponent {
  http = inject(HttpClient); 
  firstName: string = '';
  lastName: string = '';
  tc: string = '';
  email: string = '';
  password: string = '';
  phoneNumber: string = '';
  confirmPassword: string = '';
  showPassword: boolean = false;
  
  passwordRules = {
    length: false,
    uppercase: false,
    lowercase: false,
    number: false,
  };

  emailRules = {
    notEmpty: false,
    hasAtSymbol: false,
    hasDotAfterAt: false,
  };

  onPasswordInput() {
    const password = this.password;
    this.passwordRules.length = password.length >= 8;
    this.passwordRules.uppercase = /[A-Z]/.test(password);
    this.passwordRules.lowercase = /[a-z]/.test(password);
    this.passwordRules.number = /\d/.test(password);
  }

  onEmailInput() {
    const email = this.email;
    this.emailRules.notEmpty = email.trim().length > 0;
    this.emailRules.hasAtSymbol = email.includes('@');
    this.emailRules.hasDotAfterAt = email.includes('@') && email.split('@')[1]?.includes('.');
  }

  isValidPhoneNumber(phone: string): boolean {
    return /^05\d{9}$/.test(phone); 
  }
  
  isPasswordValid(): boolean {
    return (
      this.passwordRules.length &&
      this.passwordRules.uppercase &&
      this.passwordRules.lowercase &&
      this.passwordRules.number
    );
  }

  getPasswordStrength(): number {
    if (this.password.length === 0) return 0;
    
    let strength = 0;
    if (this.passwordRules.length) strength += 25;
    if (this.passwordRules.uppercase) strength += 25;
    if (this.passwordRules.lowercase) strength += 25;
    if (this.passwordRules.number) strength += 25;
    
    return strength;
  }

  isFormValid(): boolean {
    return (
      this.firstName.trim().length > 0 &&
      this.lastName.trim().length > 0 &&
      this.isValidTCKN(this.tc) &&
      this.isValidEmail(this.email) &&
      this.isValidPhoneNumber(this.phoneNumber) &&
      this.isPasswordValid() &&
      this.password === this.confirmPassword &&
      this.confirmPassword.length > 0
    );
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

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  signup() {
    if (!this.firstName.trim() || !this.lastName.trim()) {
      alert('First and Last Name are required.');
      return;
    }
    
    if (!this.isValidTCKN(this.tc)) {
      alert('Invalid TC Identity Number.');
      return;
    }

    if (this.password !== this.confirmPassword) {
      alert('Passwords do not match');
      return;
    }

    if (!this.isValidEmail(this.email)) {
      alert('Invalid email address.');
      return;
    }

    if (!this.isPasswordValid()) {
      alert('Password must meet all requirements.');
      return;
    }

    if (!this.isValidPhoneNumber(this.phoneNumber)) {
      alert('Invalid phone number. It must start with 05 and be 11 digits.');
      return;
    }
    
    const payload = {
      firstName: this.firstName,
      lastName: this.lastName,
      tc: this.tc,
      email: this.email,
      password: this.password,
      phoneNumber: this.phoneNumber
    };

    this.http.post('http://localhost:5191/api/signup', payload).subscribe({
      next: () => {
        alert('Signup!');
        this.firstName = '';
        this.lastName = '';
        this.tc = '';
        this.email = '';
        this.password = '';
        this.confirmPassword = '';
        this.phoneNumber = ''; 
      },
      error: (err) => {
        if (err.status === 400 || err.status === 409) {
          alert(err.error);
        } else {
          alert('Error!');
          console.error(err);
        }
      }
    });
  }
}
