using KiraNet.GutsMvc.BBS.Infrastructure;
using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using KiraNet.GutsMvc.BBS.Models;
using KiraNet.GutsMvc.WebSocketHub;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace KiraNet.GutsMvc.BBS.Hub
{
    [HubProtocol("gutsmvc_bbs")]
    public class ChatHub : KiraNet.GutsMvc.WebSocketHub.Hub
    {
        private readonly GutsMvcUnitOfWork _uf;
        private readonly IGutsMvcLogger _logger;
        private MoUserInfo _userInfo;
        private int _targetUserId;

        public ChatHub(GutsMvcUnitOfWork uf, ILogger<GutsMvcBBS> logger, int targetUserId)
        {
            _uf = uf;
            _logger = new GutsMvcLogger(logger, uf);
            _targetUserId = targetUserId;
        }

        public override async Task OnContentedAsync()
        {
            if (!HttpContext.Request.IsWebSocketRequest)
            {
                await OnDisconnectedAsync(false);
            }

            if (!HttpContext.TryGetUserInfo(out var userInfo) || userInfo == null)
            {
                await OnDisconnectedAsync(false);
            }

            var initResult = await this.InitChatAsync(userInfo.Id, _targetUserId);
            if (!initResult)
            {
                await OnDisconnectedAsync(false);
            }

            _userInfo = userInfo;
        }


        /// <summary>
        /// 聊天消息处理中心
        /// </summary>
        /// <param name="targetUserId"></param>
        /// <returns></returns>
        public override async Task OnReceiveAsync(string message)
        {
            var pool = HubConnectionPool.Pool;
            try
            {
                if (State == WebSocketState.Open)
                {
                    if (State != WebSocketState.Open || message == String.Empty)//连接关闭
                    {
                        pool.TryRemoveValue(_targetUserId, _userInfo.Id);
                        await OnDisconnectedAsync(false);
                        return;
                    }

                    var formatMessageResult = this.FormatMessage(message);
                    if (formatMessageResult.result)
                    {
                        // 客户端切换聊天对象
                        message = formatMessageResult.message;
                        await this.SwitchChatAsync(_userInfo.Id, _targetUserId, formatMessageResult.targetUserId);
                        _targetUserId = formatMessageResult.targetUserId;
                    }

                    var chat = new Chat
                    {
                        UserId = _userInfo.Id,
                        TargetUserId = _targetUserId,
                        Message = message
                    };

                    if (pool.TryGetValue(_userInfo.Id, _targetUserId, out GutsMvc.WebSocketHub.Hub targetHub) &&
                        targetHub != null &&
                        targetHub.State == WebSocketState.Open)
                    {
                        await targetHub.OnSendAsync($"{_userInfo.Id}:{message}");
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
                pool.TryRemove(_userInfo.Id);
                _logger.LogError(_userInfo.Id, $"发送消息时发生异常：{ex.Message}-{DateTime.Now.ToStandardFormatString()}");
            }
        }

     
        #region 辅助方法

        private async Task<bool> SwitchChatAsync(int userId, int oldTargetUserId, int newTargetUserId)
        {
            var pool = HubConnectionPool.Pool;
            pool.TryRemoveValue(oldTargetUserId, userId);
            return await InitChatAsync(userId, newTargetUserId);
        }

        private async Task<bool> InitChatAsync(int userId, int targetUserId)
        {
            if (!await _uf.UserRepository.IsExistAsync(x => x.Id == targetUserId))
            {
                // 不存在该用户
                await OnSendAsync("找不到对方的账号");
                return false;
            }

            var pool = HubConnectionPool.Pool;

            // 用户添加或更新到对方连接池中
            if (pool.Contains(targetUserId, userId))
            {
                pool.TryUpdateValue(userId, targetUserId, this);
            }
            else
            {
                pool.TryAddValue(targetUserId, userId, this);
            }

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
