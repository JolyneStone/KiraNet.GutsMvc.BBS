namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoTopicSearchItem
    {
        public int Id { get; set; }
        public string TopicName { get; set; }
        public int ReplyCount { get; set; }
        public int StarCount { get; set; }
        public string CreateTime { get; set; }
    }
}
