using KiraNet.GutsMvc.BBS.Commom;
using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    public class BBSController : Controller
    {
        private readonly MapSetting _mapSetting;
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;

        public BBSController(IOptions<MapSetting> options, GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger)
        {
            _mapSetting = options.Value;
            _uf = uf;
            _logger = new GutsMvcLogger(logger, _uf);

            //JiebaLuceneHelper.Instance.InitIndex(_uf);
        }

        /// <summary>
        /// 版块主页
        /// </summary>
        /// <param name="id">版块Id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            var bbs = await _uf.BBSRepository.GetByIdAsync(id);
            if (bbs == null)
            {
                return Redirect("http://localhost:17758/home/error");
            }

            var topics = await _uf.TopicRepository.GetAllAsync(x => x.BbsId == id && x.TopicStatus != TopicStatus.Disabled);
            var paging = new MoPaging
            {
                Total = topics.Count(),
                PageSize = 15
            };

            ViewData["Title"] = bbs.Bbsname;
            ViewData["BBSId"] = bbs.Id;
            return View(typeof(MoPaging), paging);
        }

        /// <summary>
        /// 获取版块帖子
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTopics(int id, int page = 1)
        {
            var data = new MoData();
            var topics = await _uf.TopicRepository.GetLatelyTopicsAsync(page, 15, false, id);
            if (topics == null && !topics.Topics.Any())
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
        /// 发帖
        /// </summary>
        /// <param name="bbsId"></param>
        /// <returns></returns>
        [HttpGet]
        [UserAuthorize]
        public IActionResult CreateTopic(int bbsId)
        {
            var bbs = _uf.BBSRepository.GetById(bbsId);
            if (bbs == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "你迷路了吗？" } });
            }

            ViewData["BBSId"] = bbsId;
            ViewData["BBSName"] = bbs.Bbsname;
            return View();
        }

        /// <summary>
        /// 发帖提交
        /// </summary>
        /// <param name="topicDes"></param>
        /// <returns></returns>
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> CreateTopic(MoTopicDes topicDes)
        {
            if (!ModelState.IsValid)
            {
                return ReturnViewMsg("发帖失败，请检查您的标题和内容是否为空");
            }

            HttpContext.TryGetUserInfo(out var userInfo);
            var topic = new Topic(userInfo.Id, topicDes.BBSId, topicDes.TopicName);
            try
            {
                await _uf.TopicRepository.InsertAsync(topic);
                if (await _uf.SaveChangesAsync() == 0)
                {
                    return ReturnViewMsg("发表新帖失败");
                }
                if (topicDes.ReplyType == ReplyType.Image)
                {
                    var (success, resultMsg) = SaveImg(topic.Id,
                        HttpRequest.Form.Files.Where(x => x.ContentType.Contains("image")).SingleOrDefault());
                    if (!success)
                    {
                        return ReturnViewMsg(resultMsg);
                    }

                    topicDes.TopicDes = resultMsg;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(userInfo.Id, $"发表新帖失败-{ex.Message.Substring(0, 50)}-{DateTime.Now.ToStandardFormatString()}");
                return ReturnViewMsg("发表新帖失败");
            }

            var reply = new Reply
            {
                UserId = userInfo.Id,
                TopicId = topic.Id,
                UserName = userInfo.UserName,
                TopicName = topic.TopicName,
                ReplyType = topicDes.ReplyType,
                Message = topicDes.TopicDes,
                ReplyIndex = 1
            };

            _uf.ReplyRepository.Insert(reply);
            if (!UpdateIntegrate(userInfo.Id, (int)IntegrateType.CreateTopic) &&
                _uf.SaveChanges() == 0)
            {
                _uf.TopicRepository.Delete(topic);
                _uf.SaveChanges();
                return ReturnViewMsg("发表新帖失败");
            }

            AddIndex(topic, reply);
            ViewData["TopicId"] = topic.Id;
            return ReturnViewMsg("发表新帖成功");
        }

        /// <summary>
        /// 帖子内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Topic(int id)
        {
            var topic = await _uf.TopicRepository.GetAsync(x => x.Id == id && x.TopicStatus != TopicStatus.Disabled);
            if (topic == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "帖子丢失啦~~~~" } });
            }

            var bbs = await _uf.BBSRepository.GetByIdAsync(topic.BbsId);
            if (bbs == null)
            {
                return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "帖子丢失啦~~~~" } });
            }

            var replys = await _uf.ReplyRepository.GetAllAsync(x => x.TopicId == id);
            var paging = new MoPaging
            {
                Total = replys.Count(),
                PageSize = 15
            };

            if (HttpContext.TryGetUserInfo(out var userInfo))
            {

                var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
                if (user == null)
                {
                    return RedirectToAction("home", "error", new Dictionary<string, object>() { { "msg", "请重新登录啦~~~~" } });
                }

                var isLike = await _uf.TopicStarRepository.IsExistAsync(x => x.TopicId == topic.Id && x.UserId == user.Id);
                ViewData["IsLike"] = isLike;

                if (user.Id == topic.UserId)
                {
                    ViewData["Role"] = "User";
                }

                if ((userInfo.Roles.Equals("admin", StringComparison.OrdinalIgnoreCase) && bbs.UserId == userInfo.Id) ||
                    userInfo.Roles.Equals("superadmin", StringComparison.OrdinalIgnoreCase))
                {
                    ViewData["Role"] = "Admin";
                }
            }
            else
            {
                ViewData["IsLike"] = false;
                ViewData["Role"] = String.Empty;
            }

            ViewData["TopicStatus"] = topic.TopicStatus.ToString();
            ViewData["Title"] = topic.TopicName;
            ViewData["TopicId"] = topic.Id;
            ViewData["BBSId"] = topic.BbsId;
            ViewData["BBSName"] = bbs.Bbsname;
            ViewData["StarCount"] = topic.StarCount;

            return View(typeof(MoPaging), paging);
        }

        /// <summary>
        /// 获取帖子内容
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetTopicContent(int id, int page)
        {
            var data = new MoData();
            var topic = await _uf.TopicRepository.GetAsync(x => x.Id == id && x.TopicStatus != TopicStatus.Disabled);
            if (topic == null)
            {
                data.IsOk = false;
                data.Msg = "帖子丢失啦~~~";
                return Json(data);
            }

            var replys = await _uf.ReplyRepository.GetTopicReplyAsync(id, topic.UserId, page, 20);
            if (replys == null && replys.PageData != null)
            {
                data.IsOk = false;
            }
            else
            {
                data.IsOk = true;
                data.Data = replys;
            }

            return Json(data);
        }

        /// <summary>
        /// 获取帖子评论子回复
        /// </summary>
        /// <param name="id"></param>
        /// <param name="index"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetChildReply(int id, int index, int page, int pageSize)
        {
            var data = new MoData();
            var topic = await _uf.TopicRepository.GetByIdAsync(id);
            MoPageData pageData = await _uf.ReplyUserRepository.GetChildReplyAsync(id, topic.UserId, index, page, pageSize);
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
        /// 提交评论
        /// </summary>
        /// <param name="subComment"></param>
        /// <returns></returns>
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> SubmitReply(MoSubComment subComment)
        {
            if (subComment == null &&
                (subComment.ReplyType == ReplyType.Text &&
                String.IsNullOrWhiteSpace(subComment.Message)))
            {
                return ReturnJsonMsg(false, "评论内容不能为空");
            }

            HttpContext.TryGetUserInfo(out var userInfo);
            Topic topic = await _uf.TopicRepository.GetAsync(x => x.Id == subComment.TopicId &&
                                    x.TopicStatus != TopicStatus.Disabled);
            if (topic == null)
            {
                return ReturnJsonMsg(false, "找不到帖子，该帖子已删除或被禁用");
            }

            string message = String.Empty;
            if (subComment.ReplyType == ReplyType.Image)
            {
                var file = HttpRequest.Form.Files.Where(x => x.ContentType.Contains("image")).SingleOrDefault();
                var (success, resultMsg) = SaveImg(topic.Id, file);
                if (!success)
                {
                    ReturnJsonMsg(false, resultMsg);
                }

                message = resultMsg;
            }
            else
            {
                message = subComment.Message;
            }

            var user = await _uf.UserRepository.GetByIdAsync(userInfo.Id);
            if (user == null)
            {
                return ReturnJsonMsg(false, "找不到您的用户信息");
            }

            using (var trans = _uf.BeginTransaction())
            {
                try
                {
                    if (subComment.ReplyObject == ReplyObject.User)
                    {
                        // 楼中楼
                        if (!(await AddChildReply()))
                        {
                            return ReturnJsonMsg(false, "提交评论失败，请稍后再试");
                        }
                    }
                    else
                    {
                        // 评论
                        if (!(await AddReply()))
                        {
                            return ReturnJsonMsg(false, "提交评论失败，请稍后再试");
                        }
                    }

                    await _uf.SaveChangesAsync();
                    trans.Commit();
                    return Json(new MoData { IsOk = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(userInfo.Id, $"提交评论：{ex.Message}-{DateTime.Now.ToStandardFormatString()}");
                    trans.Rollback();
                    return ReturnJsonMsg(false, "系统出错啦~~~，请稍后重试");
                }
            }

            /// 提交评论
            async Task<bool> AddReply()
            {
                var reply = new Reply
                {
                    Message = message,
                    TopicId = subComment.TopicId,
                    UserId = userInfo.Id,
                    ReplyType = subComment.ReplyType,
                    TopicName = topic.TopicName,
                    UserName = user.UserName
                };

                UpdateIntegrate(user, (int)IntegrateType.Reply);
                var replyCount = topic.ReplyCount + 1;
                topic.ReplyCount = replyCount;
                reply.ReplyIndex = replyCount + 1; // 除去楼主
                _uf.TopicRepository.Update(topic);
                await _uf.ReplyRepository.InsertAsync(reply);
                AddIndex(topic, reply);
                return true;
            }

            /// 提交子评论
            async Task<bool> AddChildReply()
            {
                if (subComment.ReplyIndex == null ||
                               subComment.ReplyUserId == null)
                {
                    return false;
                }

                var replyUser = new ReplyUser
                {
                    Message = message,
                    ReplyIndex = subComment.ReplyIndex.Value,
                    ReplyUserId = subComment.ReplyUserId.Value,
                    TopicId = subComment.TopicId,
                    UserId = userInfo.Id,
                    ReplyType = subComment.ReplyType
                };


                var reply = await _uf.ReplyRepository.GetAsync(x => x.ReplyIndex == subComment.ReplyIndex &&
                                                                        x.UserId == subComment.ReplyUserId &&
                                                                        x.TopicId == subComment.TopicId);
                if (reply == null)
                {
                    return false;
                }

                await _uf.ReplyUserRepository.InsertAsync(replyUser);
                var replyCount = reply.ReplyCount + 1;
                reply.ReplyCount = replyCount;
                _uf.ReplyRepository.Update(reply);
                UpdateIntegrate(user, (int)IntegrateType.Reply);
                return true;
            }
        }

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UserAuthorize]
        public async Task<IActionResult> DeleteReply(int id)
        {
            var data = new MoData();
            var reply = await _uf.ReplyRepository.GetByIdAsync(id);
            if (reply == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的评论";
                return Json(data);
            }

            if (reply.ReplyType == ReplyType.Image)
            {
                //var path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + reply.Message.Substring(6).Replace('/', Path.DirectorySeparatorChar);
                //if(System.IO.File.Exists(path))
                //{
                //    System.IO.File.Delete(path);
                //}
                DeleteImg(reply.Message);
            }

            var childReplies = await _uf.ReplyUserRepository.GetAllAsync(x => x.TopicId == reply.TopicId && x.ReplyIndex == reply.ReplyIndex);
            if (childReplies != null && childReplies.Any())
            {
                foreach (var childReply in childReplies)
                {
                    if (childReply.ReplyType == ReplyType.Image)
                    {
                        DeleteImg(childReply.Message);
                    }
                }

                _uf.ReplyUserRepository.DeleteRange(childReplies);
            }

            _uf.ReplyRepository.Delete(reply);
            // 删除索引
            JiebaLuceneHelper.Instance.Delete(reply.Id.ToString());

            await _uf.SaveChangesAsync();

            data.IsOk = true;
            return Json(data);
        }

        /// <summary>
        /// 删除子回复
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UserAuthorize]
        public async Task<IActionResult> DeleteChildReply(int id)
        {
            var data = new MoData();
            var childReply = await _uf.ReplyUserRepository.GetByIdAsync(id);
            if (childReply == null)
            {
                data.IsOk = false;
                data.Msg = "找不到指定的评论";
                return Json(data);
            }

            if (childReply.ReplyType == ReplyType.Image)
            {
                //var path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + childReply.Message.Substring(6).Replace('/', Path.DirectorySeparatorChar);
                //if (System.IO.File.Exists(path))
                //{
                //    System.IO.File.Delete(path);
                //}
                DeleteImg(childReply.Message);
            }

            _uf.ReplyUserRepository.Delete(childReply);
            await _uf.SaveChangesAsync();

            data.IsOk = true;
            return Json(data);
        }

        /// <summary>
        /// 删除帖子
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [UserAuthorize]
        public async Task<IActionResult> DeleteTopic(int id)
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

            if (userInfo.Id != topic.UserId)
            {
                if (!userInfo.Roles.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
                {
                    var bbs = await _uf.BBSRepository.GetByIdAsync(topic.BbsId);
                    if (bbs.UserId != userInfo.Id)
                    {
                        data.IsOk = false;
                        data.Msg = "您没有删除此贴的权限";
                        return Json(data);
                    }
                }
            }

            var separator = Path.DirectorySeparatorChar;
            var path = Directory.GetCurrentDirectory() + separator + _mapSetting.UpContentPhotoPath.Replace("\\", $"{separator}") + separator + topic.Id;
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            var replies = await _uf.ReplyRepository.GetAllAsync(x => x.TopicId == topic.Id);
            foreach (var reply in replies)
            {
                JiebaLuceneHelper.Instance.Delete(reply.Id.ToString());
            }

            _uf.TopicRepository.Delete(topic);
            if (await _uf.SaveChangesAsync() > 0)
            {
                data.IsOk = true;
            }
            else
            {
                data.IsOk = false;
                data.Msg = "删除失败，请稍后再试";
            }

            return Json(data);
        }

        #region 辅助方法

        private IActionResult ReturnJsonMsg(bool isOk, string msg, object data=null)
        {
            return Json(new MoData { IsOk = isOk, Msg = msg, Data = data });
        }

        private IActionResult ReturnViewMsg(string msg)
        {
            this.MsgBox(msg);
            return View();
        }

        /// <summary>
        /// 更新积分
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="intergrate"></param>
        /// <returns></returns>
        private bool UpdateIntegrate(int userId, int integrate)
        {
            return UpdateIntegrate(_uf.UserRepository.GetById(userId), integrate);
        }

        /// <summary>
        /// 更新积分
        /// </summary>
        /// <param name="user"></param>
        /// <param name="integrate"></param>
        /// <returns></returns>
        private bool UpdateIntegrate(User user, int integrate)
        {
            if (user == null)
            {
                return false;
            }

            user.Integrate += integrate;
            _uf.UserRepository.Update(user);
            return true;
        }

        /// <summary>
        /// 保存图片
        /// </summary>
        /// <param name="topicId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        private (bool, string) SaveImg(int topicId, IFormFile file)
        {
            var result = String.Empty;
            if (file == null)
            {
                result = "请选择上传的图片";
                return (false, result);
            }

            var maxSize = 1024 * 1024 * 4; // 图片大小不超过4M
            if (file.Length > maxSize)
            {
                result = "图片不能大于4M";
                return (false, result);
            }

            var separator = Path.DirectorySeparatorChar;
            var directory = Directory.GetCurrentDirectory() + separator + _mapSetting.UpContentPhotoPath + separator + topicId;
            new DirectoryInfo(directory).CreateDirectory();

            var fileExtend = file.FileName.Substring(file.FileName.LastIndexOf('.'));
            var fileNewName = $"{DateTime.Now.Ticks}{fileExtend}";
            var des = Path.Combine(directory, fileNewName);

            using (var stream = new FileStream(des, FileMode.OpenOrCreate, FileAccess.Write))
            {
                file.CopyTo(stream); // 读取上传的图片并保存
            }

            result = $"../../wwwroot{_mapSetting.ViewContentPhotoPath}/{topicId}/{fileNewName}";
            return (true, result);
        }


        /// <summary>
        /// 删除图片
        /// </summary>
        /// <param name="path"></param>
        private void DeleteImg(string path)
        {
            path = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + path.Substring(6).Replace('/', Path.DirectorySeparatorChar);
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        /// <summary>
        /// 添加索引
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="reply"></param>
        private void AddIndex(Topic topic, Reply reply)
        {
            JiebaLuceneHelper.Instance.AddIndex(new MoContentSearchItem
            {
                Id = reply.Id,
                TopicId = topic.Id,
                TopicName = topic.TopicName,
                Content = reply.Message,
                ReplyIndex = reply.ReplyIndex,
                ReplyType = reply.ReplyType,
                CreateTime = reply.CreateTime.ToStandardFormatString()
            });
        }

        #endregion
    }
}
