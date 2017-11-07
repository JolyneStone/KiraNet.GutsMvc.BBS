using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoLoginUser
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "账号长度范围6-30字符！")]
        [Display(Prompt = "邮箱/6-30字符")]
        [RegularExpression(@"[^\s]{6,30}", ErrorMessage = "账号长度范围6-30字符。")]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "密码长度范围6-20字符！")]
        [DataType(DataType.Password)]
        [Display(Prompt = "密码长度范围6-20字符！")]
        [RegularExpression(@"[^\s]{6,20}", ErrorMessage = "密码长度范围6-20字符。")]
        public string UserPwd { get; set; }

        /// <summary>
        /// 回跳地址
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}
