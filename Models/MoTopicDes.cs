using KiraNet.GutsMvc.BBS.Commom;
using System;
using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoTopicDes
    {
        public int BBSId { get; set; }
        public string BBSName { get; set; }
        [Required]
        [MaxLength(50)]
        public string TopicName { get; set; }
        [Required]
        [MaxLength(500)]
        public string TopicDes { get; set; } = String.Empty;
        public ReplyType ReplyType { get; set; }
    }
}
