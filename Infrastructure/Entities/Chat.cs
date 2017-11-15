using KiraNet.UnitOfWorkModel;
using System;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Chat : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
        public bool IsArrive { get; set; }
        public string Message { get; set; }
        public DateTime CreateTime { get; set; }
        public User User { get; set; }
        public User TargetUser { get; set; }
    }
}
