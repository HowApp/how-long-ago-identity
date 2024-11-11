using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

using System.Text.Json;

public class CallApiWithPolicy : PageModel
{
    public string Json = string.Empty;
    
    public async Task OnGet(IHttpClientFactory httpclientFactory)
    {
        // var accessToken = await HttpContext.GetTokenAsync("access_token");
        // var client = new HttpClient();
        // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        
        var client = httpclientFactory.CreateClient("apiclient");
        var content = await client.GetStringAsync("https://localhost:6001/identity/with-policy");
        
        var parse = JsonDocument.Parse(content);
        var formatter = JsonSerializer.Serialize(parse, new JsonSerializerOptions{ WriteIndented = true });
        
        Json = formatter;
    }
}