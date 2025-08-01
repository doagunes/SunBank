import { Component, OnInit } from '@angular/core';
import { NewsService } from '../services/news.service';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HttpClientModule],
  templateUrl: './home.html',
  styleUrls: ['./home.css'] 
})
export class HomeComponent implements OnInit {
  newsArticles: any[] = [];

  constructor(
    private newsService: NewsService,
  ) {}

  ngOnInit(): void {
    this.newsService.getBusinessNews().subscribe({
      next: (data) => {
        this.newsArticles = data?.articles || [];
      },
      error: (err) => {
        console.error('Cannot Get News:', err);
      }
    });

  }
}