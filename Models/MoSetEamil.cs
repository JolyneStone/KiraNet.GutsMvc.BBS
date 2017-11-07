using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoSetEmail
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "密码不许为空！")]
        [DataType(DataType.Password)]
        [Display(Prompt = "密码长度范围6-20字符！")]
        [RegularExpression(@"[^\s]{6,20}", ErrorMessage = "密码长度范围6-20字符。")]
        public string Password { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "账号长度范围6-30字符！")]
        [Display(Prompt = "邮箱/6-30字符")]
        [StringLength(30, MinimumLength = 6, ErrorMessage = "邮箱过长或过短！")]
        [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "邮箱格式不正确！请重新输入")]
        public string Email { get; set; }
    }
}
