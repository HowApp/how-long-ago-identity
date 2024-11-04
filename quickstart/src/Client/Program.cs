// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using IdentityModel.Client;

var client = new HttpClient();
var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
if (disco.IsError)
{
    Console.WriteLine(disco.Error);
    Console.WriteLine(disco.Exception);
    return 1;
}

Console.WriteLine("\nDiscovery Document:");
Console.WriteLine(disco.Json);

var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
{
    Address = disco.TokenEndpoint,
    ClientId = "client",
    ClientSecret = "secret",
    Scope = "api1"
});
if (tokenResponse.IsError)
{
    Console.WriteLine(tokenResponse.Error);
    Console.WriteLine(tokenResponse.Exception);
    return 1;
}

Console.WriteLine("\nAccess Token:");
Console.WriteLine(tokenResponse.AccessToken);

client.SetBearerToken(tokenResponse.AccessToken!);
var response = await client.GetAsync("https://localhost:6001/identity");
if (!response.IsSuccessStatusCode)
{
    Console.WriteLine(response.StatusCode);
    return 1;
}

var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
Console.WriteLine("\nResponse:");
Console.WriteLine(JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true }));

var response2 = await client.GetAsync("https://localhost:6001/identity/with-policy");
if (!response2.IsSuccessStatusCode)
{
    Console.WriteLine(response2.StatusCode);
    return 1;
}

var doc2 = JsonDocument.Parse(await response2.Content.ReadAsStringAsync()).RootElement;
Console.WriteLine("\nResponse:");
Console.WriteLine(JsonSerializer.Serialize(doc2, new JsonSerializerOptions { WriteIndented = true }));
return 0;