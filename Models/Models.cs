namespace H2BIG.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Staff, Rider
        public string Status { get; set; } = "Active";
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Contact { get; set; }
        public string? Address { get; set; }
        public int BottleDebt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class Sale
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Type { get; set; } = string.Empty; // Walk-In, Delivery
        public DateTime DateTime { get; set; }
        public int StaffId { get; set; }
        public int? RiderId { get; set; }
    }

    public class SaleItem
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Delivery
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int RiderId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Delivered, Cancelled
    }

    public class BottleLedger
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int BottlesOut { get; set; }
        public int BottlesIn { get; set; }
        public int Balance { get; set; }
        public int TransactionId { get; set; }
        public DateTime Date { get; set; }
    }

    public class Expense
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
