namespace HowIdentity.Common.Configurations;

public class IdentityServerConfiguration
{
    public List<ClientConfiguration> Clients { get; set; } = new List<ClientConfiguration>();
    public List<ClientConfiguration> ApiResources { get; set; } = new List<ClientConfiguration>();
}

public class ClientConfiguration
{
    public string ClientId { get; set; }
    public string Secret { get; set; }
}