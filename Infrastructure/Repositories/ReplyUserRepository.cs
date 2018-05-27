using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using KiraNet.GutsMvc.BBS.Commom;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Repositories
{
    public class ReplyUserRepository : Repository<ReplyUser>
    {
        public ReplyUserRepository(DbContext dbContext) : base(dbContext)
        {
        }


        public async Task<List<MoReplyUser>> GetReplyUsers(int topicId, int replyUserId, int replyIndex)
        {
            return (await GetAllAsync(x =>
                x.TopicId == topicId &&
                x.ReplyUserId == replyUserId &&
                x.ReplyIndex == replyIndex))
                .OrderBy(x => x.Id)
                .Select(x => new MoReplyUser
                {
                    ReplyUserId = replyUserId,
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserPhoto = x.User.HeadPhoto,
                    Message = x.Message,
                    ReplyTime = x.CreateTime.ToStandardFormatString()
                })
                .ToList();
        }

        public async Task<MoPageData> GetUserReplyLog(int userId, int page, int pageSize)
        {
            page = page > 1 ? page : 1;
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
                    Id = x.Id,
                    Message = x.ReplyType == ReplyType.Text ? (x.Message.Length > 15 ? x.Message.Substring(15) + "..." : x.Message) : "【上传图片】",
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
                        Id = x.Id,
                        Message = x.ReplyType == ReplyType.Text ? (x.Message.Length > 15 ? x.Message.Substring(15) + "..." : x.Message) : "【上传图片】",
                        CreateTime = x.CreateTime.ToStandardFormatString()
                    })
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToList();
                data.NextPage = total > skipCount ? page + 1 : 0;
            }

            return data;
        }

        public async Task<MoPageData> GetChildReplyAsync(int topicId, int userId, int index, int page, int pageSize)
        {
            index = index >= 2 ? index : 2;
            page = page <= 0 ? 1 : page;
            pageSize = pageSize > 5 ? pageSize : 5;
            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };

            var childReplies = GetAll(x => x.ReplyIndex == index &&
                                x.TopicId == topicId)
                .Join(_dbContext.Set<User>(), c => c.ReplyUserId, u => u.Id, (c, u) => new
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.UserId == userId ? "楼主" : u.UserName,
                    HeadPhoto = u.HeadPhoto,
                    DesType = c.ReplyType,
                    Message = c.Message,
                    CreateTime = c.CreateTime.ToStandardFormatString()
                });

            var total = await childReplies.CountAsync();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = await childReplies
                .OrderBy(x => x.Id)
                .TakeLast(total % pageSize)
                .ToListAsync();
                data.NextPage = 0;
            }
            else
            {
                data.PageData = await childReplies
                    .OrderBy(x => x.Id)
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToListAsync();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }

    }
}
