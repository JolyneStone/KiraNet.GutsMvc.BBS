using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Linq.Expressions;
using KiraNet.GutsMvc.BBS.Models;
using System.Collections.Generic;
using KiraNet.GutsMvc.BBS.Commom;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Repositories
{
    public class TopicRepository : Repository<Topic>
    {
        public TopicRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<MoPageData> GetTopicSearchAsync(string query, int page, int pageSize)
        {
            query = query.Trim();
            page = page > 1 ? page : page;
            pageSize = pageSize > 5 ? pageSize : 15;

            var data = new MoPageData
            {
                CurrentPage = page,
                PreviousPage = page > 1 ? page - 1 : 0
            };

            var searchResults = GetAll(x => EF.Functions.Like(x.TopicName, $"%{query}%"))
                .Select(x => new MoTopicSearchItem
                {
                    Id = x.Id,
                    TopicName = x.TopicName,
                    StarCount = x.StarCount,
                    ReplyCount = x.ReplyCount,
                    CreateTime = x.CreateTime.ToStandardFormatString()
                });

            var total = await searchResults.CountAsync();
            var skipCount = (page - 1) * pageSize;
            if (total < skipCount)
            {
                data.PageData = await searchResults
                .OrderByDescending(x => x.Id)
                .TakeLast(total % pageSize)
                .ToListAsync();
                data.NextPage = 0;
            }
            else
            {
                data.PageData = await searchResults
                    .OrderByDescending(x => x.Id)
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToListAsync();
                data.NextPage = (total - skipCount) > pageSize ? page + 1 : 0;
            }

            return data;
        }

        public async Task<List<MoRecommend>> GetRecommends(int num)
        {
            return await GetAll()
                .OrderByDescending(x => x.StarCount)
                .OrderByDescending(x => x.ReplyCount)
                .OrderByDescending(x => x.Id)
                .Take(num)
                .Select(x => new MoRecommend
                {
                    TopicId = x.Id,
                    TopicName = x.TopicName,
                    TopicCreateTime = x.CreateTime.ToStandardFormatString(),
                    UserId = x.UserId,
                    UserName = x.User.UserName,
                    UserPhoto = x.User.HeadPhoto
                })
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<MoTopicDisplay> GetTopicDisplays(int topicId, int currentPage, int pageSize)
        {
            var topic = await GetByIdAsync(topicId);
            if (topic == null)
            {
                return null;
            }

            var replay = topic.Reply
                .OrderBy(x => x.Id)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize);

            return new MoTopicDisplay
            {
                TopicId = topic.Id,
                TopicName = topic.TopicName,
                StarCount = topic.StarCount,
                UserId = topic.UserId,
                TotalCount = topic.ReplyCount,
                PageSize = pageSize,
                CurrentPage = currentPage,
                Replays = replay
                    .Select(x => new MoReplayDisplay
                    {
                        UserId = x.UserId,
                        UserName = x.UserName,
                        UserPhoto = x.User.HeadPhoto,
                        ReplayType = (int)x.ReplyType,
                        Message = x.Message,
                        ReplayTime = x.CreateTime.ToStandardFormatString()
                    })
                    .ToList()
            };
        }

        public async Task<MoTopicList> GetLatelyTopicsAsync(int page, int pageSize, bool isContainBBS, int? bbsId)
        {
            page = page <= 0 ? 1 : page;
            pageSize = page <= 0 ? 0 : pageSize;
            IQueryable<Topic> topics;
            MoTopicList moTopics = new MoTopicList
            {
                Page = page,
                PageSize = pageSize
            };

            if (bbsId.HasValue)
            {
                topics = Entities.Where(x => x.BbsId == bbsId.Value).OrderByDescending(x => x.LastReplyTime);
            }
            else
            {
                topics = Entities.OrderByDescending(x => x.LastReplyTime);
            }

            if (isContainBBS)
            {
                moTopics.Topics = await (from topic in topics
                                         from bbs in _dbContext.Set<Bbs>()
                                         from reply in _dbContext.Set<Reply>()
                                         where topic.Id == reply.TopicId &&
                                               reply.ReplyIndex == 1 &&
                                               topic.BbsId == bbs.Id &&
                                               topic.TopicStatus != TopicStatus.Disabled
                                         select new MoTopic
                                         {
                                             TopicId = topic.Id,
                                             UserName = topic.User.UserName,
                                             UserPhoto = topic.User.HeadPhoto,
                                             CreateTime = topic.CreateTime.ToStandardFormatString(),
                                             TopicName = topic.TopicName,
                                             TopicDes = reply.Message,
                                             DesType = reply.ReplyType,
                                             StarCount = topic.StarCount,
                                             ReplyCount = topic.ReplyCount,
                                             BBSId = topic.BbsId,
                                             BBSName = topic.Bbs.Bbsname,
                                             TopicStatus = topic.TopicStatus
                                         })
                                  .OrderByDescending(x => x.TopicStatus)
                                  .ThenByDescending(x => x.TopicId)
                                  .Skip((page - 1) * pageSize)
                                  .Take(pageSize)
                                  .ToListAsync();
            }
            else
            {
                moTopics.Topics = await topics.Join(_dbContext.Set<Reply>(), t => t.Id, r => r.TopicId, (topic, reply) => new
                {
                    Topic = topic,
                    Reply = reply
                })
                .Where(x => x.Topic.TopicStatus != TopicStatus.Disabled
                    && x.Reply.ReplyIndex == 1)
                .Select(x => new MoTopic
                {
                    TopicId = x.Topic.Id,
                    UserName = x.Topic.User.UserName,
                    UserPhoto = x.Topic.User.HeadPhoto,
                    TopicName = x.Topic.TopicName,
                    TopicDes = x.Reply.Message,
                    DesType = x.Reply.ReplyType,
                    StarCount = x.Topic.StarCount,
                    ReplyCount = x.Topic.ReplyCount,
                    TopicStatus = x.Topic.TopicStatus,
                    CreateTime = x.Topic.CreateTime.ToStandardFormatString()
                })
                .OrderByDescending(x => x.TopicStatus)
                .ThenByDescending(x => x.TopicId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            }

            moTopics.Total = topics.Count();
            return moTopics;
        }

        public MoTopicList GetLatelyTopics(int page, int pageSize, bool isContainBBS, int? bbsId)
        {
            page = page <= 0 ? 1 : page;
            pageSize = page <= 0 ? 0 : pageSize;
            IQueryable<Topic> topics;
            MoTopicList moTopics = new MoTopicList
            {
                Page = page,
                PageSize = pageSize
            };

            if (bbsId.HasValue)
            {
                topics = Entities.Where(x => x.BbsId == bbsId.Value).OrderByDescending(x => x.LastReplyTime);

            }
            else
            {
                topics = Entities.OrderByDescending(x => x.LastReplyTime);
            }

            if (isContainBBS)
            {
                moTopics.Topics = (from topic in topics
                                   from bbs in _dbContext.Set<Bbs>()
                                   from reply in _dbContext.Set<Reply>()
                                   where topic.Id == reply.TopicId &&
                                         reply.ReplyIndex == 1 &&
                                         topic.BbsId == bbs.Id &&
                                         topic.TopicStatus != TopicStatus.Disabled
                                   //orderby topic.LastReplyTime descending
                                   select new MoTopic
                                   {
                                       TopicId = topic.Id,
                                       UserName = topic.User.UserName,
                                       UserPhoto = topic.User.HeadPhoto,
                                       CreateTime = topic.CreateTime.ToStandardFormatString(),
                                       TopicName = topic.TopicName,
                                       TopicDes = reply.ReplyType == ReplyType.Text ? (reply.Message.Length > 50 ? reply.Message.Substring(0, 50) + "..." : reply.Message) : reply.Message,
                                       DesType = reply.ReplyType,
                                       StarCount = topic.StarCount,
                                       ReplyCount = topic.ReplyCount,
                                       BBSId = topic.BbsId,
                                       BBSName = topic.Bbs.Bbsname,
                                   })
                                .OrderBy(x => x.TopicStatus)
                                .OrderByDescending(x => x.TopicId)
                                .Skip((page - 1) * pageSize)
                                .Take(pageSize)
                                .ToList();
            }
            else
            {
                moTopics.Topics = topics.Join(_dbContext.Set<Reply>(), t => t.Id, r => r.TopicId, (topic, reply) => new
                {
                    Topic = topic,
                    Reply = reply
                })
                .Where(x => x.Topic.TopicStatus != TopicStatus.Disabled
                    && x.Reply.ReplyIndex == 1)
                .Select(x => new MoTopic
                {
                    TopicId = x.Topic.Id,
                    UserName = x.Topic.User.UserName,
                    UserPhoto = x.Topic.User.HeadPhoto,
                    CreateTime = x.Topic.CreateTime.ToStandardFormatString(),
                    TopicName = x.Topic.TopicName,
                    TopicDes = x.Reply.Message,
                    DesType = x.Reply.ReplyType,
                    StarCount = x.Topic.StarCount,
                    ReplyCount = x.Topic.ReplyCount,
                    TopicStatus = x.Topic.TopicStatus
                })
                .OrderBy(x => x.TopicStatus)
                .OrderByDescending(x => x.TopicId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            }

            moTopics.Total = topics.Count();
            return moTopics;
        }

        public async Task<MoPageData> GetUserTopicLogAsync(int userId, int page, int pageSize)
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
                    Message = x.TopicName,
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
                        Message = x.TopicName,
                        CreateTime = x.CreateTime.ToStandardFormatString()
                    })
                    .Skip(skipCount)
                    .Take(pageSize)
                    .ToList();
                data.NextPage = (total - pageSize) > skipCount ? page + 1 : 0;
            }

            return data;
        }
    }
}
