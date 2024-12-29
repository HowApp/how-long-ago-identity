namespace HowIdentity.Common.Constants;

public static class AppConstants
{
    public const string CorsPolicy = "HowApplicationCors";
    public struct Role
    {
        public struct User
        {
            public const int Id = 1;
            public const string Name = "User";
        }
        
        public struct Admin
        {
            public const int Id = 2;
            public const string Name = "Admin";
        }
    }

    public static class Images
    {
        public const int ThumbnailResolution = 340;
    }
}