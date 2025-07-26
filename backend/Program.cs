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
        if (string.IsNullOrWhiteSpace(req.Prompt))
            return Results.BadRequest("Prompt cannot be empty");

        var reply = await geminiService.GetGeminiResponse(req.Prompt);
        return Results.Ok(new { response = reply });
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error: {ex.Message}");
    }
})
.WithName("Chat")
.WithTags("AI");

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
public record GeminiRequest(string Prompt);
