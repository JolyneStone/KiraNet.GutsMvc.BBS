using KiraNet.GutsMvc.BBS.Commom;
using System;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoSubComment
    {
        public ReplyObject ReplyObject { get; set; }
        public int? ReplyUserId { get; set; } = null;
        public int TopicId { get; set; }
        public int? ReplyIndex { get; set; } = null;
        public ReplyType ReplyType { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
