using KiraNet.UnitOfWorkModel;

namespace KiraNet.GutsMvc.BBS.Infrastructure.Entities
{
    public class Log : IEntity
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public int LogLevel { get; set; }
    }
}
