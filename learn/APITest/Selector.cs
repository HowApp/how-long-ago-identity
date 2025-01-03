namespace APITest;

// Provides helper functions for forwarding logic
public static class Selector
{
    // Provides a forwarding func for JWT vs reference tokens (based on existence of dot in token)
    public static Func<HttpContext, string> ForwardReferenceToken(string introspectionScheme = "introspection")
    {
        string Select(HttpContext context)
        {
            var (scheme, credential) = GetSchemeAndCredential(context);
            if (scheme.Equals("Bearer", StringComparison.OrdinalIgnoreCase) &&
                !credential.Contains("."))
            {
                return introspectionScheme;
            }

            return null;
        }

        return Select;
    }
    
    // Extracts scheme and credential from Authorization header (if present)
    public static (string, string) GetSchemeAndCredential(HttpContext context)
    {
        var header = context.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrEmpty(header))
        {
            return ("", "");
        }

        var parts = header.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            return ("", "");
        }

        return (parts[0], parts[1]);
    }
}