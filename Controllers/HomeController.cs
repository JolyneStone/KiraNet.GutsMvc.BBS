﻿using KiraNet.GutsMvc;
using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.GutsMvc.BBS.Statics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS
{
    public class HomeController : Controller
    {
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IMemoryCache _cache;
        private IGutsMvcLogger _logger;

        public HomeController(GutsMvcUnitOfWork uf, IMemoryCache cache, ILogger<GutsMvcBBS> logger)
        {
            _uf = uf;
            _cache = cache;
            _logger = new GutsMvcLogger(logger, uf);
        }

        /// <summary>
        /// 论坛首页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoPaging))]
        public async Task<IActionResult> Index()
        {
            var topics = await _uf.TopicRepository.GetAllAsync(x=>x.TopicStatus != TopicStatus.Disabled);
            var paging = new MoPaging
            {
                Total = topics.Count(),
                PageSize = 15
            };

            return View(paging);
        }

        /// <summary>
        /// 客服页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [UserAuthorize]
        public IActionResult Service()
        {
            return View();
        }

        /// <summary>
        /// 调用图灵机器人接口，回复用户的问题
        /// </summary>
        /// <param name="message">用户发送的消息</param>
        /// <returns></returns>
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> SendQuestion(string message)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            string url = $"http://www.tuling123.com/openapi/api?key=d62fe8c1764648d8a5b5633b816454a6&info={message}&userid={userInfo.Id}";

            string responseResult = await RequestApi(url, Encoding.UTF8);
            if(responseResult == String.Empty)
            {
                data.IsOk = false;
                return Json(data);
            }

            data.Data = JsonConvert.DeserializeObject(responseResult);
            data.IsOk = true;
            return Json(data);

            async Task<string> RequestApi(string apiUrl, Encoding encoding)
            {
                string result = String.Empty;
                try
                {
                    WebRequest request = WebRequest.Create(url);
                    request.Credentials = CredentialCache.DefaultCredentials; // 默认身份验证
                    request.Timeout = 10000;
                    request.Method = "POST";
                    WebResponse response = await request.GetResponseAsync();
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        result = await reader.ReadToEndAsync();
                    }
                }
                catch
                {
                    return String.Empty;
                }

                return result;
            }
        }

        /// <summary>
        /// 搜索页面
        /// </summary>
        /// <param name="query"></param>
        /// <param name="searchType"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(string query, int searchType)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "搜索关键词为空" } });
            }

            ViewData["Query"] = query;
            ViewData["SearchType"] = searchType;
            return View();
        }

        /// <summary>
        /// 获取搜索结果
        /// </summary>
        /// <param name="query">关键词</param>
        /// <param name="searchType">搜索类型</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetSearchResult(string query, int searchType, int page = 1)
        {
            var data = new MoData();
            // int转枚举型失败不会抛出异常，用default捕获
            switch ((SearchType)searchType)
            {
                case SearchType.Topic:
                    {
                        var topics = await _uf.TopicRepository.GetTopicSearchAsync(query, page, 15);
                        if (topics != null && topics.PageData != null)
                        {
                            data.Data = topics;
                            data.IsOk = true;
                        }
                        else
                        {
                            data.IsOk = false;
                        }

                        break;
                    }
                case SearchType.User:
                    {
                        var users = await _uf.UserRepository.GetUserSearchAsync(query, page, 15);
                        if (users != null && users.PageData != null)
                        {
                            data.Data = users;
                            data.IsOk = true;
                        }
                        else
                        {
                            data.IsOk = false;
                        }

                        break;
                    }
                case SearchType.Content:
                    {
                        try
                        {
                            var (contents, totalCount) = await JiebaLucene.Instance.Search(query, page, 15);
                            if (contents == null || totalCount == 0)
                            {
                                data.IsOk = false;
                            }
                            else
                            {
                                data.Data = new MoPageData
                                {
                                    CurrentPage = page,
                                    PreviousPage = page > 1 ? page - 1 : 0,
                                    NextPage = page < ((totalCount + 14) / 15) ? page + 1 : 0,
                                    PageTotal = totalCount,
                                    PageData = contents
                                };

                                data.IsOk = true;
                            }
                        }
                        catch(Exception ex)
                        {
                            data.IsOk = false;
                        }

                        break;
                    }
                default:
                    data.IsOk = false;
                    break;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取首页的帖子
        /// </summary>
        /// <param name="page"页码></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTopics(int page = 1)
        {
            var data = new MoData();
            var topics = await _uf.TopicRepository.GetLatelyTopicsAsync(page, 15, true, null);
            if (topics == null && topics.Topics.Any())
            {
                data.IsOk = false;
            }
            else
            {
                data.IsOk = true;
                data.Data = topics;
            }

            return Json(data);
        }

        /// <summary>
        /// 左侧导航页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult GetBBSList()
        {
            bool result;
            IList<MoBBSItem> bbsList = null;
            try
            {
                using (_uf)
                {
                    bbsList = BBSList.GetCurrentList(_uf);
                }

                result = true;
            }
            catch
            {
                result = false;
            }

            return Json(new MoData { IsOk = result, Data = bbsList });
        }

        /// <summary>
        /// 获取推荐
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetRecommends()
        {
            var data = new MoData();

            try
            {
                var key = "recommends";
                List<MoRecommend> recommends;
                recommends = _cache.Get<List<MoRecommend>>(key);

                if (recommends == null || recommends.Count == 0)
                {
                    using (_uf)
                    {
                        recommends = (await _uf.TopicRepository.GetRecommends(5));

                        if (recommends != null && recommends.Count > 0)
                        {
                            data.Data = recommends;
                            data.IsOk = true;

                            _cache.Set<List<MoRecommend>>(key, recommends, TimeSpan.FromDays(1));
                        }
                        else
                        {
                            data.IsOk = false;
                        }
                    }
                }
                else
                {
                    data.IsOk = true;
                    data.Data = recommends;
                }
            }
            catch (Exception ex)
            {
                data.IsOk = false;
                _logger.LogError(0, $"在{nameof(HomeController)}.{nameof(GetRecommends)}中，{ex.Message}");
            }

            return Json(data);
        }

        /// <summary>
        /// 说明页
        /// </summary>
        /// <returns></returns>
        public IActionResult About()
        {
            return View();
        }

        /// <summary>
        /// 联系页
        /// </summary>
        /// <returns></returns>
        public IActionResult Contact()
        {
            ViewData["Contact"] = "997525106@qq.com";

            return View();
        }

        /// <summary>
        /// 错误页
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public IActionResult Error(string msg = null)
        {
            this.MsgBox(msg ?? "访问出了问题，开发人员正从火星赶回来修复");
            return View();
        }
    }
}
