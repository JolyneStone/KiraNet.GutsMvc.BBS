using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.UnitOfWorkModel;
using System;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Topic : IEntity
    {
        public Topic()
        {
            Reply = new HashSet<Reply>();
            ReplyUser = new HashSet<ReplyUser>();
            TopicStar = new HashSet<TopicStar>();
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int Bbsid { get; set; }
        public string TopicName { get; set; }
        public DateTime CreateTime { get; set; }
        public int ReplyCount { get; set; }
        public int StarCount { get; set; }
        public DateTime LastReplyTime { get; set; }
        public TopicStatus TopicStatus{ get; set; }
        public virtual Bbs Bbs { get; set; }
        public virtual User User { get; set; }
        public virtual ICollection<Reply> Reply { get; set; }
        public virtual ICollection<ReplyUser> ReplyUser { get; set; }
        public virtual ICollection<TopicStar> TopicStar { get; set; }
    }
}
