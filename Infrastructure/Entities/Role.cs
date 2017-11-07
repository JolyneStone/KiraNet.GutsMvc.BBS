using KiraNet.UnitOfWorkModel;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Role : IEntity
    {
        public Role()
        {
            UserToRole = new HashSet<UserToRole>();
        }

        public int Id { get; set; }
        public string RoleName { get; set; }

        public virtual ICollection<UserToRole> UserToRole { get; set; }
    }
}
