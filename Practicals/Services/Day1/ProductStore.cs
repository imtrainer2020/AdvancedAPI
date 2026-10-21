using Practicals.Models;
using static Practicals.Models.Day1;

namespace Practicals.Services.Day1
{
    // In-memory store for Day 1. No database needed yet (Day 2/3).
    public class ProductStore
    {
        private int _nextId = 13;
        private int _nextReviewId = 3;

        public List<Product> Products { get; } = new()
        {
            new() { Id = 1,  Name = "Laptop Pro 15",     Price = 75000, Category = "Electronics", Stock = 10 },
            new() { Id = 2,  Name = "Wireless Mouse",    Price = 1500,  Category = "Electronics", Stock = 50 },
            new() { Id = 3,  Name = "USB-C Cable",       Price = 499,   Category = "Electronics", Stock = 200 },
            new() { Id = 4,  Name = "Mechanical Keyboard",Price = 4500, Category = "Electronics", Stock = 25 },
            new() { Id = 5,  Name = "27-inch Monitor",   Price = 18500, Category = "Electronics", Stock = 8 },
            new() { Id = 6,  Name = "Notebook A5",       Price = 120,   Category = "Stationery",  Stock = 500 },
            new() { Id = 7,  Name = "Gel Pen Pack",      Price = 199,   Category = "Stationery",  Stock = 300 },
            new() { Id = 8,  Name = "Desk Organizer",    Price = 899,   Category = "Stationery",  Stock = 40 },
            new() { Id = 9,  Name = "Water Bottle 1L",   Price = 350,   Category = "Kitchen",     Stock = 150 },
            new() { Id = 10, Name = "Coffee Mug",        Price = 299,   Category = "Kitchen",     Stock = 120 },
            new() { Id = 11, Name = "Lunch Box Steel",   Price = 650,   Category = "Kitchen",     Stock = 60 },
            new() { Id = 12, Name = "Laptop Stand",      Price = 1299,  Category = "Electronics", Stock = 35 },
        };

        public List<Review> Reviews { get; } = new()
        {
            new() { Id = 1, ProductId = 1, Rating = 5, Comment = "Excellent build" },
            new() { Id = 2, ProductId = 1, Rating = 4, Comment = "Good but heavy" },
        };

        public int NextProductId() => _nextId++;
        public int NextReviewId() => _nextReviewId++;
    }
}
