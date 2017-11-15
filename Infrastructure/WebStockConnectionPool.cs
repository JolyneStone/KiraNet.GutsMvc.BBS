using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    /// <summary>
    /// WebSocket连接池
    /// </summary>
    public class WebSocketConnectionPool
    {
        private static object _sync = new object();
        private SpinLock _spin = new SpinLock();
        private static WebSocketConnectionPool _instance;
        private IDictionary<int, IDictionary<int, WebSocket>> _pool;

        /// <summary>
        /// 获取WebSocket实例
        /// </summary>
        public static WebSocketConnectionPool Pool
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new WebSocketConnectionPool();
                        }
                    }
                }

                return _instance;
            }
        }
        private WebSocketConnectionPool() => _pool = new ConcurrentDictionary<int, IDictionary<int, WebSocket>>();

        public bool TryAdd(int userId, IDictionary<int, WebSocket> values)
        {
            var lockTaken = false;
            try
            {
                _spin.Enter(ref lockTaken);
                return _pool.TryAdd(userId, values);
            }
            finally
            {
                if(lockTaken)
                {
                    _spin.Exit();
                }
            }
        }

        public bool TryAddValue(int userId, int targetUserId, WebSocket webSocket)
        {
            var lockTaken = false;
            try
            {
                if (_pool.TryGetValue(userId, out var values) && values != null)
                {
                    if (values.TryAdd(targetUserId, webSocket))
                        return true;

                    return false;
                }

                values = new Dictionary<int, WebSocket>() { { targetUserId, webSocket } };
                _pool[userId] = values;
                return true;
            }
            finally
            {
                if(lockTaken)
                {
                    _spin.Exit();
                }
            }
        }

        public bool Contains(int userId) => _pool.ContainsKey(userId);

        public bool Contains(int userId, int targetUserId) => _pool.TryGetValue(userId, out var values) && values.ContainsKey(targetUserId);

        public bool TryUpdateValue(int userId, int targetUserId, WebSocket webSocket)
        {
            if (_pool.TryGetValue(userId, out var values) && values != null)
            {
                if (values.ContainsKey(targetUserId))
                {
                    var lockTaken = false;
                    try
                    {
                        _spin.Enter(ref lockTaken);
                        values[targetUserId] = webSocket;
                    }
                    finally
                    {
                        if(lockTaken)
                        {
                            _spin.Exit();
                        }
                    }

                    return true;
                }
            }

            return false;
        }

        public bool TryRemove(int userId)
        {
            var lockTaken = false;
            try
            {
                _spin.Enter(ref lockTaken);
                return _pool.Remove(userId, out var value);
            }
            finally
            {
                if (lockTaken)
                {
                    _spin.Exit();
                }
            }
        }

        public bool TryRemoveValue(int userId, int targetUserId)
        {
            var lockTaken = false;
            try
            {
                _spin.Enter(ref lockTaken);
                return _pool.TryGetValue(userId, out var values) && values.Remove(targetUserId);
            }
            finally
            {
                if (lockTaken)
                {
                    _spin.Exit();
                }
            }
        }

        public bool TryGet(int userId, out IDictionary<int, WebSocket> values) => _pool.TryGetValue(userId, out values);

        public bool TryGetValue(int userId, int targetUserId, out WebSocket webSocket)
        {
            webSocket = null;
            if (_pool.TryGetValue(userId, out var values) && values.TryGetValue(targetUserId, out webSocket))
                return true;

            return false;
        }
    }
}
