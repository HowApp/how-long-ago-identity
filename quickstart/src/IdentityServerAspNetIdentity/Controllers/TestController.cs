namespace IdentityServerAspNetIdentity.Controllers;

using Duende.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class TestController : ControllerBase
{
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    [HttpGet]
    [Route("bobr/get-simple")]
    public IActionResult Get()
    {
        return Ok("First controller");
    }
}