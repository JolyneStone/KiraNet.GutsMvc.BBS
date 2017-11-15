using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.UnitOfWorkModel;
using System;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class User : IEntity
    {
        public User()
        {
            Bbs = new HashSet<Bbs>();
            Reply = new HashSet<Reply>();
            ReplyUserReplyUser = new HashSet<ReplyUser>();
            ReplyUserUser = new HashSet<ReplyUser>();
            Topic = new HashSet<Topic>();
            UserToRole = new HashSet<UserToRole>();
            UserChat = new HashSet<Chat>();
            TargetUserChat = new HashSet<Chat>();
        }
        public int Id { get; set; }
        public string RealName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool Sex { get; set; }
        public string Tel { get; set; }
        public string Nation { get; set; }
        public string City { get; set; }
        public DateTime CreateTime { get; set; }
        public string Introduce { get; set; }
        public string HeadPhoto { get; set; }
        public int Integrate { get; set; }
        public string Password { get; set; }
        public UserStatus UserStatus { get; set; }
        public virtual ICollection<TopicStar> TopicStar { get; set; }
        public virtual ICollection<UserStar> UserStarUser { get; set; }
        public virtual ICollection<UserStar> UserStarUserStar { get; set; }
        public virtual ICollection<Bbs> Bbs { get; set; }
        public virtual ICollection<Reply> Reply { get; set; }
        public virtual ICollection<ReplyUser> ReplyUserReplyUser { get; set; }
        public virtual ICollection<ReplyUser> ReplyUserUser { get; set; }
        public virtual ICollection<Topic> Topic { get; set; }
        public virtual ICollection<UserToRole> UserToRole { get; set; }
        public virtual ICollection<Chat> UserChat { get; set; }
        public virtual ICollection<Chat> TargetUserChat { get; set; }
    }
}
