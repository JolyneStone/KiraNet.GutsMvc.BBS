using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Repositories
{
    public class RoleRepository : Repository<Role>
    {
        public RoleRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public Role GetOrAdd(RoleType roleType)
        {
            var roleName = roleType.ToString();
            var role = Get(x => roleName.Equals(x.RoleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                role = new Role
                {
                    RoleName = roleName
                };

                Insert(role);
            }

            return role;
        }

        public async Task<Role> GetOrAddAsync(RoleType roleType)
        {
            var roleName = roleType.ToString();
            var role = await GetAsync(x => roleName.Equals(x.RoleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                role = new Role
                {
                    RoleName = roleName
                };

                Insert(role);
            }

            return role;
        }

        public async Task<List<MoAdminDes>> GetAllAdminsListAsync()
        {
            return await (from userRole in _dbContext.Set<UserToRole>()
                        join user in _dbContext.Set<User>() on userRole.UserId equals user.Id
                        join role in GetAll() on userRole.RoleId equals role.Id
                        where role.RoleName == "Admin"
                        select new MoAdminDes
                        {
                            Id = user.Id,
                            HeadPhoto = user.HeadPhoto,
                            Email = user.Email,
                            UserName = user.UserName,
                        }).ToListAsync();
        }
    }
}
