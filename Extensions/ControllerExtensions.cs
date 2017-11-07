namespace KiraNet.GutsMvc.BBS
{
    public static class ControllerExtensions
    {
        public static void MsgBox(this Controller controller, string msg, string key = "msgbox")
        {
            controller.ViewData[key] = msg;
        }

        public static string GetUserIp(this Controller controller)
        {
            // TODO; 此处需要测试
            return controller.HttpRequest.RemoteEndPoint.Address.ToString();
        }
    }
}
