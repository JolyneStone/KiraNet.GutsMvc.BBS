namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoReplayDisplay
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        /// <summary>
        /// 回复类型，0—文字，1—图片
        /// </summary>
        public int ReplayType { get; set; }
        /// <summary>
        /// 根据回复类型，该值可能是回复的文字，也可能是回复的图片
        /// </summary>
        public string Message { get; set; }
        public string ReplayTime { get; set; }
    }
}