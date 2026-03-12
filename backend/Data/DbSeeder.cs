using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        // Seed initial data if Products table is empty
        // Note: Migrations should be applied before calling this method
        // No filter: don't use LINQ query syntax here
        var hasProducts = await dbContext.Products.AnyAsync();
        if (hasProducts == false)
        {
            dbContext.Products.AddRange(new[]
            {
                new Product { Name = "Laptop", Description = "14\" Ultrabook", Price = 999.99m, Stock = 10 },
                new Product { Name = "Mouse", Description = "Wireless", Price = 24.99m, Stock = 200 },
                new Product { Name = "Keyboard", Description = "Mechanical", Price = 79.99m, Stock = 50 }
            });
            await dbContext.SaveChangesAsync();
            Console.WriteLine("Database seeded with initial data.");
        }
    }
}


