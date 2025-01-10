namespace HowIdentity.Pages.Account.Login;

public static class LoginMessage
{
    public static readonly (string Key, string Message) InvalidCredentialsErrorMessage =
        ("Credentials error", "Invalid username or password!");
    public static readonly (string Key, string Message) AccountIsLockedOutErrorMessage =
        ("Account is locked out", "Too many attempts, account temporarily blocked!");
    public static readonly (string Key, string Message) AccountIsSuspendedErrorMessage =
        ("Account is Suspended", "Account has been suspended!");
}