using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    /// <summary>
    /// Hub连接池
    /// </summary>
    public class HubConnectionPool
    {
        private static object _sync = new object();
        private SpinLock _spin = new SpinLock();
        private static HubConnectionPool _instance;
        private IDictionary<int, IDictionary<int, GutsMvc.WebSocketHub.Hub>> _pool;

        /// <summary>
        /// 获取hub实例
        /// </summary>
        public static HubConnectionPool Pool
        {
            get
            {
                if (_instance == null)
                {
                    lock (_sync)
                    {
                        if (_instance == null)
                        {
                            _instance = new HubConnectionPool();
                        }
                    }
                }

                return _instance;
            }
        }
        private HubConnectionPool() => _pool = new ConcurrentDictionary<int, IDictionary<int, GutsMvc.WebSocketHub.Hub>>();

        public bool TryAdd(int userId, IDictionary<int, GutsMvc.WebSocketHub.Hub> values)
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

        public bool TryAddValue(int userId, int targetUserId, GutsMvc.WebSocketHub.Hub hub)
        {
            var lockTaken = false;
            try
            {
                if (_pool.TryGetValue(userId, out var values) && values != null)
                {
                    if (values.TryAdd(targetUserId, hub))
                        return true;

                    return false;
                }

                values = new Dictionary<int, GutsMvc.WebSocketHub.Hub>() { { targetUserId, hub } };
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

        public bool TryUpdateValue(int userId, int targetUserId, GutsMvc.WebSocketHub.Hub hub)
        {
            if (_pool.TryGetValue(userId, out var values) && values != null)
            {
                if (values.ContainsKey(targetUserId))
                {
                    var lockTaken = false;
                    try
                    {
                        _spin.Enter(ref lockTaken);
                        values[targetUserId] = hub;
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

        internal bool TryGetValue(int id, int targetUserId, out object targetHub)
        {
            throw new NotImplementedException();
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

        public bool TryGet(int userId, out IDictionary<int, GutsMvc.WebSocketHub.Hub> values) => _pool.TryGetValue(userId, out values);

        public bool TryGetValue(int userId, int targetUserId, out GutsMvc.WebSocketHub.Hub hub)
        {
            hub = null;
            if (_pool.TryGetValue(userId, out var values) && values.TryGetValue(targetUserId, out hub))
                return true;

            return false;
        }
    }
}
