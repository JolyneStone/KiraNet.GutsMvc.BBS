using KiraNet.UnitOfWorkModel;
using System;
using KiraNet.GutsMvc.BBS.Commom;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Reply : IEntity
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public DateTime CreateTime { get; set; }
        public int ReplyCount { get; set; }
        public int ReplyIndex { get; set; }
        public ReplyType ReplyType { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual User User { get; set; }
    }
}
