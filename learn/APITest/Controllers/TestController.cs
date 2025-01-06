namespace APITest.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class TestController : ControllerBase
{
    private string[] _summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };
    
    [HttpGet]
    [Route("/weatherforecast")]
    public ActionResult GetForecast()
    {
        var forecast =  Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    _summaries[Random.Shared.Next(_summaries.Length)]
                ))
            .ToArray();
        return new JsonResult(forecast);
    }
    
    [HttpGet]
    [Route("/claim-user-info")]
    [Authorize(Roles = "User")]
    public ActionResult GetClaimUserInfo()
    {
        var result =  User.Claims.Select(c => new { c.Type, c.Value });
        return new JsonResult(result);
    }
    
    [HttpGet]
    [Route("/claim-admin-info")]
    [Authorize(Roles = "Admin")]
    public ActionResult GetClaimAdminInfo()
    {
        var result =  User.Claims.Select(c => new { c.Type, c.Value });
        return new JsonResult(result);
    }
    
    [HttpGet]
    [Route("/claim-super-admin-info")]
    [Authorize(Roles = "SuperAdmin")]
    public ActionResult GetClaimSuperAdminInfo()
    {
        var result =  User.Claims.Select(c => new { c.Type, c.Value });
        return new JsonResult(result);
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}