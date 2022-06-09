public class TestAuthenticationProvider : IAuthenticationProvider
{
    public string Name => "string";

    public Task<string> GetKeyByTokenAsync(string token)
    {
        return Task.FromResult(token);
    }
}