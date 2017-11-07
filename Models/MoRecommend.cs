namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoRecommend
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public string TopicCreateTime { get; set; }
    }
}
