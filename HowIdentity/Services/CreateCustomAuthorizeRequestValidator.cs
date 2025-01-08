namespace HowIdentity.Services;

using Duende.IdentityServer.Validation;

public class CreateCustomAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
{
    public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
    {
        var prompt = context.Result.ValidatedRequest.Raw.Get("prompt");
        if (!string.IsNullOrWhiteSpace(prompt) && 
            prompt.Equals("create", StringComparison.OrdinalIgnoreCase))
        {
            context.Result.ValidatedRequest.PromptModes = new[] { "create" };
        }
        
        return Task.CompletedTask;
    }
}