using System;
using System.Globalization;

namespace KiraNet.GutsMvc.BBS
{
    public static class DateTimeExtensions
    {
        public static string ToStandardFormatString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy/MM/dd HH:mm:ss");
        }

        public static string ToJSString(this DateTime dateTime)
        {
            return dateTime.ToString("r");
        }

        /// <summary>
        /// 将js时间字符串转换为DateTime， js时间字符串注意时区问题
        /// </summary>
        /// <param name="jsString"></param>
        /// <returns></returns>
        public static DateTime ToCSharpDateTime(this string jsString)
        {
            return DateTime.ParseExact(jsString, "r", CultureInfo.CurrentCulture);
        }
    }
}
