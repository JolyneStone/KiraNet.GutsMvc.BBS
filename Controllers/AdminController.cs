using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly MapSetting _mapSetting;
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;

        public AdminController(IOptions<MapSetting> options, GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger)
        {
            _mapSetting = options.Value;
            _uf = uf;
            _logger = new GutsMvcLogger(logger, _uf);
        }

        /// <summary>
        /// 管理员（版主）主页
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoBBSData))]
        public async Task<IActionResult> Index()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            var bbs = await _uf.BBSRepository.GetAsync(x => x.UserId == userInfo.Id);
            if (bbs == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "找不到您负责的版块" } });
            }

            var topics = await _uf.TopicRepository.GetAllAsync(x => x.BbsId == bbs.Id);

            var moBBSData = new MoBBSData
            {
                BBSName = bbs.Bbsname,
                TotalCount = topics.Count(),
                IncreaseCount = topics.Count(x => x.CreateTime > DateTime.Now.Date),
                IncreaseReplyCount = topics.Join(await _uf.ReplyRepository.GetAllAsync(), t => t.Id, r => r.TopicId, (t, r) => r.CreateTime)
                .Count(data => data > DateTime.Now.Date)
            };


            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            return View(moBBSData);
        }

        /// <summary>
        /// 禁用或启用指定用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isDisable"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GoTopicDisable(int id, bool isDisable)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            var topic = await _uf.TopicRepository.GetByIdAsync(id);
            if (topic == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的帖子";
                return Json(data);
            }

            if (!userInfo.Roles.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!userInfo.Roles.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    !await _uf.BBSRepository.IsExistAsync(x => x.Id == topic.BbsId && x.UserId == userInfo.Id))
                {
                    data.IsOk = false;
                    data.Msg = "您没有屏蔽此贴的权限";
                    return Json(data);
                }
            }

            topic.TopicStatus = isDisable ? TopicStatus.Disabled : TopicStatus.Normal;
            _uf.TopicRepository.Update(topic);
            await _uf.SaveChangesAsync();
            data.IsOk = true;
            return Json(data);
        }

        /// <summary>
        /// 帖子管理页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(Int32))]
        public async Task<IActionResult> TopicManagement()
        {
            HttpContext.TryGetUserInfo(out var userInfo);

            var bbs = await _uf.BBSRepository.GetAsync(x => x.UserId == userInfo.Id);
            if (bbs == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "您没有版块管理的权限" } });
            }

            var count = await _uf.TopicRepository.CountAsync(x => x.BbsId == bbs.Id && x.TopicStatus == TopicStatus.Disabled);
            ViewData["BBSId"] = bbs.Id;
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            return View(count);
        }

        /// <summary>
        /// 获取被禁用帖子
        /// </summary>
        /// <param name="page"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTopicsDisabled(int page, int? id)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            if (id == null)
            {
                if (!userInfo.Roles.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    data.IsOk = false;
                    data.Msg = "您不是超级管理员";
                    return Json(data);
                }

                data.Data = (await _uf.TopicRepository.GetAllAsync(x => x.TopicStatus == TopicStatus.Disabled))
                    .Join(await _uf.UserRepository.GetAllAsync(), t => t.UserId, u => u.Id, (t, u) => new MoTopicDisabled
                    {
                        TopicId = t.Id,
                        TopicName = t.TopicName,
                        UserId = u.Id,
                        UserName = u.UserName,
                        CreateTime = t.CreateTime.ToStandardFormatString()
                    });
            }
            else
            {
                if (!userInfo.Roles.Equals("Admin") || !await _uf.BBSRepository.IsExistAsync(x => x.Id == id))
                {
                    data.IsOk = false;
                    data.Msg = "您没有对该版块的管理权限";
                    return Json(data);
                }

                data.Data = (await _uf.TopicRepository.GetAllAsync(x => x.BbsId == id && x.TopicStatus == TopicStatus.Disabled))
                    .Join(await _uf.UserRepository.GetAllAsync(), t => t.UserId, u => u.Id, (t, u) => new MoTopicDisabled
                    {
                        TopicId = t.Id,
                        TopicName = t.TopicName,
                        UserId = u.Id,
                        UserName = u.UserName,
                        CreateTime = t.CreateTime.ToStandardFormatString()
                    });
            }

            data.IsOk = true;
            return Json(data);
        }

        /// <summary>
        /// 置顶或取消置顶指定的帖子
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isTop"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GoTopicTop(int id, bool isTop)
        {
            var data = new MoData();
            var topic = await _uf.TopicRepository.GetByIdAsync(id);
            if (topic == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的帖子";
                return Json(data);
            }

            HttpContext.TryGetUserInfo(out var userInfo);
            if (!userInfo.Roles.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
            {
                if (!(await _uf.BBSRepository.IsExistAsync(x => x.Id == topic.BbsId && x.UserId == userInfo.Id)))
                {
                    data.IsOk = false;
                    data.Msg = "您不是该版块的版主，无法进行此操作";
                    return Json(data);
                }
            }

            if (topic.TopicStatus == TopicStatus.Disabled)
            {
                data.IsOk = false;
                data.Msg = "该帖子目前处于屏蔽状态，无法操作";
                return Json(data);
            }

            if (isTop)
            {
                if (topic.TopicStatus == TopicStatus.Top)
                {
                    data.IsOk = false;
                    data.Msg = "该帖子已被置顶，请勿反复操作";
                    return Json(data);
                }

                topic.TopicStatus = TopicStatus.Top;
            }
            else
            {
                if (topic.TopicStatus == TopicStatus.Normal)
                {
                    data.IsOk = false;
                    data.Msg = "该帖子未被置顶，请勿仿佛操作";
                    return Json(data);
                }

                topic.TopicStatus = TopicStatus.Normal;
            }

            _uf.TopicRepository.Update(topic);
            await _uf.SaveChangesAsync();

            data.IsOk = true;
            return Json(data);
        }
    }
}
