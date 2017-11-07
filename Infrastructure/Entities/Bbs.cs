using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.UnitOfWorkModel;
using System;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Bbs : IEntity
    {
        public Bbs()
        {
            Topic = new HashSet<Topic>();
        }

        public int Id { get; set; }
        public string Bbsname { get; set; }
        public BBSType Bbstype { get; set; }
        public DateTime BbscreateTime { get; set; }
        public int UserId { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Topic> Topic { get; set; }
    }
}
