using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddSingleton<MailService>();
builder.Services.AddHttpClient();

builder.Services.AddScoped<GeminiService>();
builder.Services.AddScoped<NewsService>();


var config = builder.Configuration;

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
    };
});
builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHostedService<WeeklyBillGenerator>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=users.db"));

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

//otomatik iban oluşturuyorum
string GenerateIban(int userId)
{
    return $"TR{DateTime.Now.Ticks % 1_000_000_000_000_000}{userId}"
        .PadRight(26, '0')
        .Substring(0, 26);
}

app.MapPost("/api/login", async ([FromBody] LoginDto loginDto, [FromServices] AppDbContext db, IConfiguration config) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Tc == loginDto.Tc);

    if (user is null)
        return Results.BadRequest("No user found with this TC.");

    var hashedInputPassword = HashPassword(loginDto.Password);

    if (user.Password != hashedInputPassword)
        return Results.BadRequest("Incorrect password.");

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Tc),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.FirstName + " " + user.LastName)
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: config["Jwt:Issuer"],
        audience: null,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(2),
        signingCredentials: creds
    );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return Results.Ok(new { token = jwt,
    userId = user.Id });
})
.WithName("Login")
.WithTags("Auth");

// hesap oluşturma ekledim
app.MapPost("/api/signup", async (UserDto userDto, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(userDto.Tc) || string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
        return Results.BadRequest("All fields are required.");

    if (!IsValidTCKN(userDto.Tc))
        return Results.BadRequest("Invalid TC Identity Number.");

    if (!IsValidEmail(userDto.Email))
        return Results.BadRequest("Invalid Email format.");

    if (!IsValidPhone(userDto.PhoneNumber))
        return Results.BadRequest("Invalid Phone format.");

    if (!IsValidPassword(userDto.Password))
        return Results.BadRequest("Password must be at least 8 characters, include uppercase, lowercase, and a number.");

    if (await db.Users.AnyAsync(u => u.Email == userDto.Email))
        return Results.Conflict("Email already registered.");

    if (await db.Users.AnyAsync(u => u.PhoneNumber == userDto.PhoneNumber))
        return Results.Conflict("Phone Number already registered.");

    if (await db.Users.AnyAsync(u => u.Tc == userDto.Tc))
        return Results.Conflict("TC Identity Number already registered.");

    var hashedPassword = HashPassword(userDto.Password);

    var newUser = new User
    {
        Tc = userDto.Tc,
        Email = userDto.Email,
        Password = hashedPassword,
        FirstName = userDto.FirstName,
        LastName = userDto.LastName,
        PhoneNumber = userDto.PhoneNumber
    };

    db.Users.Add(newUser);
    await db.SaveChangesAsync(); // newUser.Id oluşur

    // Hesap oluştur
    var iban = GenerateIban(newUser.Id);
    var account = new Account
    {
        UserId = newUser.Id,
        Iban = iban,
        Balance = 1000 // Başlangıç bakiyesi
    };

    db.Accounts.Add(account);
    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "User and account created successfully",
        userId = newUser.Id,
        iban = iban,
        balance = account.Balance
    });
});

app.MapPost("/api/forgot-password", async (ForgotPasswordDto dto, AppDbContext db, MailService mailService) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Tc == dto.Tc && u.Email == dto.Email);

    if (user is null)
        return Results.BadRequest("No user found with provided TC and Email.");

    var resetLink = $"http://localhost:4200/reset-password?tc={user.Tc}";

    await mailService.SendResetPasswordEmailAsync(user.Email, resetLink);

    return Results.Ok("Password reset link sent to your email.");
});

app.MapPost("/api/reset-password", async (ResetPasswordDto dto, AppDbContext db) =>
{
    var user = await db.Users.FirstOrDefaultAsync(u => u.Tc == dto.Tc);
    if (user is null)
        return Results.BadRequest("No user found with provided TC.");

    if (!IsValidPassword(dto.NewPassword))
        return Results.BadRequest("Password must be at least 8 characters, include uppercase, lowercase, and a number.");

    user.Password = HashPassword(dto.NewPassword);
    await db.SaveChangesAsync();

    return Results.Ok("Password reset successfully.");
});

app.MapGet("/api/news/business", async (NewsService newsService) =>
{
    try
    {
        var json = await newsService.GetBusinessNewsAsync();
        return Results.Content(json, "application/json");
    }
    catch (Exception ex)
    {
        return Results.Problem($"Cannot Get News: {ex.Message}");
    }
})
.WithName("BusinessNews")
.WithTags("News");

app.MapPost("/api/chat", async (GeminiRequest req, GeminiService geminiService) =>
{
    try
    {
        if (req.Messages == null || !req.Messages.Any())
            return Results.BadRequest("Messages cannot be empty");

        var reply = await geminiService.GetGeminiResponse(req.Messages);
        return Results.Ok(new { response = reply });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error: {ex.Message}");
    }
}); 

