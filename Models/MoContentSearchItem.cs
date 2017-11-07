using KiraNet.GutsMvc.BBS.Commom;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoContentSearchItem
    {
        public int Id { get; set; }
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int ReplyIndex { get; set; }
        public ReplyType ReplyType { get; set; }
        public string Content { get; set; }
        public string CreateTime { get; set; }
    }
}
