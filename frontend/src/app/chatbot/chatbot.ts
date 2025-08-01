import { Component, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.html',
  styleUrls: ['./chatbot.css'],
})
export class ChatbotComponent implements AfterViewChecked {
  @ViewChild('chatMessages') private chatMessages!: ElementRef;
  
  messages: { from: 'user' | 'bot'; text: string }[] = [];
  userInput = '';
  isLoading = false;

  constructor(private http: HttpClient) {}

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  scrollToBottom(): void {
    try {
      this.chatMessages.nativeElement.scrollTop = this.chatMessages.nativeElement.scrollHeight;
    } catch(err) { }
  }

  getCurrentTime(): string {
    return new Date().toLocaleTimeString('en-US', { 
      hour: '2-digit', 
      minute: '2-digit',
      hour12: true 
    });
  }

  sendMessage() {
    if (!this.userInput.trim()) return;
  
    const userText = this.userInput.trim();
    this.messages.push({ from: 'user', text: userText });
    this.userInput = '';
    this.isLoading = true;
  
    // Backend'e gönderilecek format
    const backendMessages = this.messages.map((msg) => ({
      role: msg.from === 'user' ? 'user' : 'bot', // Backend'de bot -> model dönüşümü yapılacak
      text: msg.text
    }));
  
    this.http
      .post('http://localhost:5191/api/chat', { messages: backendMessages }, { responseType: 'text' })
      .subscribe({
        next: (response: string) => {
          let message = JSON.parse(response);
          this.messages.push({ from: 'bot', text: message.response });
          this.isLoading = false;
        },
        error: () => {
          this.messages.push({ from: 'bot', text: '⚠️ Server error.' });
          this.isLoading = false;
        },
      });
  }
  
}