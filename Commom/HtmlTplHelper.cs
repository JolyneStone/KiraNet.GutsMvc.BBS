using System;
using System.IO;
using System.Threading.Tasks;
using KiraNet.GutsMvc.Infrastructure;

namespace KiraNet.GutsMvc.BBS.Commom
{
    public class HtmlTplHelper
    {
        public static async Task<string> GetHtmlTpl(EmailTpl tpl, string folderPath = @"wwwroot\tpl")
        {
            var separator = Path.DirectorySeparatorChar;
            if(folderPath== null)
            {
                folderPath = separator + "wwwroot" + separator + "tpl";
            }
            var content = string.Empty;
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return content;
            }

            folderPath = RootConfiguration.Root + separator + folderPath;
            var path = $"{folderPath}{separator}{tpl}.html";
            try
            {
                using (var stream = File.OpenRead(path))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        content = await reader.ReadToEndAsync(); // 读取HTML模板
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return content;
        }
    }
}
