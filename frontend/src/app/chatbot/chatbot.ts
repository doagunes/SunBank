import { Component } from '@angular/core';
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
export class ChatbotComponent {
  messages: { from: 'user' | 'bot'; text: string }[] = [];
  userInput = '';
  isLoading = false;

  constructor(private http: HttpClient) {}

  sendMessage() {
    if (!this.userInput.trim()) return; //Kullanıcı boş bir mesaj göndermek isterse, işlem yapılmaz.

    const prompt = this.userInput;
    this.messages.push({ from: 'user', text: prompt });
    this.userInput = '';
    this.isLoading = true;

    this.http
    //prompt: POST isteğinde gönderilen kullanıcı mesajı.
    //localhost:5191/api/chat: Backend API adresi (örneğin senin .NET backend’in).
    .post('http://localhost:5191/api/chat', { prompt }, { responseType: 'text' })

    //Cevabı Alma ve Ekleme
    .subscribe({
    next: (response: string) => {
      //Gelen yanıt JSON.parse() ile çözülür (çünkü responseType: 'text' denmiş).
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
