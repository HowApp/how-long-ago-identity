namespace HowIdentity.Entity;

using HowCommon.Enums;

public class UserMicroservices
{
    public int UserId { get; set; }
    public MicroServicesEnum MicroService { get; set; }
    public bool ConfirmExisting { get; set; }
}