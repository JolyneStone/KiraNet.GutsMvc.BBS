namespace KiraNet.GutsMvc.BBS
{
    public static class StringExtensions
    {
        public static string FomartSubStr(this string val, int startLen = 20, string op = "...")
        {
            if (string.IsNullOrWhiteSpace(val)) { return ""; }
            val = val.Trim();
            if (val.Length < startLen) { return val; }
            else
            {
                val = val.Substring(0, startLen) + op;
            }
            return val;
        }
    }
}
