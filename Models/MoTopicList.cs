using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoTopicList
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public IList<MoTopic> Topics { get; set; }
    }
}
