using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.UnitOfWorkModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Repositories
{
    public class ChatRepository : Repository<Chat>
    {
        public ChatRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IQueryable<Chat>> GetOnLineChatAsync(int userId, int targetUserId, DateTime dateTime)
        {
            return (await GetAllAsync(x =>
                                    x.CreateTime < dateTime &&
                                   (x.UserId == userId || x.UserId == targetUserId) &&
                                   (x.TargetUserId == targetUserId || x.TargetUserId == userId) &&
                                   x.IsArrive))
                                   .OrderByDescending(x => x.Id)
                                   .Take(5);
        }

        public async Task<IQueryable<Chat>> GetOffLineChatAsync(int userId, int targetUserId, DateTime dateTime)
        {
            return (await GetAllAsync(x => x.CreateTime < dateTime &&
                                            (x.UserId == userId || x.UserId == targetUserId) &&
                                            (x.TargetUserId == targetUserId || x.TargetUserId == userId) &&
                                            !x.IsArrive))
                                            .OrderByDescending(x => x.Id);
        }
    }
}