app.MapGet("/api/account/{userId}", async (int userId, AppDbContext db) =>
{
    var account = await db.Accounts
        .FirstOrDefaultAsync(a => a.UserId == userId);

    if (account == null)
        return Results.NotFound("Account not found for this user.");

    return Results.Ok(new
    {
        iban = account.Iban,
        balance = account.Balance
    });
});

app.MapPost("/api/transfer", async (TransferDto transferDto, AppDbContext db,  MailService mailService) =>
{
    var senderAccount = await db.Accounts.FirstOrDefaultAsync(a => a.UserId == transferDto.SenderUserId);
    if (senderAccount == null)
        return Results.NotFound("Sender account not found.");

    var recipientAccount = await db.Accounts.FirstOrDefaultAsync(a => a.Iban == transferDto.RecipientIban);
    if (recipientAccount == null)
        return Results.NotFound("Recipient account not found.");

    var recipientUser = await db.Users.FirstOrDefaultAsync(u => u.Id == recipientAccount.UserId);
    if (recipientUser == null)
        return Results.NotFound("Recipient user not found.");

    // İsim soyisim kontrolü
    if (!string.Equals(recipientUser.FirstName, transferDto.RecipientFirstName, StringComparison.OrdinalIgnoreCase) ||
        !string.Equals(recipientUser.LastName, transferDto.RecipientLastName, StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest("Recipient name and surname do not match.");
    }

    if (transferDto.Amount <= 0)
        return Results.BadRequest("Transfer amount must be positive.");

    if (senderAccount.Balance < transferDto.Amount)
        return Results.BadRequest("Insufficient balance.");

    senderAccount.Balance -= transferDto.Amount;
    recipientAccount.Balance += transferDto.Amount;

    await db.SaveChangesAsync();

    // Mail gönderimi (recipient)
    var recipientFullName = $"{recipientUser.FirstName} {recipientUser.LastName}";
    await mailService.SendTransferNotificationEmailAsync(
        recipientUser.Email,
        recipientFullName,
        transferDto.Amount,
        transferDto.Note ?? ""
    );

    return Results.Ok(new
    {
        message = "Transfer successful",
        senderBalance = senderAccount.Balance,
        recipientBalance = recipientAccount.Balance
    });
});


// Tüm faturaları listele
app.MapGet("/api/bills", async (AppDbContext db) =>
    await db.Bills.Include(b => b.User).ToListAsync());

// Kullanıcıya özel faturaları listele 
app.MapGet("/api/users/{userId}/bills", async (int userId, AppDbContext db) =>
{
    var bills = await db.Bills
        .Where(b => b.UserId == userId)
        .Include(b => b.User)
        .ToListAsync();

    return Results.Ok(bills);
})
.WithName("GetUserBills")
.WithTags("Bills");    

// Id'ye göre tek fatura getir
app.MapGet("/api/bills/{id}", async (int id, AppDbContext db) =>
{
    var bill = await db.Bills.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
    return bill is not null ? Results.Ok(bill) : Results.NotFound();
});

// Yeni fatura oluştur
app.MapPost("/api/bills", async (Bill bill, AppDbContext db) =>
{
    db.Bills.Add(bill);
    await db.SaveChangesAsync();
    return Results.Created($"/api/bills/{bill.Id}", bill);
});

// Faturayı ödenmiş olarak güncelle
app.MapPut("/api/bills/{id}/pay", async (int id, AppDbContext db) =>
{
    var bill = await db.Bills.FindAsync(id);
    if (bill is null) return Results.NotFound("Fatura bulunamadı.");

    if (bill.IsPaid)
        return Results.BadRequest("Fatura zaten ödenmiş.");

    var account = await db.Accounts.FirstOrDefaultAsync(a => a.UserId == bill.UserId);
    if (account is null)
        return Results.NotFound("Kullanıcının hesabı bulunamadı.");

    if (account.Balance < bill.Amount)
        return Results.BadRequest("Yetersiz bakiye.");

    // 💸 Bakiye düş, fatura öde
    account.Balance -= bill.Amount;
    bill.IsPaid = true;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Fatura ödendi.",
        remainingBalance = account.Balance
    });
});

/*
// Bills tablosunu temizle - GEÇİCİ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Bills.RemoveRange(db.Bills);
    db.SaveChanges();
}*/


