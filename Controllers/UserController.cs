using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    [UserAuthorize]
    public class UserController : Controller
    {
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IMemoryCache _cache;
        private IGutsMvcLogger _logger;

        public UserController(GutsMvcUnitOfWork uf, IMemoryCache cache, ILogger<GutsMvcBBS> logger)
        {
            _uf = uf;
            _cache = cache;
            _logger = new GutsMvcLogger(logger, uf);
        }

        /// <summary>
        /// 用户中心页面
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [ModelType(typeof(MoUserDes))]
        public async Task<IActionResult> Index(int id)
        {
            var user = await _uf.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction("error", new Dictionary<string, object>() { { "msg", "找不到指定的用户" } });
            }

            var userToRole = _uf.UserToRoleRepository.GetAll(x => x.UserId == user.Id);
            var role = userToRole.Any(x => x.Role.RoleName.Equals(RoleType.SuperAdmin.ToString(),
                                    StringComparison.OrdinalIgnoreCase)) ? RoleType.SuperAdmin.ToString() :
                               userToRole.Any(x => x.Role.RoleName.Equals(RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase)) ?
                                    RoleType.Admin.ToString() : RoleType.User.ToString();

            ViewData["IsStar"] = false;
            if (HttpContext.TryGetUserInfo(out var userInfo))
            {
                if (userInfo.Id == user.Id)
                {
                    return RedirectToAction("usercenter", "index");
                }

                ViewData["Id"] = userInfo.Id;
                ViewData["Role"] = userInfo.Roles;
                //ViewData["Higher"] = RoleHelper.IsHelperRole(userInfo.Roles, role);
                var userStar = await _uf.UserStarRepository.GetAsync(x => x.StarUserId == user.Id && x.UserId == userInfo.Id);
                if (userStar != null)
                {
                    ViewData["IsStar"] = true;
                }
            }
            else
            {
                ViewData["Higher"] = false;
            }

            if (user.UserStatus == UserStatus.禁用)
            {
                ViewData["Disabled"] = true;
            }
            else
            {
                ViewData["Disabled"] = false;
            }

            var userDes = new MoUserDes
            {
                UserId = user.Id,
                Sex = user.Sex,
                Address = $"{user.Nation} {user.City}",
                Email = user.Email,
                HeadPhoto = user.HeadPhoto,
                Grade = user.Integrate,
                Introduce = user.Introduce ?? "这个人很懒，还没有留下什么...",
                UserName = user.UserName,
                UserRole = role
            };

            return View(userDes);
        }

        /// <summary>
        /// 关注或者取消关注帖子
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isLike"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GoTopicStar(int id, bool isLike)
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
            if (userInfo.Id == topic.UserId)
            {
                data.IsOk = false;
                data.Msg = "你不能点赞你自己的帖子";
                return Json(data);
            }

            if (isLike)
            {
                // 点赞
                var isTemp = await _uf.TopicStarRepository.IsExistAsync(x => x.TopicId == id && x.UserId == userInfo.Id);
                if (isTemp)
                {
                    data.IsOk = false;
                    data.Msg = "你已经赞过了";
                    return Json(data);
                }

                var topicStar = new TopicStar
                {
                    TopicId = topic.Id,
                    UserId = userInfo.Id
                };


                topic.StarCount = topic.StarCount + 1;
                await _uf.TopicStarRepository.InsertAsync(topicStar);
            }
            else
            {
                var topicStar = await _uf.TopicStarRepository.GetAsync(x => x.TopicId == id && x.UserId == userInfo.Id);
                if (topicStar != null)
                {
                    topic.StarCount = topic.StarCount - 1;
                    _uf.TopicStarRepository.Delete(topicStar);
                }
            }

            _uf.TopicRepository.Update(topic);
            await _uf.SaveChangesAsync();
            data.IsOk = true;
            data.Msg = topic.StarCount.ToString();
            return Json(data);
        }

        /// <summary>
        /// 关注或者取消关注用户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isStar"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GoUserStar(int id, bool isStar)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            if (userInfo.Id == id)
            {
                data.IsOk = false;
                data.Msg = "你不能关注你自己";
                return Json(data);
            }

            var user = await _uf.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的用户";
                return Json(data);
            }


            var userStar = await _uf.UserStarRepository.GetAsync(x => x.StarUserId == user.Id && x.UserId == userInfo.Id);
            if (isStar)
            {
                // 关注
                if (userStar != null)
                {
                    data.IsOk = false;
                    data.Msg = $"你已经关注{user.UserName}";
                    return Json(data);
                }

                var newUserStar = new UserStar
                {
                    StarUserId = user.Id,
                    UserId = userInfo.Id
                };

                await _uf.UserStarRepository.InsertAsync(newUserStar);
            }
            else
            {
                // 取消关注
                if (userStar == null)
                {
                    data.IsOk = false;
                    data.Msg = $"你还没有关注{user.UserName}";
                    return Json(data);
                }

                _uf.UserStarRepository.Delete(userStar);
            }

            await _uf.SaveChangesAsync();
            data.IsOk = true;
            return Json(data);
        }
    }
}
