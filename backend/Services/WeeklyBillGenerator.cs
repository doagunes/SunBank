using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class WeeklyBillGenerator : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public WeeklyBillGenerator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

/*
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var users = await db.Users.ToListAsync();

        foreach (var user in users)
        {
            // 1 dakika içindeki faturalar var mı kontrol et
            var hasExistingBill = await db.Bills.AnyAsync(b =>
                b.UserId == user.Id &&
                b.Type == "Haftalık Elektrik" &&
                b.DueDate >= DateTime.Now && 
                b.DueDate <= DateTime.Now.AddMinutes(1)
            );

            if (!hasExistingBill)
            {
                var bill = new Bill
                {
                    Amount = 100,
                    DueDate = DateTime.Now.AddMinutes(1), // 1 dakika sonrası
                    Type = "Haftalık Elektrik",
                    IsPaid = false,
                    UserId = user.Id
                };

                db.Bills.Add(bill);
            }
        }

        await db.SaveChangesAsync();

        // 1 dakika bekle
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
    }
}*/
  

      protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var users = await db.Users.ToListAsync();

                foreach (var user in users)
                {
                    // 📅 Bu kullanıcı için bu haftanın faturası var mı kontrolü
                    var hasExistingBill = await db.Bills.AnyAsync(b =>
                        b.UserId == user.Id &&
                        b.Type == "Haftalık Elektrik" &&
                        b.DueDate >= DateTime.Today && 
                        b.DueDate <= DateTime.Today.AddDays(7)
                    );

                    if (!hasExistingBill)
                    {
                        var bill = new Bill
                        {
                            Amount = 100,
                            DueDate = DateTime.Today.AddDays(7),
                            Type = "Haftalık Elektrik",
                            IsPaid = false,
                            UserId = user.Id
                        };

                        db.Bills.Add(bill);
                    }
                }

                await db.SaveChangesAsync();

                // 🕒 2 dakika bekle ve tekrar kontrol et
                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
            }
        }
    }
}
