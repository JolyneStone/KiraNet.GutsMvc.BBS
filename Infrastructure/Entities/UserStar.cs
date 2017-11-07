using KiraNet.UnitOfWorkModel;
using System;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class UserStar : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StarUserId { get; set; }
        public DateTime CreateTime { get; set; }
        public virtual User User { get; set; }
        public virtual User StarUser { get; set; }
    }
}
