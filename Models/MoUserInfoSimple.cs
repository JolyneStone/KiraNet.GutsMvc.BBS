using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoUserInfoSimple
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "账号长度范围6-30字符！")]
        [Display(Prompt = "邮箱/6-30字符")]
        [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "邮箱不符合格式")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "密码长度范围6-20字符！")]
        [DataType(DataType.Password)]
        [Display(Prompt = "密码长度范围6-20字符！")]
        [RegularExpression(@"[^\s]{6,20}", ErrorMessage = "密码长度范围6-20字符。")]
        public string UserPwd { get; set; }

        [Compare("UserPwd", ErrorMessage = "密码与确认密码不相同！")]
        [DataType(DataType.Password)]
        [Display(Prompt = "必须与密码相同")]
        public string ComfirmPwd { get; set; }
    }
}
