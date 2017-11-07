namespace KiraNet.GutsMvc.BBS.Commom
{
    public enum ReplyObject
    {
        Topic = 0,
        User = 1
    }
    public enum IntegrateType
    {
        Reply = 2,
        CreateTopic = 20,
        TopicTop = 30
    }

    public enum TopicStatus
    {
        Normal = 1,
        Top = 2,
        Disabled = 3
    }
    public enum BBSType
    {
        DotNet = 1,
        Database = 2,
        CloudCompute = 3,
        NetWork = 4,
        Web = 5,
        iOS = 6,
        Android = 7,
        Linux = 8,
        ITLife = 9
    }

    public enum RoleType
    {
        User,
        Admin,
        SuperAdmin
    }

    public enum UserStatus
    {
        禁用 = -1,
        启用 = 0,
        未登录 = 999
    }

    public enum SearchType
    {
        Topic = 1,
        Content = 2,
        User = 3,
    }

    public enum ReplyType
    {
        Text = 0,
        Image = 1
    }

    public enum EmailTpl
    {
        /// <summary>
        /// 消息通知
        /// </summary>
        MsgBox = 1,

        /// <summary>
        /// 绑定邮箱
        /// </summary>
        SettingEmail = 2,

        /// <summary>
        /// 绑定手机
        /// </summary>
        SettingTel = 3
    }

}
