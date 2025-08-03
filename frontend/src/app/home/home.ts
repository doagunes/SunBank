import { Component, OnInit, ViewChild, ElementRef, AfterViewInit, HostListener } from '@angular/core';
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
export class HomeComponent implements OnInit, AfterViewInit {
  newsArticles: any[] = [];
  isChatOpen = false;
  canScrollLeft = false;
  canScrollRight = false;

  @ViewChild('newsContainer') newsContainer!: ElementRef;

  constructor(
    private newsService: NewsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.newsService.getBusinessNews().subscribe({
      next: (data) => {
        this.newsArticles = data?.articles || [];
        // Initialize image states for each article
        this.newsArticles.forEach(article => {
          article.imageLoaded = false;
          article.imageError = false;
        });
        // Update scroll buttons after articles load
        setTimeout(() => {
          this.updateScrollButtons();
        }, 100);
      },
      error: (err) => {
        console.error('Cannot Get News:', err);
      }
    });
  }

  ngAfterViewInit(): void {
    // Initial update after view is ready
    setTimeout(() => {
      this.updateScrollButtons();
    }, 100);
  }

  @HostListener('window:resize')
  onResize(): void {
    this.updateScrollButtons();
  }

  getImageUrl(article: any): string {
    // Check if the article has a valid image URL
    if (article.urlToImage && article.urlToImage.trim() !== '') {
      return article.urlToImage;
    }
    
    // Return a default placeholder image
    return 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMzIwIiBoZWlnaHQ9IjIyMCIgdmlld0JveD0iMCAwIDMyMCAyMjAiIGZpbGw9Im5vbmUiIHhtbG5zPSJodHRwOi8vd3d3LnczLm9yZy8yMDAwL3N2ZyI+CjxyZWN0IHdpZHRoPSIzMjAiIGhlaWdodD0iMjIwIiBmaWxsPSIjRkZGRkZGIi8+CjxyZWN0IHdpZHRoPSIzMjAiIGhlaWdodD0iMjIwIiBmaWxsPSJ1cmwoI2dyYWRpZW50KSIvPgo8ZGVmcz4KPGxpbmVhckdyYWRpZW50IGlkPSJncmFkaWVudCIgeDE9IjAiIHkxPSIwIiB4Mj0iMzIwIiB5Mj0iMjIwIiBncmFkaWVudFVuaXRzPSJ1c2VyU3BhY2VPblVzZSI+CjxzdG9wIG9mZnNldD0iMCUiIHN0eWxlPSJzdG9wLWNvbG9yOiNmZmY5YzQ7c3RvcC1vcGFjaXR5OjEiLz4KPHN0b3Agb2Zmc2V0PSI1MCUiIHN0eWxlPSJzdG9wLWNvbG9yOiNmZmY1OWQ7c3RvcC1vcGFjaXR5OjEiLz4KPHN0b3Agb2Zmc2V0PSIxMDAlIiBzdHlsZT0ic3RvcC1jb2xvcjojZmZlYjNiO3N0b3Atb3BhY2l0eToxIi8+CjwvbGluZWFyR3JhZGllbnQ+CjwvZGVmcz4KPC9zdmc+';
  }

  onImageError(event: any): void {
    const img = event.target;
    const article = this.newsArticles.find(a => a.urlToImage === img.src);
    if (article) {
      article.imageError = true;
      article.imageLoaded = false;
    }
  }

  onImageLoad(event: any): void {
    const img = event.target;
    const article = this.newsArticles.find(a => a.urlToImage === img.src);
    if (article) {
      article.imageLoaded = true;
      article.imageError = false;
    }
  }

  scrollNews(direction: 'left' | 'right'): void {
    const container = this.newsContainer.nativeElement;
    const scrollAmount = 350; // Slightly smaller than card width for smooth scrolling
    
    if (direction === 'left') {
      container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
    } else {
      container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
    }
    
    // Update button states after scroll animation completes
    setTimeout(() => {
      this.updateScrollButtons();
    }, 600); // Increased timeout to account for smooth scroll duration
  }

  updateScrollButtons(): void {
    const container = this.newsContainer.nativeElement;
    if (container) {
      const scrollLeft = container.scrollLeft;
      const scrollWidth = container.scrollWidth;
      const clientWidth = container.clientWidth;
      
      // Enable left button if we can scroll left
      this.canScrollLeft = scrollLeft > 0;
      
      // Enable right button if we can scroll right
      this.canScrollRight = scrollLeft < (scrollWidth - clientWidth - 1); // -1 for rounding issues
      
      console.log('Scroll state:', {
        scrollLeft,
        scrollWidth,
        clientWidth,
        canScrollLeft: this.canScrollLeft,
        canScrollRight: this.canScrollRight
      });
    }
  }

  openChatbot(): void {
    this.isChatOpen = true;
    // Prevent body scroll when modal is open
    document.body.style.overflow = 'hidden';
  }

  closeChatbot(event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    this.isChatOpen = false;
    // Restore body scroll
    document.body.style.overflow = '';
  }
}