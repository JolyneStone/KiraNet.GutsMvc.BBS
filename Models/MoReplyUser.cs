namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoReplyUser
    {
        public int ReplyUserId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public string Message { get; set; }
        public string ReplyTime { get; set; }
    }
}
