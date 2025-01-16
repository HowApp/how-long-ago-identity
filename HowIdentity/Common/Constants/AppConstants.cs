namespace HowIdentity.Common.Constants;

public static class AppConstants
{
    public const string CorsPolicy = "HowIdentityCors";
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
        
        public struct SuperAdmin
        {
            public const int Id = 3;
            public const string Name = "SuperAdmin";
        }

        public static (int Id, string Name)[] RoleList()
        {
            return new[]
            {
                (User.Id, User.Name),
                (Admin.Id, Admin.Name),
                (SuperAdmin.Id, SuperAdmin.Name),
            };
        }
    }
}