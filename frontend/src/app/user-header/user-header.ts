import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-user-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-header.html',
  styleUrls: ['./user-header.css']
})
export class UserHeaderComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  goToMyAccounts() {
    this.router.navigate(['/my-accounts']);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/home']);
  }
}
