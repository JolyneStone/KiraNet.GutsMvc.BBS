using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Infrastructure.Repositories;
using KiraNet.UnitOfWorkModel;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    public class GutsMvcUnitOfWork : UnitOfWork<GutsMvcDbContext>
    {
        public GutsMvcUnitOfWork(GutsMvcDbContext context) : base(context)
        {
        }

        #region 仓储属性

        private UserRepository _user;
        private Repository<Bbs> _bbs;
        private RoleRepository _role;
        private Repository<UserToRole> _userToRole;
        private TopicRepository _topic;
        private TopicStarRepository _topicStar;
        private ReplyRepository _reply;
        private Repository<Log> _log;
        private ReplyUserRepository _replyUser;
        private UserStarRepository _userStar;
        private ChatRepository _chat;

        public UserStarRepository UserStarRepository => _userStar ?? (_userStar = new UserStarRepository(_dbContext));

        public UserRepository UserRepository => _user ?? (_user = new UserRepository(_dbContext));

        public Repository<Bbs> BBSRepository => _bbs ?? (_bbs = new Repository<Bbs>(_dbContext));

        public RoleRepository RoleRepository => _role ?? (_role = new RoleRepository(_dbContext));

        public Repository<UserToRole> UserToRoleRepository => _userToRole ?? (_userToRole = new Repository<UserToRole>(_dbContext));

        public TopicRepository TopicRepository => _topic ?? (_topic = new TopicRepository(_dbContext));

        public TopicStarRepository TopicStarRepository => _topicStar ?? (_topicStar = new TopicStarRepository(_dbContext));

        public ReplyRepository ReplyRepository => _reply ?? (_reply = new ReplyRepository(_dbContext));

        public Repository<Log> LogRepository => _log ?? (_log = new Repository<Log>(_dbContext));

        public ReplyUserRepository ReplyUserRepository => _replyUser ?? (_replyUser = new ReplyUserRepository(_dbContext));

        public ChatRepository ChatRepository => _chat ?? (_chat = new ChatRepository(_dbContext));

        #endregion
    }
}
