public class TestAuthenticationProvider : IAuthenticationProvider
{
    public string Name => "Test";

    public Task<string> GetKeyByTokenAsync(string token)
    {
        return Task.FromResult(token);
    }
}