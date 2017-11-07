using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using System.Linq;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoTopic
    {
        public MoTopic()
        {
        }

        //public MoTopic(Topic topic, bool isContainBBS)
        //{
        //    var reply = topic.Reply.First(x => x.ReplyIndex == 0);
        //    TopicId = topic.Id;
        //    UserName = topic.User.UserName;
        //    UserPhoto = topic.User.HeadPhoto;
        //    CreateTime = topic.CreateTime.ToStandardFormatString();
        //    TopicName = topic.TopicName;
        //    TopicDes = reply.Message.Length > 50 ? reply.Message.Substring(0, 50) + "..." : reply.Message;
        //    DesType = reply.ReplyType;
        //    StarCount = topic.StarCount;
        //    ReplyCount = topic.ReplyCount;
        //    if (isContainBBS)
        //    {
        //        BBSId = topic.Bbsid;
        //        BBSName = topic.Bbs.Bbsname;
        //    }
        //}
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public string TopicDes { get; set; }
        public ReplyType DesType { get; set; }
        public string UserName { get; set; }
        public string UserPhoto { get; set; }
        public int StarCount { get; set; }
        public int ReplyCount { get; set; }
        public string CreateTime { get; set; }
        public string BBSName { get; set; }
        public int BBSId { get; set; }
        public TopicStatus TopicStatus { get; set; }
    }
}
