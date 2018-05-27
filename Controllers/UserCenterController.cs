using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.GutsMvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    [UserAuthorize]
    public class UserCenterController : Controller
    {
        private readonly MapSetting _mapSetting;
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;

        public UserCenterController(IOptions<MapSetting> options, GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger)
        {
            _mapSetting = options.Value;
            _uf = uf;
            _logger = new GutsMvcLogger(logger, _uf);
        }

        /// <summary>
        /// 用户中心主页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View();
        }

        #region User count

        /// <summary>
        /// 获取用户关注数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserStarCount(int userId)
        {
            var data = new MoData
            {
                Data = (await _uf.UserStarRepository
                .GetAllAsync(x => x.UserId == userId))
                .Count(),
                IsOk = true
            };
            return Json(data);
        }

        /// <summary>
        /// 获取关注该用户数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUserStarCount(int userId)
        {
            var data = new MoData
            {
                Data = (await _uf.UserStarRepository
                .GetAllAsync(x => x.StarUserId == userId))
                .Count(),
                IsOk = true
            };
            return Json(data);
        }

        /// <summary>
        /// 获取用户发帖数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserTopicCount(int userId)
        {
            var data = new MoData
            {
                Data = (await _uf.TopicRepository
                .GetAllAsync(x => x.UserId == userId))
                .Count(),
                IsOk = true
            };
            return Json(data);
        }

        /// <summary>
        /// 获取用户评论数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserReplyCount(int userId)
        {
            var data = new MoData
            {
                Data = (await _uf.ReplyRepository
                .GetAllAsync(x => x.UserId == userId && x.ReplyIndex != 0))
                .Count(),
                IsOk = true
            };
            return Json(data);
        }

        #endregion

        #region User logs

        /// <summary>
        /// 获取用户发帖记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserTopicLog(int id, int page, int pageSize = 5)
        {
            var data = new MoData();
            page = page <= 0 ? 1 : page;
            var pageData = await _uf.TopicRepository.GetUserTopicLogAsync(id, page, pageSize);
            if (pageData.PageData != null)
            {
                data.Data = pageData;
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取用户评论记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserReplyLog(int id, int page, int pageSize = 5)
        {
            var data = new MoData();
            page = page <= 0 ? 1 : page;
            var pageData = await _uf.ReplyRepository.GetUserReplyLogAsync(id, page, pageSize);
            if (pageData.PageData != null)
            {
                data.Data = pageData;
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取用户关注记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserStarLog(int id, int page, int pageSize = 5)
        {
            var data = new MoData();
            page = page <= 0 ? 1 : page;
            var pageData = await _uf.UserStarRepository.GetUserStarListAsync(id, page, pageSize);
            if (pageData.PageData != null)
            {
                data.Data = pageData;
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取关注该用户记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUserStarLog(int id, int page, int pageSize = 5)
        {
            var data = new MoData();
            page = page <= 0 ? 1 : page;
            var pageData = await _uf.UserStarRepository.GetByUserStarListAsync(id, page, pageSize);
            if (pageData.PageData != null)
            {
                data.Data = pageData;
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取关注帖子记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopicStarLog(int id, int page, int pageSize = 5)
        {
            var data = new MoData();
            page = page <= 0 ? 1 : page;
            var pageData = await _uf.TopicStarRepository.GetTopicStarListAsync(id, page, pageSize);
            if (pageData.PageData != null)
            {
                data.Data = pageData;
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
            }

            return Json(data);
        }

        #endregion

        #region User setting

        /// <summary>
        /// 账户设置主模块
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoUserInfoEntire))]
        public async Task<IActionResult> AccountSettings()
        {
            if (HttpContext.TryGetUserInfo(out var userInfo))
            {
                ViewData["Role"] = userInfo.Roles.ToLower();
                ViewData["HeadPhoto"] = userInfo.HeadPhoto;
                var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
                if (user != null)
                {
                    var entire = new MoUserInfoEntire
                    {
                        Id = user.Id,
                        Sex = user.Sex,
                        Tel = user.Tel,
                        RealName = user.RealName,
                        Email = user.Email,
                        UserName = user.UserName,
                        Introduce = user.Introduce
                    };

                    ViewData["Grade"] = user.Integrate;
                    return View(entire);
                }
            }

            return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "请登录" } });
        }

        /// <summary>
        /// 上传头像页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoUserInfo))]
        public IActionResult UpHeadPhoto()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(userInfo);
        }

        /// <summary>
        /// 提交头像
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelType(typeof(MoUserInfo))]
        public async Task<IActionResult> UpHeadPhoto(int id)
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Role"] = userInfo.Roles.ToLower();
            var file = HttpRequest.Form.Files.Where(x =>
                                  x.Name == "myHeadPhoto" &&
                                  x.ContentType.Contains("image"))
                            .SingleOrDefault();
            if (file == null)
            {
                this.MsgBox("请选择上传的头像图片！");
                return View(userInfo);
            }


            var maxSize = 1024 * 1024 * 4; // 图片大小不超过4M
            if (file.Length > maxSize)
            {
                this.MsgBox("头像图片不能大于4M");
                return View(userInfo);
            }

            var directory = RootConfiguration.Root + Path.DirectorySeparatorChar + _mapSetting.UpHeadPhotoPath;
            var directoryInfo = new DirectoryInfo(directory);
            directoryInfo.DeleteAll(userInfo.Email);

            var fileExtend = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            var fileNewName = $"{userInfo.Email}-{DateTime.Now.ToString("yyyyMMddHHssfff")}{fileExtend}";
            var path = Path.Combine(directory, fileNewName);

            using (var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await file.CopyToAsync(stream); // 读取上传的图片并保存
            }

            // 更新数据
            var viewPath = $"{_mapSetting.ViewHeadPhotoPath}/{fileNewName}";

            using (var trans = _uf.BeginTransaction())
            {
                try
                {
                    var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
                    if (user == null)
                    {
                        this.MsgBox("上传失败，请稍后重试！");
                        return View(userInfo);
                    }
                    user.HeadPhoto = $"../../wwwroot{viewPath}";
                    _uf.UserRepository.Update(user);
                    var result = await _uf.SaveChangesAsync();
                    if (result > 0)
                    {
                        userInfo.HeadPhoto = user.HeadPhoto;

                        HttpContext.DeleteUserInfo();
                        HttpContext.AddUserInfo(userInfo);

                        this.MsgBox("上传成功！");
                        trans.Commit();
                    }
                    else
                    {
                        this.MsgBox("上传失败，请稍后再试！");
                        trans.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    this.MsgBox("上传失败，请稍后再试！");
                    trans.Rollback();
                    _logger.LogError(userInfo.Id, "上传头像操作失败");
                    _uf.SaveChanges();
                    trans.Commit();
                }
            }

            return View(userInfo);
        }

        /// <summary>
        /// 修改用户页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoUserInfoEntire))]
        public async Task<IActionResult> ModifyUser()
        {
            MoUserInfoEntire userInfoEntire = new MoUserInfoEntire();
            HttpContext.TryGetUserInfo(out var userInfo);
            var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
            userInfoEntire.Id = user.Id;
            userInfoEntire.Email = user.Email;
            userInfoEntire.UserName = user.UserName;
            userInfoEntire.RealName = user.RealName;
            userInfoEntire.Sex = user.Sex;
            userInfoEntire.Tel = user.Tel;
            userInfoEntire.Introduce = user.Introduce;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(userInfoEntire);
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="userInfoEntire"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelType(typeof(MoUserInfoEntire))]
        public async Task<IActionResult> ModifyUser(MoUserInfoEntire userInfoEntire)
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();

            if (userInfoEntire == null || userInfoEntire.Id <= 0)
            {
                this.MsgBox("修改失败，请稍后重试");
                return View(userInfoEntire);
            }
            else if (String.IsNullOrEmpty(userInfoEntire.UserName))
            {
                this.MsgBox("昵称不能为空");
                return View(userInfoEntire);
            }

            if (!Regex.IsMatch(userInfoEntire.Email, @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                this.MsgBox("非法邮箱，请重新输入");
            }

            if (!String.IsNullOrWhiteSpace(userInfoEntire.Tel) &&
                !Regex.IsMatch(userInfoEntire.Tel, "^(13|15|18)[0-9]{9}$"))
            {
                this.MsgBox("非法手机号码，请重新输入");
            }

            try
            {
                var user = await _uf.UserRepository.GetByIdAsync(userInfoEntire.Id);
                if (user == null)
                {
                    this.MsgBox("修改失败，请稍后重试");
                    return View(userInfoEntire);
                }

                user.RealName = userInfoEntire.RealName;
                user.UserName = userInfoEntire.UserName;
                user.Tel = userInfoEntire.Tel;
                user.Sex = userInfoEntire.Sex;
                user.Introduce = userInfoEntire.Introduce;

                _uf.UserRepository.Update(user);

                var result = await _uf.SaveChangesAsync();
                userInfo.UserName = userInfoEntire.UserName;
                HttpContext.DeleteUserInfo();
                HttpContext.AddUserInfo(userInfo);

                this.MsgBox("修改成功！");
            }
            catch (Exception ex)
            {
                this.MsgBox("修改失败，请稍后重试！");
                _logger.LogError(userInfoEntire.Id, "更新用户信息操作失败");
                _uf.SaveChanges();
            }

            return View(userInfoEntire);
        }

        /// <summary>
        /// 修改密码页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoUserInfoSimple))]
        public IActionResult ModifyPwd()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(new MoUserInfoSimple { UserName = userInfo.Email });
        }

        /// <summary>
        /// 提交修改密码
        /// </summary>
        /// <param name="moUserInfoSimple"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelType(typeof(MoUserInfoSimple))]
        public async Task<IActionResult> ModifyPwd(MoUserInfoSimple moUserInfoSimple)
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["Role"] = userInfo.Roles.ToLower();
            if (!ModelState.IsValid)
            {
                this.MsgBox("密码格式错误");
                return View(moUserInfoSimple);
            }

            try
            {
                var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
                if (user == null)
                {
                    this.MsgBox("修改密码失败！请稍后重试");
                    return View(moUserInfoSimple);
                }

                var password = CryptHelper.GetMd5Value(moUserInfoSimple.UserPwd.Trim());
                if (user.Password == password)
                {
                    this.MsgBox("新密码不能与旧密码相同！");
                    return View(moUserInfoSimple);
                }

                user.Password = password;
                var result = await _uf.SaveChangesAsync();
                if (result > 0)
                {
                    this.MsgBox("修改成功");
                }
                else
                {
                    this.MsgBox("修改失败");
                }

            }
            catch (Exception ex)
            {
                this.MsgBox("修改失败");
            }

            return View(moUserInfoSimple);
        }

        /// <summary>
        /// 安全设置页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SecuritySettings()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["Email"] = userInfo.Email;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View();
        }

        /// <summary>
        /// 设置邮箱页面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult SettingEmail()
        {
            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View();
        }

        /// <summary>
        /// 提交邮箱设置信息
        /// </summary>
        /// <param name="moEmail"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelType(typeof(MoSetEmail))]
        public async Task<IActionResult> SettingEmail(MoSetEmail moEmail)
        {
            if (!HttpContext.TryGetUserInfo(out var userInfo))
            {
                this.MsgBox("登录过期，请重新登录");
                return View();
            }

            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["Role"] = userInfo.Roles.ToLower();

            if (!ModelState.IsValid)
            {
                this.MsgBox("邮箱或密码错误！");
                return View();
            }

            if (String.IsNullOrWhiteSpace(moEmail.Email))
            {
                this.MsgBox("邮箱必填！");
                return View();
            }

            if (String.IsNullOrWhiteSpace(moEmail.Password))
            {
                this.MsgBox("密码必填");
                return View();
            }

            var password = moEmail.Password.Trim();
            var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
            if (user == null)
            {
                this.MsgBox("出现错误，请稍后再试！");
                return View();
            }

            if (user.Password != CryptHelper.GetMd5Value(password))
            {
                this.MsgBox("密码错误，请重新输入！");
                return View(moEmail);
            }

            var email = moEmail.Email.Trim();
            var timeOut = 30;
            var now = DateTime.Now.AddMinutes(timeOut);
            var expires = now.Ticks;
            var token = CryptHelper.GetMd5Value($"{expires}-{email}");
            var appUrl = $"http://{HttpRequest.UserHostName}";
            var comfirmUrl = $"{appUrl}/usercenter/confirmsettingemail/?expire={expires}&token={token}&email={email}";

            // 读取模板
            var tpl = await HtmlTplHelper.GetHtmlTpl(EmailTpl.SettingEmail, _mapSetting.EmailTplPath);
            if (String.IsNullOrWhiteSpace(tpl))
            {
                this.MsgBox("发送绑定邮件失败，请稍后重试");
                return View();
            }

            tpl = tpl.Replace("{name}", userInfo.UserName)
                   .Replace("{content}", $"您正在使用GutsMvcBBS邮件绑定功能，请点击以下链接取人绑定邮箱<a href='{comfirmUrl}'>{comfirmUrl}</a><br />注意该地址有效时间为{timeOut}分钟");

            // 发送
            var isOk = EmailHelper.SendEmail(
                        new Dictionary<string, string>
                        {
                            { userInfo.UserName, email }
                        },
                        "GutsMvcBBS - 绑定邮箱",
                        tpl
                    );

            this.MsgBox(isOk ? "已给您邮箱发送了绑定确认邮件，请收件后点击确认绑定链接地址。" : "发送绑定邮件失败，请稍后重试！");
            return View();
        }

        /// <summary>
        /// 确认新邮箱
        /// </summary>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ConfirmSettingEmail(string expire, string token, string email)
        {
            if (string.IsNullOrWhiteSpace(expire) || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "无效的请求" } });
            }

            if (!long.TryParse(expire, out var expireNum))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "无效的请求" } });
            }

            if (DateTime.Now.Ticks > expireNum)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "请求已过期，请重新申请！" } });
            }

            var compareToken = CryptHelper.GetMd5Value($"{expire}-{email}");
            if (!token.Equals(compareToken))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "验证失败，请重新操作" } });
            }

            if (!HttpContext.TryGetUserInfo(out var userInfo))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "登录已过期，请重新登录" } });
            }

            // 处理
            var user = await _uf.UserRepository.GetFirstOrDefaultAsync(x => x.Id == userInfo.Id && x.UserStatus == (int)UserStatus.启用);
            if (user == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "绑定失败，请重新操作" } });
            }

            user.Email = email;
            _uf.UserRepository.Update(user);

            var result = await _uf.SaveChangesAsync();
            if (result > 0)
            {
                this.MsgBox("绑定邮箱成功！");
                userInfo.Email = email;
                //如果是登陆状态，需要更新session
                HttpContext.DeleteUserInfo();
                HttpContext.AddUserInfo(userInfo);
            }
            else
            {
                this.MsgBox("绑定失败，请稍重试！");
            }

            return RedirectToAction("UserCenter", "Index");
        }

        #endregion
    }
}
