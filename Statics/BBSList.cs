using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KiraNet.GutsMvc.BBS.Statics
{
    public class BBSList
    {
        private static IList<MoBBSItem> _bbsList;

        public static IList<MoBBSItem> GetCurrentList(GutsMvcUnitOfWork uf)
        {
            if (_bbsList != null && _bbsList.Count > 0)
            {
                return _bbsList;
            }

            FreshBBSList(uf);
            return _bbsList;
        }

        public static void FreshBBSList(GutsMvcUnitOfWork uf)
        {
            if (uf == null)
            {
                throw new ArgumentNullException(nameof(uf));
            }

            _bbsList = uf.BBSRepository.GetAll().AsNoTracking()
                .Select(x => new MoBBSItem { BBSId = x.Id, BBSName = x.Bbsname, BBSType = ((int)x.Bbstype).ToString() })
                .ToList();
        }
    }
}
