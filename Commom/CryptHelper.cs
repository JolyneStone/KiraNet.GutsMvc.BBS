using System.Security.Cryptography;
using System.Text;

namespace KiraNet.GutsMvc.BBS.Commom
{
    public class CryptHelper
    {
        public static string GetMd5Value(string input, string key = "天佑中华")
        {
            var hash = string.Empty;

            using (MD5 md5Hash = MD5.Create())
            {
                hash = GetMd5Hash(md5Hash, input + key);
            }
            return hash;
        }

        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2")); // 转化为十六进制
            }

            return sBuilder.ToString().ToUpper();
        }
    }
}
