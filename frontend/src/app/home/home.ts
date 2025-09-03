import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { NewsService } from '../services/news.service';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { ChatbotComponent } from '../chatbot/chatbot';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, HttpClientModule, RouterModule, ChatbotComponent],
  templateUrl: './home.html',
  styleUrls: ['./home.css'] 
})
export class HomeComponent implements OnInit {
  newsArticles: any[] = [];
  @ViewChild('newsContainer') newsContainer!: ElementRef;
  
  // Scroll properties
  canScrollLeft = false;
  canScrollRight = true;
  
  // Chat properties
  isChatOpen = false;

  constructor(
    private newsService: NewsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.newsService.getBusinessNews().subscribe({
      next: (data) => {
        this.newsArticles = data?.articles || [];
        // Initialize image properties for each article
        this.newsArticles.forEach(article => {
          article.imageLoaded = false;
          article.imageError = false;
        });
        this.updateScrollButtons();
      },
      error: (err) => {
        console.error('Cannot Get News:', err);
      }
    });
  }

  openChatbot() {
    this.isChatOpen = true;
  }

  // News scrolling methods
  scrollNews(direction: 'left' | 'right') {
    if (!this.newsContainer) return;
    
    const container = this.newsContainer.nativeElement;
    const scrollAmount = 300;
    
    if (direction === 'left') {
      container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
      container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
    
    // Update scroll buttons after scrolling
    setTimeout(() => this.updateScrollButtons(), 300);
  }

  updateScrollButtons() {
    if (!this.newsContainer) return;
    
    const container = this.newsContainer.nativeElement;
    this.canScrollLeft = container.scrollLeft > 0;
    this.canScrollRight = container.scrollLeft < (container.scrollWidth - container.clientWidth);
  }

  // Image handling methods
  getImageUrl(article: any): string {
    return article.urlToImage || 'assets/default-news-image.jpg';
  }

  onImageError(event: any) {
    const img = event.target;
    const article = this.newsArticles.find(a => a.urlToImage === img.src);
    if (article) {
      article.imageError = true;
    }
  }

  onImageLoad(event: any) {
    const img = event.target;
    const article = this.newsArticles.find(a => a.urlToImage === img.src);
    if (article) {
      article.imageLoaded = true;
    }
  }

  // Chat methods
  closeChatbot(event?: Event) {
    if (event) {
      event.stopPropagation();
    }
    this.isChatOpen = false;
  }
}