using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoTopicDisplay
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int UserId { get; set; }
        public int CurrentPage { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int StarCount { get; set; }
        public List<MoReplayDisplay> Replays { get; set; }
    }
}
