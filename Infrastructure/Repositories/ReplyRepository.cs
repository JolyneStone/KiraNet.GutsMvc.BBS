using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS
{
    public class ReplyRepository : Repository<Reply>
    {
        public ReplyRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<MoPageData> GetTopicReplyAsync(int topicId, int userId, int page, int pageSize)
        {
            page = page > 0 ? page : 1;
            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };
            var replys = (await GetAllAsync(x => x.TopicId == topicId));
            var total = replys.Count();
            var skipCount = (page - 1) * pageSize;
            data.PageTotal = (total + pageSize - 1) / pageSize;
            data.NextPage = total < skipCount ? 0 : (total - skipCount) > pageSize ? page + 1 : 0;
            data.PageData = await replys.Join(_dbContext.Set<User>(), r => r.UserId, u => u.Id, (r, u) => new
            {
                UserId = u.Id,
                UserName = u.Id == userId ? "楼主" : u.UserName,
                HeadPhoto = u.HeadPhoto,
                ReplyId = r.Id,
                ReplyIndex = r.ReplyIndex,
                DesType = r.ReplyType,
                Message = r.Message,
                CreateTime = r.CreateTime.ToStandardFormatString(),
                ReplyCount = r.ReplyCount
            })
            .OrderBy(x => x.ReplyId)
            .Skip(skipCount)
            .Take(pageSize)
            .ToListAsync();

            return data;
        }

        public async Task<MoPageData> GetUserReplyLogAsync(int userId, int page, int pageSize)
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
                    Id = x.TopicId,
                    Message = (x.TopicName.Length > 10 ? x.TopicName.Substring(10) + "..." : x.TopicName) + ": " + (x.ReplyType == ReplyType.Text ? x.Message.Length > 15 ? x.Message.Substring(15) + "..." : x.Message : "上传图片"),
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
                        Id = x.TopicId,
                        Message = (x.TopicName.Length > 10 ? x.TopicName.Substring(10) + "..." : x.TopicName) + ": " + (x.ReplyType == ReplyType.Text ? x.Message.Length > 15 ? x.Message.Substring(15) + "..." : x.Message : "上传图片"),
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
