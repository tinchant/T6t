internal interface IAuthenticationProvider
{
    string Name { get; }
    Task<string> GetKeyByTokenAsync(string token);
}