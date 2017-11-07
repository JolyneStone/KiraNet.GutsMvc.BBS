using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.UnitOfWorkModel;
using System;
using Microsoft.EntityFrameworkCore;
using KiraNet.GutsMvc.BBS.Commom;
using System.Threading.Tasks;
using KiraNet.GutsMvc.BBS.Models;
using System.Linq;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>
    {
        public UserRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<MoPageData> GetUserSearchAsync(string query, int page, int pageSize = 15)
        {
            query = query.Trim();
            page = page > 1 ? page : page;
            pageSize = pageSize > 5 ? pageSize : 15;

            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };

            var searchResults = GetAll(x => EF.Functions.Like(x.UserName, $"%{query}%"))
                .Select(x => new MoUserSearchItem
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    HeadPhoto = x.HeadPhoto,
                    Introduce = x.Introduce ?? "这个人很懒，什么也没留下..."
                });

            var total = await searchResults.CountAsync();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = await searchResults
                .OrderBy(x => x.Id)
                .TakeLast(total % pageSize)
                .ToListAsync();
                data.NextPage = 0;
            }
            else
            {
                data.PageData = await searchResults
                    .OrderBy(x => x.Id)
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToListAsync();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }


        public async Task<User> GetUser(string email, string password)
        {
            if (String.IsNullOrWhiteSpace(password) &&
                String.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var md5Pwd = CryptHelper.GetMd5Value(password.Trim());
            var user = await GetAsync(x => String.Equals(x.Email, email, StringComparison.OrdinalIgnoreCase) &&
            x.Password.Equals(md5Pwd));

            return user;
        }

        public User CreateUser(MoUserInfoSimple registerUser)
        {
            if (registerUser == null)
            {
                throw new ArgumentNullException(nameof(registerUser));
            }

            var user = CreateUserInstance(registerUser);

            Insert(user);

            return user;
        }

        public async Task<User> CreateUserAsync(MoUserInfoSimple registerUser)
        {
            if (registerUser == null)
            {
                throw new ArgumentNullException(nameof(registerUser));
            }

            var user = CreateUserInstance(registerUser);

            await InsertAsync(user);

            return user;
        }

        private User CreateUserInstance(MoUserInfoSimple registerUser)
        {
            return new User
            {
                UserName = registerUser.UserName,
                Email = registerUser.UserName,
                Password = CryptHelper.GetMd5Value(registerUser.UserPwd),
                UserStatus = UserStatus.禁用,
                HeadPhoto = "../../wwwroot/images/gutsmvc.png",
                Sex = false
            };
        }
    }
}
