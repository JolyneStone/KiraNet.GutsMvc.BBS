namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoPageData
    {
        public int CurrentPage { get; set; }
        /// <summary>
        /// 0代表已经为首页
        /// </summary>
        public int PreviousPage { get; set; }
        /// <summary>
        /// 0代表已经为尾页
        /// </summary>
        public int NextPage { get; set; }
        public int PageTotal { get; set; }
        public object PageData { get; set; }
    }

    public class MoPaging
    {
        public int Total { get; set; }
        public int PageSize { get; set; }
    }
}
