using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.UnitOfWorkModel;
using System;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class ReplyUser : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TopicId { get; set; }
        public int ReplyUserId { get; set; }
        public int ReplyIndex { get; set; }
        public ReplyType ReplyType { get; set; }
        public string Message { get; set; }
        public DateTime CreateTime { get; set; }

        public virtual  User ReplyUserNavigation { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }
    }
}
