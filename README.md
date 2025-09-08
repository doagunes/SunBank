# SunBank

SunBank is a modern internet banking application designed to combine security, convenience, and AI-powered financial assistance. It offers users a full digital banking experience with account management, money transfers, loan applications, AI chatbot support, and real-time financial insights. These insights include up-to-date financial news powered by NewsAPI and AI-generated explanations and suggestions through the integrated Gemini-powered chatbot, helping users stay informed and make better financial decisions.

## Technologies Used

### Frontend

Angular (Standalone Components)

Angular Material

CSS (Sunset gradient theme, responsive UI)

Bootstrap (for additional styling components)

### Backend

ASP.NET Core Minimal APIs (C#)

DTO-based request/response handling

### Database

SQLite (for development and data storage)

### AI & APIs

Google Gemini API (AI-powered chatbot assistant – Sunny)

NewsAPI (real-time economy & finance news)

### Email Integration

System.Net.Mail.SmtpClient (for password reset & notification emails)

### Other Tools & Services

Git (Version Control)

Postman (API testing)

## Features

### User Authentication
Secure login and signup with validation for national ID, email, and password.
<img width="900" height="900" alt="image" src="https://github.com/user-attachments/assets/46108f20-1965-44af-b975-5af17d23c514" />
<img width="1314" height="1660" alt="image" src="https://github.com/user-attachments/assets/78637b08-dc13-4afe-9f66-ae91cb1d42e6" />

### Forgot Password Flow
Reset password functionality with email verification and secure token validation.
<img width="1226" height="1102" alt="image" src="https://github.com/user-attachments/assets/0e4fddc3-44ee-4715-a24e-99bca63d4385" />
<img width="1002" height="548" alt="image" src="https://github.com/user-attachments/assets/d7f80c38-ab5f-4b33-ad55-a2f784c4268b" />
<img width="1260" height="1272" alt="image" src="https://github.com/user-attachments/assets/186f601c-17c2-43ae-be8c-aba2fded74f7" />

### Email Notifications
Password reset links and critical notifications are sent via email using SMTP (System.Net.Mail.SmtpClient).
<img width="2088" height="164" alt="image" src="https://github.com/user-attachments/assets/bb6cb3a4-6f4b-4f28-9b96-0d856872d18e" />

### Guest Mode
Explore banking features without logging in, including AI demo chat, live news (latest economy & finance news fetched via NewsAPI.).
<img width="2782" height="1570" alt="image" src="https://github.com/user-attachments/assets/13173f5a-334f-41d4-88fb-3c7d4dafecfc" />

### AI Chatbot – Sunny 
An AI-powered banking assistant integrated with Google Gemini API, providing instant responses to banking queries and personalized guidance.
<img width="2856" height="1456" alt="image" src="https://github.com/user-attachments/assets/31bbece4-9deb-4565-9c8c-82b158d45071" />
<img width="880" height="1216" alt="image" src="https://github.com/user-attachments/assets/71ecf6f7-4740-4aa1-99b5-95e6b1884102" />

### My Accounts
After login, view account balances, transaction history, and manage multiple banking accounts.
<img width="2870" height="1590" alt="image" src="https://github.com/user-attachments/assets/3074d4ba-59f3-480b-825f-0263a05dde5a" />

### Transfer Money
Send money to other accounts, make international transfers, and manage recurring payments.
<img width="2874" height="1636" alt="image" src="https://github.com/user-attachments/assets/ab0ac678-ebd5-4899-af1f-13def3dca674" />
<img width="816" height="496" alt="image" src="https://github.com/user-attachments/assets/62794e27-21fd-418f-8214-35666b793d8e" />

### Bill Payments
Pay bills, set up automatic payments, and track your expenses.
<img width="2884" height="1568" alt="image" src="https://github.com/user-attachments/assets/df190fbe-c389-43ff-9268-2d7218187a29" />

### Loan Application
Apply for personal loans, mortgages, and financing options with competitive rates.
<img width="2882" height="1598" alt="image" src="https://github.com/user-attachments/assets/29db3dc3-d2f8-472f-91ce-3c248d38ae59" />

### Credit Card Management
View statements, manage credit cards, and track your spending.
<img width="2882" height="1520" alt="image" src="https://github.com/user-attachments/assets/8b125ad8-3b36-4459-8b50-c6ee1b838fa7" />
<img width="2880" height="1334" alt="image" src="https://github.com/user-attachments/assets/8c49b368-b223-45b7-b827-7c19222ebe6f" />
