namespace SharedModels.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public List<Interest> Interests { get; set; }
    }
}
