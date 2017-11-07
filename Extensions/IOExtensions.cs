using System.IO;

namespace KiraNet.GutsMvc.BBS
{
    public static class IOExtensions
    {
        public static void CreateDirectory(this DirectoryInfo directory)
        {
            if(directory.Exists)
            {
                return;
            }

            directory.Create();
        }

        /// <summary>
        /// 删除指定目录中的所有文件名包含了指定关键字的文件
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool DeleteAll(this DirectoryInfo directory, string key)
        {
            bool result;
            try
            {
                foreach (var file in directory.EnumerateFiles())
                {
                    if (file.Name.StartsWith(key))
                        file.Delete();
                }

                result = true;
            }
            catch
            {
                result = false;
            }
            return result;
        }
    }
}
