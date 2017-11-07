using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS
{
    public class TopicStarRepository : Repository<TopicStar>
    {
        public TopicStarRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<MoPageData> GetTopicStarListAsync(int userId, int page, int pageSize)
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
                    Id = x.Id,
                    Message = x.Topic.TopicName,
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
                        Message = x.Topic.TopicName,
                        CreateTime = x.CreateTime.ToStandardFormatString()
                    })
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToList();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }

        public async Task<MoPageData> GetUserStarListAsync(int topicId, int page, int pageSize)
        {
            page = page > 0 ? page : 1;
            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };
            var topics = await GetAllAsync(x => topicId == x.TopicId);
            var total = topics.Count();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = topics
                .OrderByDescending(x => x.Id)
                .Select(x => new
                {
                    Id = x.Id,
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
                        Id = x.Id,
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
    }
}
