public class Account
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string AuthenticationKey { get; set; }
    public string AuthenticationProvider { get; set; }
    public string Name { get; set; }
    public virtual ICollection<Role> Roles { get; private set; } = new List<Role>();

    internal void UpdateRoles(List<Role> roles)
    {
        Roles = roles;
    }
}
