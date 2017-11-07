namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoAdminManagement
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string HeadPhoto { get; set; }
        public string Email { get; set; }
        public int BBSId { get; set; }
        public string BBSName { get; set; }
        public MoBBSItem[] BBSList { get; set; }
    }
}
