using KiraNet.GutsMvc.BBS.Models;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;

namespace KiraNet.GutsMvc.BBS
{
    public static class HttpContextExtensions
    {
        public static string CookieKey(this HttpContext httpContext)
        {
            return "GutsMvcUser";
        }

        public static string SessionKey(this HttpContext httpContext)
        {
            return "KiraSession";
        }
        public static bool TryGetUserInfo(this HttpContext httpContext, out MoUserInfo userInfo)
        {
            var cookieValue = httpContext.Request.Cookies[httpContext.CookieKey()];
            if (cookieValue == null)
            {
                userInfo = httpContext.Session.Get<MoUserInfo>(httpContext.SessionKey());
                return userInfo != null;
            }

            var serializer = JsonSerializer.Create();
            using (var sr = new StringReader(cookieValue.Value.Replace("&&", ",")))
            {
                try
                {
                    userInfo = serializer.Deserialize<MoUserInfo>(new JsonTextReader(sr));
                }
                catch
                {
                    httpContext.Request.Cookies.Remove(httpContext.CookieKey());
                    userInfo = null;
                }
            }

            if (userInfo == null)
            {
                userInfo = httpContext.Session.Get<MoUserInfo>(httpContext.SessionKey());
            }

            if (userInfo == null)
            {
                return false;
            }

            if(httpContext.Response.Cookies.Get(httpContext.CookieKey())==null)
            {
                httpContext.Response.Cookies.Add(new Cookie(httpContext.CookieKey(), cookieValue.Value)
                {
                    Expires = DateTime.Now.AddDays(30),
                    Path = "/"
                });
            }

            return true;
        }

        public static void AddUserInfo(this HttpContext httpContext, MoUserInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new System.ArgumentNullException(nameof(userInfo));
            }

            var cookie = new Cookie(httpContext.CookieKey(), Newtonsoft.Json.JsonConvert.SerializeObject(userInfo).Replace(",", "&&"))
            {
                Expires = DateTime.Now.AddDays(30),
                Path = "/"
            };

            httpContext.Response.Cookies.Add(cookie);
            httpContext.Session.Set<MoUserInfo>(httpContext.SessionKey(), userInfo);
        }

        public static void DeleteUserInfo(this HttpContext httpContext)
        {
            httpContext.Request.Cookies.Remove(httpContext.CookieKey());
            httpContext.Response.Cookies.Remove(httpContext.CookieKey());
            httpContext.Session.Remove(httpContext.SessionKey());
        }
    }
}