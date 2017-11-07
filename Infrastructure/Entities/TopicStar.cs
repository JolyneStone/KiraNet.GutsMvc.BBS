using KiraNet.UnitOfWorkModel;
using System;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class TopicStar : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TopicId { get; set; }
        public DateTime CreateTime { get; set; }
        public virtual User User { get; set; }
        public virtual Topic Topic { get; set; }
    }
}