app.MapPost("/api/loan/apply", async ([FromBody] LoanRequestDto dto, AppDbContext db) =>
{
    var user = await db.Users.FindAsync(dto.UserId);
    if (user is null)
        return Results.NotFound("Kullanıcı bulunamadı.");

    var hasActiveLoan = await db.Loans.AnyAsync(l => l.UserId == dto.UserId && l.IsActive);
    if (hasActiveLoan)
        return Results.BadRequest("Zaten aktif bir krediniz mevcut.");

    var userAccounts = await db.Accounts.Where(a => a.UserId == dto.UserId).ToListAsync();
    var totalBalance = userAccounts.Sum(a => a.Balance);
    bool isApproved = dto.Amount <= 10000 || totalBalance >= (dto.Amount * 0.25m);

    var loan = new Loan
    {
        UserId = dto.UserId,
        Amount = dto.Amount,
        Term = dto.Term, // ➕ eklendi
        IsApproved = isApproved,
        IsActive = isApproved,
        ApplicationDate = DateTime.UtcNow
    };

    db.Loans.Add(loan);
if (isApproved)
{
    // Kullanıcının ilk hesabına krediyi yatırıyoruz
    var firstAccount = userAccounts.FirstOrDefault();
    if (firstAccount is not null)
    {
        firstAccount.Balance += dto.Amount;
    }
}    await db.SaveChangesAsync();

    var response = new LoanResponseDto(
        loan.Id,
        loan.Amount,
        loan.Term, // ➕ eklendi
        loan.ApplicationDate,
        loan.IsApproved,
        loan.IsActive
    );

    return Results.Ok(new
    {
        Message = isApproved ? "Kredi başvurusu onaylandı." : "Kredi başvurusu reddedildi.",
        Loan = response
    });
});

app.MapGet("/api/loan/{userId}", async (int userId, AppDbContext db) =>
{
    var loans = await db.Loans
        .Where(l => l.UserId == userId)
        .ToListAsync();

    return Results.Ok(loans);
});

app.MapPut("/api/loan/close/{loanId}", async (int loanId, AppDbContext db) =>
{
    var loan = await db.Loans.FindAsync(loanId);
    if (loan is null)
        return Results.NotFound("Kredi bulunamadı.");

    if (!loan.IsActive)
        return Results.BadRequest("Bu kredi zaten kapatılmış.");

    loan.IsActive = false;
    await db.SaveChangesAsync();

    return Results.Ok("Kredi başarıyla kapatıldı.");
});

app.MapGet("/api/loan/user/{userId}", async (int userId, bool? isActive, AppDbContext db) =>
{
    var loansQuery = db.Loans.AsQueryable().Where(l => l.UserId == userId);

    if (isActive.HasValue)
        loansQuery = loansQuery.Where(l => l.IsActive == isActive.Value);

    var loans = await loansQuery.OrderByDescending(l => l.ApplicationDate).ToListAsync();

    var response = loans.Select(l => new LoanResponseDto(
        l.Id,
        l.Amount,
        l.Term, // ➕ eklendi
        l.ApplicationDate,
        l.IsApproved,
        l.IsActive
    ));

    return Results.Ok(response);
});

/*
// Loans tablosunu temizle - GEÇİCİ
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Loans.RemoveRange(db.Loans);
    db.SaveChanges();
}*/


app.Run();

static bool IsValidTCKN(string tc)
{
    if (!Regex.IsMatch(tc, @"^[1-9][0-9]{10}$"))
        return false;

    var digits = tc.Select(c => c - '0').ToArray();
    var sumOdd = digits[0] + digits[2] + digits[4] + digits[6] + digits[8];
    var sumEven = digits[1] + digits[3] + digits[5] + digits[7];
    var digit10 = ((sumOdd * 7) - sumEven) % 10;
    var sumFirst10 = digits.Take(10).Sum();
    var digit11 = sumFirst10 % 10;

    return digit10 == digits[9] && digit11 == digits[10];
}

static bool IsValidEmail(string email)
{
    return Regex.IsMatch(email, @"^[^\s@]+@[^\s@]+\.[^\s@]+$");
}

static bool IsValidPhone(string phone)
{
    return Regex.IsMatch(phone, @"^05\d{9}$");
}

static bool IsValidPassword(string password)
{
    if (password.Length < 8) return false;
    if (!Regex.IsMatch(password, @"[A-Z]")) return false;
    if (!Regex.IsMatch(password, @"[a-z]")) return false;
    if (!Regex.IsMatch(password, @"\d")) return false;
    return true;
}

static string HashPassword(string password)
{
    using var sha256 = SHA256.Create();
    byte[] bytes = Encoding.UTF8.GetBytes(password);
    byte[] hash = sha256.ComputeHash(bytes);
    return Convert.ToBase64String(hash);
}

public record UserDto(string Tc, string Email, string Password, string FirstName, string LastName, string PhoneNumber);
public record LoginDto(string Tc, string Password);
public record ForgotPasswordDto(string Tc, string Email);
public record ResetPasswordDto(string Tc, string NewPassword);
public class TransferDto
{
    public int SenderUserId { get; set; }
    public string RecipientIban { get; set; } = null!;
    public string RecipientFirstName { get; set; } = null!;
    public string RecipientLastName { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

public record LoanRequestDto(int UserId, decimal Amount, int Term);
public record LoanResponseDto(
    int Id,
    decimal Amount,
    int Term,
    DateTime ApplicationDate,
    bool IsApproved,
    bool IsActive
);



