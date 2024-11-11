using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebClient.Pages;

using System.Text.Json;

public class CallApiModel : PageModel
{
    public string Json = string.Empty;
    
    public async Task OnGet(IHttpClientFactory httpclientFactory)
    {
        var client = httpclientFactory.CreateClient("apiclient");
        
        var content = await client.GetStringAsync("https://localhost:6001/identity");
        
        var parse = JsonDocument.Parse(content);
        var formatter = JsonSerializer.Serialize(parse, new JsonSerializerOptions{ WriteIndented = true });
        
        Json = formatter;
    }
}

