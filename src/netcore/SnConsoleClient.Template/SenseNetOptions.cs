namespace SnConsoleClient
{
    public class SenseNetOptions
    {
        public string RepositoryUrl { get; set; }
        public AuthenticationOptions Authentication { get; set; } = new AuthenticationOptions();
    }

    public class AuthenticationOptions
    {
        public string Authority { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Scope { get; set; } = "sensenet";
    }
}
