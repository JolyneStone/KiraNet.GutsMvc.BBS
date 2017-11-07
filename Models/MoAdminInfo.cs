using System.ComponentModel.DataAnnotations;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoAdminInfo
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "账号长度范围6-30字符！")]
        [Display(Prompt = "邮箱/6-30字符")]
        [RegularExpression(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$", ErrorMessage = "邮箱不符合格式")]
        public string Email { get; set; }

        public int BBSId { get; set; }
    }
}
