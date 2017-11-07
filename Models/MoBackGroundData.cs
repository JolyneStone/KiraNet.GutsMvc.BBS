using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoBackgroundData
    {
        public int IncreaseUserCount { get; set; }
        public int UserCount { get; set; }
        public int IncreaseTopicCount { get; set; }
        public int TopicCount { get; set; }
        public IList<MoBBSData> BBSDatas { get; set; }
    }
}
