public class Role
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Api { get; set; }
    public string Name { get; set; }
    private ICollection<Account> Accounts { get; set; } = new List<Account>();
}
