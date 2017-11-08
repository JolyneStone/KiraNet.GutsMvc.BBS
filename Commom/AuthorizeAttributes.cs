using KiraNet.GutsMvc.Filter;

namespace KiraNet.GutsMvc.BBS.Commom
{
    public class BBSAuthorizeAttribute : AuthorizeAttribute
    {
        public BBSAuthorizeAttribute()
        {
            AuthorizeMode = AuthorizeMode.Or;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result != null)
            {
                filterContext.Result = new RedirectResult
                {
                    Url = "http://localhost:17758/home/login"
                };
            }
        }
    }

    public class UserAuthorizeAttribute : BBSAuthorizeAttribute
    {
        public UserAuthorizeAttribute() : base()
        {
            Roles = new string[] { RoleType.User.ToString(), RoleType.Admin.ToString(), RoleType.SuperAdmin.ToString() };
        }
    }

    public class AdminAuthorizeAttribute : BBSAuthorizeAttribute
    {
        public AdminAuthorizeAttribute() : base()
        {
            Roles = new string[] { RoleType.Admin.ToString(), RoleType.SuperAdmin.ToString() };
        }
    }

    public class SuperAdminAuthorizeAttribute : BBSAuthorizeAttribute
    {
        public SuperAdminAuthorizeAttribute() : base()
        {
            Roles = new string[] { RoleType.SuperAdmin.ToString() };
        }
    }
}