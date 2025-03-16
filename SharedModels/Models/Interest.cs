namespace SharedModels.Models
{
    public class Interest
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long CurrencyId { get; set; }
        public Currency Currency { get; set; }
    }
}
