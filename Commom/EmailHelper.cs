using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Commom
{
    public class EmailHelper
    {
        public static bool SendEmail(Dictionary<string, string> dicToEmail,
            string title, string content,
            string name = "GutsMvcBBS", string fromEmail = "997525106@qq.com")
        {
            var isOk = false;
            try
            {
                if (String.IsNullOrWhiteSpace(title) || String.IsNullOrWhiteSpace(content))
                {
                    return isOk;
                }

                //设置基本信息
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(name, fromEmail));
                foreach (var item in dicToEmail)
                {
                    message.To.Add(new MailboxAddress(item.Key, item.Value));
                }
                message.Subject = title;
                message.Body = new TextPart("html")
                {
                    Text = content
                };

                //链接发送
                using (var client = new SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    //采用qq邮箱服务器发送邮件
                    client.Connect("smtp.qq.com", 587, false);
                    
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    //qq邮箱，密码(安全设置短信获取后的密码)
                    client.Authenticate("kirayoshikage@qq.com", "gsbddnoexqdmbegb");

                    client.Send(message);
                    client.Disconnect(true);
                }
                isOk = true;
            }
            catch (Exception)
            {
                isOk = false;
            }

            return isOk;
        }
    }
}
