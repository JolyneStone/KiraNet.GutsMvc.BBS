using System;

namespace KiraNet.GutsMvc.BBS.Commom
{
    public class RoleHelper
    {
        public static bool IsHelperRole(string roleOne, string roleTwo)
        {
            if (string.IsNullOrWhiteSpace(roleOne))
            {
                throw new ArgumentException("message", nameof(roleOne));
            }

            if (string.IsNullOrWhiteSpace(roleTwo))
            {
                throw new ArgumentException("message", nameof(roleTwo));
            }

            if(roleTwo.Equals("user", StringComparison.OrdinalIgnoreCase))
            {
                if(roleOne.Equals("admin", StringComparison.OrdinalIgnoreCase) || roleOne.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            }

            if(roleTwo.Equals("admin", StringComparison.OrdinalIgnoreCase) && roleOne.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
