using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Commom;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KiraNet.GutsMvc.BBS.Models;
using System;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using System.Threading.Tasks;
using KiraNet.GutsMvc.BBS.Statics;
using System.Linq;
using System.Collections.Generic;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    [SuperAdminAuthorize]
    public class SuperAdminController : Controller
    {
        private readonly MapSetting _mapSetting;
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;

        public SuperAdminController(IOptions<MapSetting> options, GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger)
        {
            _mapSetting = options.Value;
            _uf = uf;
            _logger = new GutsMvcLogger(logger, _uf);
        }

        [HttpGet]
        [ModelType(typeof(MoBackgroundData))]
        public async Task<IActionResult> Index()
        {
            HttpContext.TryGetUserInfo(out var userInfo);

            var moBackgroundData = new MoBackgroundData();

            var users = (await _uf.UserRepository.GetAllAsync())
                .Select(x => x.CreateTime);

            moBackgroundData.UserCount = users.Count();
            moBackgroundData.IncreaseUserCount = users.Where(date => date > DateTime.Now.Date).Count();

            //var topics = (await _uf.BBSRepository.GetAllAsync())
            //    .Join(await _uf.TopicRepository.GetAllAsync(), b => b.Id, t => t.Bbsid, (b, t) => new
            //    {
            //        BBSName = b.Bbsname,
            //        CreateTime = t.CreateTime
            //    });

            moBackgroundData.BBSDatas = (await _uf.BBSRepository.GetAllAsync())
                .GroupJoin(await _uf.TopicRepository.GetAllAsync(), b => b.Id, t => t.Bbsid, (b, t) => new MoBBSData
                {
                    BBSName = b.Bbsname,
                    TotalCount = t.Count(),
                    IncreaseCount = t.Count(x => x.CreateTime > DateTime.Now.Date)
                })
                .ToList();

            moBackgroundData.TopicCount = moBackgroundData.BBSDatas.Sum(x => x.TotalCount);
            moBackgroundData.IncreaseTopicCount = moBackgroundData.BBSDatas.Sum(x => x.IncreaseCount);


            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(moBackgroundData);
        }

        [HttpGet]
        [ModelType(typeof(Int32))]
        public async Task<IActionResult> Users()
        {
            var userCount = await _uf.UserRepository.CountAsync(x => x.UserStatus == UserStatus.禁用);
            //ViewData["Count"] = userCount;
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(userCount);
        }


        [HttpGet]
        [ModelType(typeof(Int32))]
        public async Task<IActionResult> TopicManagement()
        {
            HttpContext.TryGetUserInfo(out var userInfo);

            var count = await _uf.TopicRepository.CountAsync(x => x.TopicStatus == TopicStatus.Disabled);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            return View(count);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserDisable(int page)
        {
            var data = new MoData();
            page = page > 1 ? page : 1;
            data.Data = (await _uf.UserRepository.GetAllAsync(x => x.UserStatus == UserStatus.禁用))
                .OrderByDescending(x => x.Id)
                .Skip((page - 1) * 50)
                .Take(50)
                .Select(x => new { Id = x.Id, Email = x.Email, UserName = x.UserName })
                .ToList();

            data.IsOk = true;
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GoUserDisable(int id, bool isDisable)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            if (userInfo.Id == id)
            {
                data.IsOk = false;
                data.Msg = "你不能禁用你自己";
                return Json(data);
            }

            var user = await _uf.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的用户";
                return Json(data);
            }

            if (isDisable)
            {
                // 禁用
                if (user.UserStatus == UserStatus.禁用)
                {
                    data.IsOk = false;
                    data.Msg = "该用户已被禁用";
                    return Json(data);
                }

                user.UserStatus = UserStatus.禁用;
            }
            else
            {
                // 取消禁用
                if (user.UserStatus != UserStatus.禁用)
                {
                    data.IsOk = false;
                    data.Msg = "该用户未被禁用";
                    return Json(data);
                }

                user.UserStatus = UserStatus.启用;
            }

            _uf.UserRepository.Update(user);
            if (await _uf.SaveChangesAsync() > 0)
            {
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
                data.Msg = "禁用失败，请稍后再试";
            }

            return Json(data);
        }

        [HttpGet]
        [ModelType(typeof(List<MoAdminDes>))]
        public async Task<IActionResult> Admins()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            var admins = await _uf.RoleRepository.GetAllAdminsListAsync();
            return View(admins);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            using (var trans = _uf.BeginTransaction())
            {
                try
                {
                    var bbs = await _uf.BBSRepository.GetAsync(x => x.UserId == id);
                    if (bbs == null)
                    {
                        data.IsOk = false;
                        return Json(data);
                    }

                    bbs.UserId = userInfo.Id;
                    _uf.BBSRepository.Update(bbs);

                    var role = await _uf.RoleRepository.GetAsync(x => x.RoleName.Equals("User", StringComparison.OrdinalIgnoreCase));
                    var userRole = await _uf.UserToRoleRepository.GetAsync(x => x.UserId == id);
                    userRole.RoleId = role.Id;
                    _uf.UserToRoleRepository.Update(userRole);

                    if (await _uf.SaveChangesAsync() > 0)
                    {
                        data.IsOk = true;
                    }
                    else
                    {
                        data.IsOk = false;
                    }

                    trans.Commit();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(id, $"撤销版主失败-{ex.Message}:{DateTime.Now.ToStandardFormatString()}");
                    _uf.SaveChanges();
                    data.IsOk = false;
                }
            }


            return Json(data);
        }

        [HttpGet]
        [ModelType(typeof(MoAdminManagement))]
        public async Task<IActionResult> AdminManagement(int id)
        {
            var admin = await _uf.UserRepository.GetByIdAsync(id);
            if (admin == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "找不到指定的版主" } });
            }

            var bbsArray = (await _uf.BBSRepository.GetAllAsync()).Select(x => new { x.Id, x.Bbsname, x.UserId });
            if (bbsArray == null || !bbsArray.Any())
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "目前还没有子论坛版块" } });
            }

            var bbs = bbsArray.FirstOrDefault(x => x.UserId == id);
            if (bbs == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "该版主没有任何所管理的论坛" } });
            }


            HttpContext.TryGetUserInfo(out var userInfo);
            var adminManagement = new MoAdminManagement
            {
                Id = id,
                UserName = admin.UserName,
                RealName = admin.RealName ?? "",
                HeadPhoto = admin.HeadPhoto,
                Email = admin.Email,
                BBSId = bbs.Id,
                BBSName = bbs.Bbsname,
                BBSList = bbsArray.Where(x => x.UserId == userInfo.Id)
                .Select(x => new MoBBSItem
                {
                    BBSId = x.Id,
                    BBSName = x.Bbsname
                })
                .ToArray()
            };

            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            return View(adminManagement);
        }

        [HttpGet]
        [ModelType(typeof(MoBBSItem[]))]
        public async Task<IActionResult> CreateAdmin()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            var bbsList = (await _uf.BBSRepository.GetAllAsync())
                .Where(x => x.UserId == userInfo.Id)
                .Select(x => new MoBBSItem
                {
                    BBSId = x.Id,
                    BBSName = x.Bbsname
                })
                .ToArray();
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(bbsList);
        }

        [HttpPost]
        [ModelType(typeof(MoBBSItem[]))]
        public async Task<IActionResult> CreateAdmin(MoAdminInfo adminInfo)
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            var bbsList = (await _uf.BBSRepository.GetAllAsync())
                .Where(x => x.UserId == userInfo.Id)
                .Select(x => new MoBBSItem
                {
                    BBSId = x.Id,
                    BBSName = x.Bbsname
                })
                .ToArray();
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            if (!ModelState.IsValid)
            {
                this.MsgBox("请检查账号是否正确");
                return View(bbsList);
            }

            var admin = await _uf.UserRepository.GetAsync(x => x.Email.Equals(adminInfo.Email));
            if (admin == null)
            {
                this.MsgBox("找不到指定的用户");
                return View(bbsList);
            }

            var originBbs = await _uf.BBSRepository.GetAsync(x => x.UserId == admin.Id);
            if (originBbs != null)
            {
                this.MsgBox("该用户已经是版主了");
                return View(bbsList);
            }

            var bbs = await _uf.BBSRepository.GetByIdAsync(adminInfo.BBSId);
            if (bbs == null)
            {
                this.MsgBox("找不到指定的版块");
                return View(bbsList);
            }

            bbs.UserId = admin.Id;

            var role = await _uf.RoleRepository.GetAsync(x => x.RoleName == "Admin");
            var adminRole = await _uf.UserToRoleRepository.GetAsync(x => x.UserId == admin.Id);

            adminRole.RoleId = role.Id;

            _uf.BBSRepository.Update(bbs);
            _uf.UserToRoleRepository.Update(adminRole);

            if (await _uf.SaveChangesAsync() > 0)
            {
                bbsList.Where(x => x.BBSId != bbs.Id).ToArray();
                this.MsgBox("操作成功");
            }
            else
            {
                this.MsgBox("操作失败");
            }

            return View(bbsList);
        }

        [HttpGet]
        public async Task<IActionResult> ModifyAdmin(int id, int bbsId)
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            var admin = await _uf.UserRepository.GetByIdAsync(id);
            if (admin == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的用户";
                return Json(data);
            }

            var bbs = await _uf.BBSRepository.GetByIdAsync(bbsId);
            var originBbs = await _uf.BBSRepository.GetAsync(x => x.UserId == id);
            if (bbs == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的版块";
                return Json(data);
            }

            if (originBbs != null)
            {
                originBbs.UserId = userInfo.Id;
                _uf.BBSRepository.Update(originBbs);
            }

            if (bbs.UserId != userInfo.Id)
            {
                data.IsOk = false;
                data.Msg = "一个版块只能有一个版主，该版块已经有其它版主了";
                return Json(data);
            }

            bbs.UserId = id;
            _uf.BBSRepository.Update(bbs);
            await _uf.SaveChangesAsync();

            data.IsOk = true;
            return Json(data);
        }

        [HttpGet]
        public IActionResult CreateBBS()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateBBS(MoBBSInfo moBBS)
        {
            if (!HttpContext.TryGetUserInfo(out var userInfo))
            {
                this.MsgBox("登录已过期，请重新登录");
                return View();
            }

            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            if (!ModelState.IsValid)
            {
                this.MsgBox("验证论坛信息失败，请重试");
                return View();
            }

            if (String.IsNullOrWhiteSpace(moBBS.BBSName))
            {
                this.MsgBox("论坛名称不可为空！");
                return View();
            }

            var isExsit = _uf.BBSRepository.IsExist(x => x.Bbstype == moBBS.BBSType && x.Bbsname.Equals(moBBS.BBSName, StringComparison.OrdinalIgnoreCase));
            if (isExsit)
            {
                this.MsgBox("该论坛已存在，请不要重复创建");
                return View();
            }

            var bbs = new Bbs
            {
                UserId = userInfo.Id,
                Bbsname = moBBS.BBSName,
                Bbstype = moBBS.BBSType
            };
            await _uf.BBSRepository.InsertAsync(bbs);
            var result = await _uf.SaveChangesAsync();
            if (result <= 0)
            {
                this.MsgBox("创建论坛失败");
                return View();
            }

            BBSList.FreshBBSList(_uf);
            this.MsgBox($"恭喜你，创建论坛【{moBBS.BBSName}】成功");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveBBS(int id)
        {
            var data = new MoData();
            var bbs = await _uf.BBSRepository.GetByIdAsync(id);
            if (bbs == null)
            {
                data.IsOk = false;
                data.Msg = "找不到该论坛编号";
                return Json(data);
            }

            _uf.BBSRepository.Delete(bbs);
            var result = await _uf.SaveChangesAsync();
            if (result <= 0)
            {
                data.IsOk = false;
                data.Msg = "删除该论坛失败";
                return Json(data);
            }

            data.IsOk = true;
            return Json(data);
        }
    }
}
