using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    public class MemberController : Controller
    {
        public readonly GutsMvcUnitOfWork _uf;
        private readonly MapSetting _mapSetting;
        private IMemoryCache _cache;
        public MemberController(GutsMvcUnitOfWork uf, IOptions<MapSetting> options, IMemoryCache cache)
        {
            _uf = uf;
            _mapSetting = options.Value;
            _cache = cache;
        }

        /// <summary>
        /// 登录页面
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            if (HttpContext.TryGetUserInfo(out var userInfo))
            {
                if (String.IsNullOrWhiteSpace(returnUrl))
                {
                    return Redirect("http://localhost:17758/home/index");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            this.MsgBox(returnUrl, "returnUrl");
            return View();
        }

        /// <summary>
        /// 提交登录信息
        /// </summary>
        /// <param name="loginUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Login(MoLoginUser loginUser)
        {
            if (ModelState.IsValid == false || loginUser == null)
            {
                this.MsgBox("验证失败，请重试！");
                return View();
            }

            User user;

            user = await _uf.UserRepository.GetUser(loginUser.UserName, loginUser.UserPwd);

            if (user == null)
            {
                this.MsgBox("账号或密码错误！");
                return View(typeof(MoLoginUser), loginUser);
            }
            else if (user.UserStatus == UserStatus.未登录)
            {
                this.MsgBox("该账号已被禁用，或许你可以尝试重新注册一个账号！");
                return View();
            }

            user.UserStatus = (int)UserStatus.启用;
            _uf.UserRepository.Update(user);

            var userToRole = _uf.UserToRoleRepository.GetAll(x => x.UserId == user.Id);
            await _uf.SaveChangesAsync();

            var userInfo = new MoUserInfo
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                HeadPhoto = user.HeadPhoto,
                UserStatus = (int)user.UserStatus,
                Roles = userToRole.Any(x => x.Role.RoleName.Equals(RoleType.SuperAdmin.ToString(),
                            StringComparison.OrdinalIgnoreCase)) ? RoleType.SuperAdmin.ToString() :
                        userToRole.Any(x => x.Role.RoleName.Equals(RoleType.Admin.ToString(), StringComparison.OrdinalIgnoreCase)) ?
                            RoleType.Admin.ToString() : RoleType.User.ToString()
            };

            HttpContext.AddUserInfo(userInfo);

            if (String.IsNullOrWhiteSpace(loginUser.ReturnUrl))
            {
                return Redirect("http://localhost:17758/home/index");
            }
            else
            {
                return Redirect(loginUser.ReturnUrl);
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult LoginOut()
        {
            HttpContext.DeleteUserInfo();
            return Redirect("http://localhost:17758/home/index");
        }

        /// <summary>
        /// 获取登录信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetLoginInfo()
        {
            var data = new MoData();

            if (!HttpContext.TryGetUserInfo(out var userInfo))
            {
                data.IsOk = false;
                data.Msg = "找不到该账号，请重新登录";
                return Json(data);
            }

            if (userInfo.UserStatus == (int)UserStatus.禁用)
            {
                data.IsOk = false;
                data.Msg = "该账号已被禁用";
            }

            data.Data = userInfo;
            data.IsOk = true;

            return Json(data);
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// 提交注册信息
        /// </summary>
        /// <param name="registerUser"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Register(MoUserInfoSimple registerUser)
        {
            if (!ModelState.IsValid || registerUser == null)
            {
                this.MsgBox("验证失败，请稍后重试");
                return View(registerUser);
            }

            var (result, userInfo) = await TryCreateUser(registerUser, RoleType.User);
            if (!result)
            {
                return View();
            }

            var timeOut = 5;
            var now = DateTime.Now.AddMinutes(timeOut).Ticks; // 设置过期时间
            var expires = now.ToString();
            var token = CryptHelper.GetMd5Value($"{expires}-{new Random().Next()}");

            // 发送密码通知的URL 
            var appUrl = $"http://{HttpRequest.UserHostName}";
            var comfirmUrl = $"{appUrl}/member/activeemail/?expire={expires}&token={token}&email={userInfo.UserName}";

            // 读取模板
            var tpl = await HtmlTplHelper.GetHtmlTpl(EmailTpl.SettingEmail, _mapSetting.EmailTplPath);
            if (String.IsNullOrWhiteSpace(tpl))
            {
                this.MsgBox("发送绑定邮件失败，请稍后重试。");
                return View();
            }

            tpl = tpl.Replace("{name}", "尊敬的用户")
                     .Replace("{content}", $"您正在使用<a href='{appUrl}'>GutsMvcBBS</a>邮件激活功能，请点击以下链接确认绑定邮箱<a href='{comfirmUrl}'>Click here</a>；注意该地址在{timeOut}分钟内有效。");

            // 发送邮件
            var isOk = EmailHelper.SendEmail(
                new Dictionary<string, string> { { "尊敬的用户", userInfo.UserName } },
                "GutsMvcBBS - 邮箱激活",
                tpl);

            this.MsgBox(isOk ? "已给您邮箱发送了账号激活邮件，请收件后点击链接地址。" : "发送绑定邮件失败，请稍后重试！");

            return View();
        }

        /// <summary>
        /// 激活邮箱，激活成功则跳转到主页，否则跳转到错误页
        /// </summary>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ActiveEmail(string expire, string token, string email)
        {
            email = email.Trim();
            if (String.IsNullOrWhiteSpace(expire) ||
                String.IsNullOrWhiteSpace(token) ||
                String.IsNullOrWhiteSpace(email))
            {
                return Redirect("http://localhost:17758/home/error/?msg=无效的请求");
            }
            else if (email.Length >= 50 || email.Length <= 4)
            {
                //return Redirect("https://www.baidu.com");
                //return Redirect("http://localhost:17758/home/error/?msg=邮箱长度不合法");
                return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "邮箱长度不合法" } });
            }
            else if (new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$").IsMatch(email) == false)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "邮箱格式不合法，请仔细甄别您的邮箱是否正确" } });
            }

            if (!long.TryParse(expire, out var longNum))
            {
                return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "无效的请求" } });
            }
            else if (longNum < DateTime.Now.Ticks)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "请求已过期，请重新请求发送激活邮箱" } });
            }

            var user = await _uf.UserRepository.GetAsync(x => x.UserName == email);
            if (user == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "不存在该邮箱指定的账号" } });
            }
            else if (user.UserStatus == (int)UserStatus.启用)
            {
                return RedirectToAction("home", "index");
            }

            var key = $"activeEmail{email}";

            user.Email = email;
            user.UserStatus = (int)UserStatus.启用;

            using (_uf)
            {
                _uf.UserRepository.Update(user);
                await _uf.SaveChangesAsync();
            }

            var userInfo = new MoUserInfo
            {
                Id = user.Id,
                UserName = user.UserName,
                UserStatus = (int)user.UserStatus,
                Email = user.Email,
                HeadPhoto = user.HeadPhoto,
                Roles = RoleType.User.ToString()
            };

            HttpContext.AddUserInfo(userInfo);
            return RedirectToAction("home", "index");
        }

        /// <summary>
        /// 忘记密码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        /// <summary>
        /// 提交邮箱信息，并发送修改密码邮件到用户邮箱
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                this.MsgBox("邮箱必填！");
                return View();
            }

            email = email.Trim().ToLower();
            if (email.Length >= 50 || email.Length <= 3)
            {
                this.MsgBox("邮箱长度不符！");
                return View();
            }
            else if (new Regex(@"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$").IsMatch(email) == false)
            {
                this.MsgBox("邮箱不合法，请重新输入");
                return View();
            }

            var user = await _uf.UserRepository.GetAsync(x => x.Email.ToLower() == email);
            if (user == null)
            {
                this.MsgBox("不存在该绑定邮箱的账号！");
                return View();
            }
            else if (user.UserStatus == UserStatus.禁用)
            {
                this.MsgBox("该绑定邮箱的账号已被禁用，可以通过发送邮件至：997525106@qq.com联系客服！");
                return View();
            }

            var timeOut = 5;
            var now = DateTime.Now.AddMinutes(timeOut); // 设置过期时间
            var expires = now.Ticks;
            var token = CryptHelper.GetMd5Value($"{expires}-{email}");

            // 发送密码通知的URL 
            var appUrl = $"http://{HttpRequest.UserHostName}";
            var comfirmUrl = $"{appUrl}/member/confirmpassword/?expire={expires}&token={token}&email={email}";

            // 读取模板
            var tpl = await HtmlTplHelper.GetHtmlTpl(EmailTpl.MsgBox, _mapSetting.EmailTplPath);
            if (String.IsNullOrWhiteSpace(tpl))
            {
                this.MsgBox("发送邮件失败，请稍后重试。");
                return View();
            }

            tpl = tpl.Replace("{name}", "尊敬的用户")
                  .Replace("{content}", $"您正在使用<a href='{appUrl}'>GutsMvcBBS</a>邮件重置密码功能，请点击以下链接确认绑定邮箱<a href='{comfirmUrl}'>{comfirmUrl}</a>；注意该地址在{timeOut}分钟内有效。");

            // 发送邮件
            var isOk = EmailHelper.SendEmail(
                new Dictionary<string, string> { { "尊敬的用户", email } },
                "GutsMvcBBS - 重置密码",
                tpl);

            this.MsgBox(isOk ? "已给您邮箱发送了重置密码邮件，请收件后点击重置密码链接地址。" : "发送绑定邮件失败，请稍后重试！");
            return View();
        }

        /// <summary>
        /// 确认信息，成功则跳转到确认密码页面，否则跳转到错误页
        /// </summary>
        /// <param name="expire"></param>
        /// <param name="token"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet]
        [ModelType(typeof(MoUserInfoSimple))]
        public async Task<IActionResult> ConfirmPassword(string expire, string token, string email)
        {
            if (String.IsNullOrWhiteSpace(expire) ||
                String.IsNullOrWhiteSpace(token) ||
                String.IsNullOrWhiteSpace(email))
            {
                return Redirect("http://localhost:17758/home/error/?msg=无效的请求");
            }

            if (!long.TryParse(expire, out long expires))
            {
                return Redirect("http://localhost:17758/home/error/?msg=无效的请求");
            }
            else if (expires < DateTime.Now.Ticks)
            {
                return Redirect("http://localhost:17758/home/error/?msg=请求已过期，请重新操作！");
            }

            var compareToken = CryptHelper.GetMd5Value($"{expire}-{email}");
            if (!token.Equals(compareToken))
            {
                return Redirect("http://localhost:17758/home/error/?msg=验证失败，请求无效！");
            }

            email = email.Trim();
            using (_uf)
            {
                var user = await _uf.UserRepository.GetAsync(x => x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
                if (user == null)
                {
                    return Redirect("http://localhost:17758/home/error/?msg=不存在绑定了该邮箱的账号！");
                }
                else if (user.UserStatus == UserStatus.禁用)
                {
                    return Redirect("http://localhost:17758/home/error/?msg=该账号已被禁用，可以通过邮箱联系管理员997525106@qq.com");
                }

                var key = $"checkConfirmPwd{email}";
                if (_cache.TryGetValue<MoUserInfo>(key, out var result) == false)
                {
                    _cache.Set<MoUserInfo>(key, new MoUserInfo { Id = user.Id, Email = email }, TimeSpan.FromMinutes(10));
                }
            }

            return View(new MoUserInfoSimple { UserName = email });
        }

        /// <summary>
        /// 提交密码修改信息
        /// </summary>
        /// <param name="registerUser"></param>
        /// <returns></returns>
        [HttpPost]
        [ModelType(typeof(MoUserInfoSimple))]
        public async Task<IActionResult> ConfirmPassword(MoUserInfoSimple registerUser)
        {
            if (ModelState.IsValid == false)
            {
                this.MsgBox("验证失败！");
                return View(registerUser);
            }

            if (string.IsNullOrWhiteSpace(registerUser.UserPwd))
            {
                this.MsgBox("密码不能为空！");
                return View(registerUser);
            }
            else if (string.IsNullOrWhiteSpace(registerUser.ComfirmPwd))
            {
                this.MsgBox("确认密码不能为空！");
                return View(registerUser);
            }
            else if (registerUser.UserPwd != registerUser.ComfirmPwd)
            {
                this.MsgBox("密码和确认密码不相同！");
                return View(registerUser);
            }

            var key = $"checkConfirmPwd{registerUser.UserName}";
            if (!_cache.TryGetValue<MoUserInfo>(key, out var checkUser))
            {
                return Redirect("http://localhost:17758/home/error/?msg=请求已过期，请重新操作！");
            }

            using (_uf)
            {
                var user = await _uf.UserRepository.GetAsync(b => b.Id == checkUser.Id && b.Email == checkUser.Email);
                if (user == null)
                {
                    _cache.Remove(key);
                    return Redirect("http://localhost:17758/home/error/?msg=重置的密码失败，请稍后重试！");
                }

                user.Password = CryptHelper.GetMd5Value(registerUser.UserPwd.Trim());
                var result = await _uf.SaveChangesAsync();

                _cache.Remove(key);
                this.MsgBox("重置密码成功！");
            }

            return View();
        }

        #region 辅助方法

        /// <summary>
        /// 尝试创建用户，并发送注册邮件到用户邮箱
        /// </summary>
        /// <param name="registerUser"></param>
        /// <param name="roleType"></param>
        /// <returns></returns>
        private async Task<ValueTuple<bool, MoUserInfo>> TryCreateUser(MoUserInfoSimple registerUser, RoleType roleType)
        {
            if (registerUser == null)
            {
                throw new ArgumentNullException(nameof(registerUser));
            }

            // 因为我们要考虑到已经入库了但邮箱还未激活的用户（也就是还未完成全部注册流程的用户）可能会重复注册，所以我这里改成这样子
            User user = _uf.UserRepository.GetFirstOrDefault(x => x.Email.Equals(registerUser.UserName, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                using (var trans = _uf.BeginTransaction())
                {
                    try
                    {
                        user = await _uf.UserRepository.CreateUserAsync(registerUser);
                        //_uf.SaveChanges();
                        var role = await _uf.RoleRepository.GetOrAddAsync(roleType);
                        //_uf.SaveChanges();
                        var userToRole = new UserToRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        };

                        await _uf.UserToRoleRepository.InsertAsync(userToRole);
                        await _uf.SaveChangesAsync();
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        this.MsgBox("注册用户失败");
                        return (false, null);
                    }
                }
            }

            var userInfo = new MoUserInfo
            {
                Id = user.Id,
                UserStatus = (int)user.UserStatus,
                Email = user.Email,
                HeadPhoto = user.HeadPhoto,
                UserName = user.UserName,
                Roles = roleType.ToString()
            };

            HttpContext.AddUserInfo(userInfo);
            this.MsgBox("注册用户成功，请查看您的邮箱，确认激活！");
            return (true, userInfo);
        }

        #endregion
    }
}
