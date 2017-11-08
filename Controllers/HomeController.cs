using KiraNet.GutsMvc;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.GutsMvc.BBS.Statics;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KiraNet.GutsMvc.BBS.Commom;
using Microsoft.Extensions.Logging;

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

        [HttpPost]
        public IActionResult Search(string query, int searchType)
        {
            if (String.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "搜索关键词为空" } });
            }
            //if(!Enum.IsDefined(typeof(SearchType), searchType))
            //{
            //    return RedirectToAction("error", new Dictionary<string, object>() { { "msg", "为指定的搜索模式" } });
            //}

            ViewData["Query"] = query;
            ViewData["SearchType"] = searchType;
            return View();
        }

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
                        var (contents, totalCount) = await JiebaLuceneHelper.Instance.Search(query, page, 15);
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

                        break;
                    }
                default:
                    data.IsOk = false;
                    break;
            }

            return Json(data);
        }

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
                        recommends = (await _uf.TopicRepository.GetRecommends(10));

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

        [HttpGet]
        public async Task<IActionResult> Topic(int id, int page = 1)
        {
            using (_uf)
            {
                MoTopicDisplay topic = await _uf.TopicRepository.GetTopicDisplays(id, page, 20);
                if (topic == null)
                {
                    return View("找不到该帖子！", "Shared", "Error");
                }

                return View(typeof(MoTopicDisplay), topic);
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetReplies(int topicId, int replyUserId, int replyIndex)
        {
            var data = new MoData();
            using (_uf)
            {
                var replies = (await _uf.ReplyUserRepository.GetReplyUsers(topicId, replyUserId, replyIndex));

                if (replies == null && replies.Any())
                {
                    data.IsOk = false;
                }
                else
                {
                    data.IsOk = true;
                    data.Data = replies;
                }

                return Json(data);
            }
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Contact"] = "997525106@qq.com";

            return View();
        }

        public IActionResult Error(string msg = null)
        {
            this.MsgBox(msg ?? "访问出了问题，开发人员正从火星赶回来修复");
            return View();
        }
    }
}
