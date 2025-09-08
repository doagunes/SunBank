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
![image](https://github.com/user-attachments/assets/80a4306c-1b3a-45bc-9ddb-4135a8253975)
![image](https://github.com/user-attachments/assets/80a4306c-1b3a-45bc-9ddb-4135a8253975)

### Forgot Password Flow
Reset password functionality with email verification and secure token validation.
![image](https://github.com/user-attachments/assets/80a4306c-1b3a-45bc-9ddb-4135a8253975)
<img width="1002" height="548" alt="image" src="https://github.com/user-attachments/assets/d7f80c38-ab5f-4b33-ad55-a2f784c4268b" />
<img width="1260" height="1272" alt="image" src="https://github.com/user-attachments/assets/186f601c-17c2-43ae-be8c-aba2fded74f7" />

### Email Notifications
Password reset links and critical notifications are sent via email using SMTP (System.Net.Mail.SmtpClient).

### Guest Mode
Explore banking features without logging in, including AI demo chat, live news, and currency updates.

### AI Chatbot – Sunny 
An AI-powered banking assistant integrated with Google Gemini API, providing instant responses to banking queries and personalized guidance.

### My Accounts
View account balances, transaction history, and manage multiple banking accounts.

### Transfer Money
Send money to other accounts, make international transfers, and manage recurring payments.

### Bill Payments
Pay bills, set up automatic payments, and track your expenses.

### Loan Application
Apply for personal loans, mortgages, and financing options with competitive rates.

### Credit Card Management
View statements, manage credit cards, and track your spending.

### Investments
Explore investment opportunities, manage your portfolio, and grow your wealth.

### Currency Tracking
Real-time exchange rates powered by CurrencyFreaks API.

### News Updates
Latest economy & finance news fetched via NewsAPI.

### Modern UI/UX
Responsive Angular frontend with a warm sunset yellow/orange theme, smooth animations, and a clean dashboard design.


 
