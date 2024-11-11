using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

using System.Text.Json;

public class CallApiModel(IHttpClientFactory httpclientFactory) : PageModel
{
    public string Json = string.Empty;

    public async Task OnGet()
    {
        //var tokenInfo = await HttpContext.GetUserAccessTokenAsync();
        //var client = new HttpClient();
        //client.SetBearerToken(tokenInfo.AccessToken!);

        var client = httpclientFactory.CreateClient("apiClient");
        
        var content = await client.GetStringAsync("https://localhost:6001/identity");
        
        var parse = JsonDocument.Parse(content);
        var formatter = JsonSerializer.Serialize(parse, new JsonSerializerOptions{ WriteIndented = true });
        
        Json = formatter;
    }
}

