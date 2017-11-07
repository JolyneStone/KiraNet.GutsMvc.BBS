using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoUserInfo
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "邮箱格式不正确！请重新输入")]
        public string Email { get; set; }
        [RegularExpression(@"^((((\(|（){0,1}[0-9]{3,4}(\)|）|-| ){0,1}){0,1}[0-9]{7,8}((-| ){1}[0-9]+)*)|(0{0,1}13[0-9]{9})){1}$", ErrorMessage = "手机号码格式不正确！请重新输入")]
        public string HeadPhoto { get; set; }
        public string Roles { get; set; }
        public int UserStatus { get; set; }
    }
}
