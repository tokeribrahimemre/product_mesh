namespace ProductMesh.Auth.Domain.Entities;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}

public static class AppPolicies
{
    public const string CanManageProducts = "CanManageProducts";
}
