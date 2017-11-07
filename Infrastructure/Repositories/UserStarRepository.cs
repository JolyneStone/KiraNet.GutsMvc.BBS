using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.UnitOfWorkModel;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using KiraNet.GutsMvc.BBS.Models;
using System.Linq;

namespace KiraNet.GutsMvc.BBS
{
    public class UserStarRepository : Repository<UserStar>
    {
        public UserStarRepository(DbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// 获取关注User列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<MoPageData> GetByUserStarListAsync(int userId, int page, int pageSize)
        {
            page = page > 0 ? page : 1;
            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };
            var topics = await GetAllAsync(x => userId == x.StarUserId);
            var total = topics.Count();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = topics
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    Id = x.UserId,
                    Message = x.User.UserName,
                    CreateTime = x.CreateTime.ToStandardFormatString()
                })
                .TakeLast(total % pageSize)
                .ToList();
                data.NextPage = 0;
            }
            else
            {
                data.PageData = topics
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        Id = x.UserId,
                        Message = x.User.UserName,
                        CreateTime = x.CreateTime.ToStandardFormatString()
                    })
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToList();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }

        /// <summary>
        /// 获取关注列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<MoPageData> GetUserStarListAsync(int userId, int page, int pageSize)
        {
            page = page > 0 ? page : 1;
            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };
            var topics = await GetAllAsync(x => userId == x.UserId);
            var total = topics.Count();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = topics
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    Id = x.StarUserId,
                    Message = x.StarUser.UserName,
                    CreateTime = x.CreateTime.ToStandardFormatString()
                })
                .TakeLast(total % pageSize)
                .ToList();
                data.NextPage = 0;
            }
            else
            {
                data.PageData = topics
                    .OrderByDescending(x => x.Id)
                    .Select(x => new
                    {
                        Id = x.StarUserId,
                        Message = x.StarUser.UserName,
                        CreateTime = x.CreateTime.ToStandardFormatString()
                    })
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToList();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }
    }
}
