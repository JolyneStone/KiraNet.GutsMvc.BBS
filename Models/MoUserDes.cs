using System;
using System.Collections.Generic;
using System.Text;

namespace KiraNet.GutsMvc.BBS.Models
{
    public class MoUserDes
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string HeadPhoto { get; set; }
        public int Grade { get; set; }
        public bool Sex { get; set; }
        public string Introduce { get; set; }
        public string UserRole { get; set; }
    }
}
