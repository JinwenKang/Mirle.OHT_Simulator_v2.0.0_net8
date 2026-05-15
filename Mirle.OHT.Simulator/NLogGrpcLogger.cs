using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core.Logging;

namespace Mirle.Agvc.Simulator
{
    public class NLogGrpcLogger : Grpc.Core.Logging.ILogger
    {
        private readonly NLog.Logger _logger = NLog.LogManager.GetLogger("grpcTraceFile");

        public Grpc.Core.Logging.ILogger ForType<T>() => this;

        public void Debug(string message) => _logger.Debug(message);
        public void Debug(string format, params object[] formatArgs) => _logger.Debug(format, formatArgs);
        public void Info(string message) => _logger.Info(message);
        public void Info(string format, params object[] formatArgs) => _logger.Info(format, formatArgs);
        public void Warning(string message) => _logger.Warn(message);
        public void Warning(string format, params object[] formatArgs) => _logger.Warn(format, formatArgs);
        public void Warning(Exception exception, string message) => _logger.Warn(exception, message);
        public void Error(string message) => _logger.Error(message);
        public void Error(string format, params object[] formatArgs) => _logger.Error(format, formatArgs);
        public void Error(Exception exception, string message) => _logger.Error(exception, message);
    }
}
