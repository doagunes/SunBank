import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HeaderComponent } from './header/header';
import { UserHeaderComponent } from './user-header/user-header';
import { AuthService } from './services/auth.service';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule, HeaderComponent, UserHeaderComponent, NgIf],
  templateUrl: './app.html',
  styleUrls: ['./app.css'],
})
export class App {
  constructor(public authService: AuthService) {}
}
