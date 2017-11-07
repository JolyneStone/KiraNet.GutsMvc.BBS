using KiraNet.GutsMvc.BBS.Infrastructure.Entities;
using Microsoft.Extensions.Logging;
using System;

namespace KiraNet.GutsMvc.BBS.Infrastructure
{
    /// <summary>
    /// 针对Logger的简单封装
    /// </summary>
    public interface IGutsMvcLogger
    {
        IDisposable BeginScope<TState>(TState state);
        bool IsEnabled(LogLevel logLevel);
        void LogDebug(int userId, string message);
        void LogError(int userId, string message);
        void LogWarning(int userId, string message);
        void LogInformation(int userId, string message);
        void LogCritical(int userId, string message);
        void LogTrace(int userId, string message);
        void LogDisposable();
    }

    public class GutsMvcLogger : IGutsMvcLogger
    {
        private readonly ILogger _logger;
        private readonly GutsMvcUnitOfWork _unitOfWork;

        public GutsMvcLogger(ILogger logger, GutsMvcUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        //public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        //{
        //    _logger.Log(logLevel, eventId, state, exception, formatter);
        //}



        public void LogDebug(int userId, string message)
        {
            _logger.LogDebug(message);
            Log(userId, message, LogLevel.Debug);
        }

        public void LogError(int userId, string message)
        {
            _logger.LogError(message);
            Log(userId, message, LogLevel.Error);
        }

        public void LogWarning(int userId, string message)
        {
            _logger.LogWarning(message);
            Log(userId, message, LogLevel.Warning);
        }

        public void LogInformation(int userId, string message)
        {
            _logger.LogInformation(message);
            Log(userId, message, LogLevel.Information);

        }

        public void LogCritical(int userId, string message)
        {
            _logger.LogCritical(message);
            Log(userId, message, LogLevel.Critical);
        }

        public void LogTrace(int userId, string message)
        {
            _logger.LogTrace(message);
            Log(userId, message, LogLevel.Trace);
        }


        public void LogDisposable()
        {
            if (_unitOfWork != null)
            {
                _unitOfWork.Dispose();
            }
        }

        private void Log(int userId, string message, LogLevel logLevel)
        {
            try
            {
                _unitOfWork.LogRepository.Insert(new Log
                {
                    LogLevel = (int)logLevel,
                    UserId = userId,
                    Message = message.Length > 50 ? message.Substring(0, 50) + "..." : message
                });

                _unitOfWork.SaveChanges();
            }
            catch(Exception ex)
            {

            }
        }

        //private enum BBSLogLevel
        //{
        //    Trace = 0,
        //    Debug = 1,
        //    Information = 2,
        //    Warning = 3,
        //    Error = 4,
        //    Critical = 5,
        //    None = 6,
        //    Integrate = 7
        //}
    }
}
