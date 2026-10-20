namespace Practicals.Services.Day3
{
    public record OrderRead(int Id, string Product, decimal Amount);
    public interface IOrderStore
    {
        Task<OrderRead> Add(string product, decimal amount);
        Task<OrderRead?> Find(int id);
    }
    public class OrderService(IOrderStore store)
    {
        public Task<OrderRead> Create(string product, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(product)) 
                throw new ArgumentException("Product is required.");
            if (amount < 1m) 
                throw new ArgumentOutOfRangeException(nameof(amount));
            return store.Add(product.Trim(), amount);
        }
        public Task<OrderRead?> Get(int id) => store.Find(id);
    }

}
