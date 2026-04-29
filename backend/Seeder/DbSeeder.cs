using Backend.Data;
using Backend.DTOs.Products.Requests;
using Backend.Mappings;
using Microsoft.EntityFrameworkCore;

namespace Backend.Seeder;

/*
 * Seeds initial database data for local/dev.
 * Runs after migrations and only inserts when Products is empty.
 */
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        var hasProducts = await dbContext.Products.AnyAsync();
        if (hasProducts)
            return;

        dbContext.Products.AddRange(new[]
        {
            new CreateProductDto { Name = "Laptop", Description = "14\" Ultrabook", Price = 999.99m, Stock = 10 }.ToEntity(),
            new CreateProductDto { Name = "Mouse", Description = "Wireless", Price = 24.99m, Stock = 200 }.ToEntity(),
            new CreateProductDto { Name = "Keyboard", Description = "Mechanical", Price = 79.99m, Stock = 50 }.ToEntity()
        });

        await dbContext.SaveChangesAsync();
        Console.WriteLine("Database seeded with initial data.");
    }
}

