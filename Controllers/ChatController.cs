using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Controllers
{
    public class ChatController : Controller
    {
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;
        public ChatController(GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger)
        {
            _uf = uf;
            _logger = new GutsMvcLogger(logger, uf);
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? targetUserId = null)
        {
            var targetUserJson = String.Empty;
            if (targetUserId != null)
            {
                var targetUser = (await _uf.UserRepository.GetByIdAsync(targetUserId.Value));
                if (targetUser == null)
                {
                    return RedirectToAction("home", "error", new Dictionary<string, object> { { "msg", "找不到指定的用户" } });
                }

                targetUserJson = Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    TargetUserId = targetUser.Id,
                    HeadPhoto = targetUser.HeadPhoto,
                    UserName = targetUser.UserName
                });
            }

            HttpContext.TryGetUserInfo(out var userInfo);
            ViewData["Id"] = userInfo.Id;
            ViewData["UserName"] = userInfo.UserName;
            ViewData["HeadPhoto"] = userInfo.HeadPhoto;
            ViewData["Role"] = userInfo.Roles.ToLower();
            return View(typeof(String), targetUserJson);
        }

        [WebSocket]
        public async Task<IActionResult> ChatHub(int targetUserId)
        {
            if (!HttpContext.Request.IsWebSocketRequest)
            {
                return WebSocket(System.Net.HttpStatusCode.SwitchingProtocols);
            }

            var socket = (await HttpContext.WebStocket.GetWebSocketAsync("gutsmvc")).WebSocket;
            HttpContext.TryGetUserInfo(out var userInfo);
            var initResult = await this.InitChatAsync(userInfo.Id, targetUserId, socket);
            if (!initResult)
            {
                return WebSocket(System.Net.HttpStatusCode.ServiceUnavailable);
            }

            while (true)
            {
                var pool = WebSocketConnectionPool.Pool;
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        var message = await socket.ReceiveAsync();
                        if (socket.State != WebSocketState.Open || message == String.Empty)//连接关闭
                        {
                            pool.TryRemoveValue(targetUserId, userInfo.Id);
                            break;
                        }

                        var formatMessageResult = this.FormatMessage(message);
                        if (formatMessageResult.result)
                        {
                            // 客户端切换聊天对象
                            message = formatMessageResult.message;
                            await this.SwitchChatAsync(userInfo.Id, targetUserId, formatMessageResult.targetUserId, socket);
                            targetUserId = formatMessageResult.targetUserId;
                        }

                        var chat = new Chat
                        {
                            UserId = userInfo.Id,
                            TargetUserId = targetUserId,
                            Message = message
                        };

                        if (pool.TryGetValue(userInfo.Id, targetUserId, out WebSocket targetSocket) &&
                            targetSocket != null &&
                            targetSocket.State == WebSocketState.Open)
                        {
                            await targetSocket.SendAsync($"{userInfo.Id}:{message}");
                            chat.IsArrive = true;
                        }
                        else
                        {
                            chat.IsArrive = false;
                        }

                        _uf.ChatRepository.Insert(chat);
                        _uf.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    pool.TryRemove(userInfo.Id);
                    _logger.LogError(userInfo.Id, $"发送消息时发生异常：{ex.Message}-{DateTime.Now.ToStandardFormatString()}");
                    break;
                }
            }

            return WebSocket(System.Net.HttpStatusCode.OK);
        }

        [HttpGet]
        public async Task<IActionResult> GetChatHistory(int targetUserId, bool isHistory, string nowTime = "")
        {
            var data = new MoData();
            HttpContext.TryGetUserInfo(out var userInfo);
            if (targetUserId == userInfo.Id)
            {
                data.IsOk = false;
                return Json(data);
            }

            DateTime dt;
            if (String.IsNullOrWhiteSpace(nowTime))
            {
                dt = DateTime.Now;
            }
            else
            {
                dt = nowTime.ToCSharpDateTime();
            }

            IList<Chat> chats;
            if (isHistory)
            {
                // 对方的离线消息
                chats = (await _uf.ChatRepository.GetOffLineChatAsync(targetUserId, userInfo.Id, dt)).ToList();

                data.Data = chats.Select(x => new
                {
                    Id = x.UserId,
                    Message = x.Message
                }).ToList();

                foreach (var chat in chats)
                {
                    chat.IsArrive = true;
                }

                _uf.ChatRepository.UpdateRange(chats);
                await _uf.SaveChangesAsync();
            }
            else
            {
                // 历史消息
                chats = (await _uf.ChatRepository
                    .GetOnLineChatAsync(userInfo.Id, targetUserId, dt))
                    .ToList();
            }

            data.Data = new
            {
                Chats = chats.Select(x => new
                {
                    Id = x.UserId,
                    Message = x.Message
                }).ToList(),
                NowTime = (chats.Count > 0 ? chats.Last().CreateTime : DateTime.Now).ToJSString()
            };

            data.IsOk = true;
            return Json(data);
        }

        #region Chat 辅助方法

        //private async Task HandleOffLineMessageAsync(int userId, int targetUserId, WebSocket webSocket)
        //{
        //    if (webSocket == null)
        //    {
        //        throw new ArgumentNullException(nameof(webSocket));
        //    }

        //    // 获取当前用户的离线消息
        //    var userChats = (await _uf.ChatRepository.GetOffLineChatAsync(userId, targetUserId)).ToList();

        //    foreach (var chat in userChats)
        //    {
        //        if (await webSocket.SendAsync($"{targetUserId}:{chat.Message}"))
        //        {
        //            chat.IsArrive = true;
        //        }
        //    }

        //    _uf.ChatRepository.UpdateRange(userChats);
        //}

        private async Task<bool> SwitchChatAsync(int userId, int oldTargetUserId, int newTargetUserId, WebSocket webSocket)
        {
            var pool = WebSocketConnectionPool.Pool;
            pool.TryRemoveValue(oldTargetUserId, userId);
            return await InitChatAsync(userId, newTargetUserId, webSocket);
        }

        private async Task<bool> InitChatAsync(int userId, int targetUserId, WebSocket webSocket)
        {
            if (!await _uf.UserRepository.IsExistAsync(x => x.Id == targetUserId))
            {
                // 不存在该用户
                await webSocket.SendAsync("找不到对方的账号");
                return false;
            }

            var pool = WebSocketConnectionPool.Pool;

            // 用户添加或更新到对方连接池中
            if (pool.Contains(targetUserId, userId))
            {
                pool.TryUpdateValue(userId, targetUserId, webSocket);
            }
            else
            {
                pool.TryAddValue(targetUserId, userId, webSocket);
            }

            //// 获取对方的离线消息
            //await this.HandleOffLineMessageAsync(targetUserId, userId, webSocket);

            //// 获取当前用户的离线消息
            //if (pool.TryGetValue(userId, targetUserId, out var targetSocket) && targetSocket != null)
            //{
            //    await this.HandleOffLineMessageAsync(userId, targetUserId, targetSocket);
            //}

            await _uf.SaveChangesAsync();
            return true;
        }

        private (bool result, int targetUserId, string message) FormatMessage(string originMessage)
        {
            var index = originMessage.IndexOf('-');
            if (index > -1 &&
                Int32.TryParse(originMessage.Substring(0, index), out var newTargetUserId) &&
                newTargetUserId > 0)
                return (true, newTargetUserId, originMessage.Substring(index + 1));
            else
                return (false, 0, String.Empty);
        }

        #endregion
    }
}
