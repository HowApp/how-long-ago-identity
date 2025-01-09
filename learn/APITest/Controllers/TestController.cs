namespace APITest.Controllers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class TestController : ControllerBase
{
    [HttpGet]
    [Route("/basic")]
    [Authorize]
    public ActionResult GetForecast()
    {
        var result = GetInfo();
        return new JsonResult(result);
    }
    
    [HttpGet]
    [Route("/claim-user-info")]
    [Authorize(Roles = "User")]
    public ActionResult GetClaimUserInfo()
    {
        var result = GetInfo();
        return new JsonResult(result);
    }
    
    [HttpGet]
    [Route("/claim-admin-info")]
    [Authorize(Roles = "Admin")]
    public ActionResult GetClaimAdminInfo()
    {
        var result = GetInfo();
        return new JsonResult(result);
    }
    
    [HttpGet]
    [Route("/claim-super-admin-info")]
    [Authorize(Roles = "SuperAdmin")]
    public ActionResult GetClaimSuperAdminInfo()
    {
        var result = GetInfo();
        return new JsonResult(result);
    }

    private dynamic GetInfo()
    {
        var claims =  User.Claims.Select(c => new { c.Type, c.Value });

        var prop = HttpContext.AuthenticateAsync().Result.Properties!.Items
            .Select(c => 
                new
                {
                    Type = c.Key, 
                    c.Value
                });

        var result = new {Claims = claims, Prop = prop};
        
        return result;
    }
}
