using KiraNet.UnitOfWorkModel;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class UserToRole : IEntity
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }

        public virtual Role Role { get; set; }
        public virtual User User { get; set; }
    }
}
