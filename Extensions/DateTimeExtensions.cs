using System;

namespace KiraNet.GutsMvc.BBS
{
    public static class DateTimeExtensions
    {
        public static string ToStandardFormatString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }
    }
}
