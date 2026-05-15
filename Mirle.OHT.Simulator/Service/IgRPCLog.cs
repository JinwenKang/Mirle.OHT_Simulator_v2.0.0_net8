using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service.gRPC
{
    internal class IgRPCLog : ILogger
    {
        NLog.Logger logger = NLog.LogManager.GetLogger("gRCPLog");
        public void Debug(string message)
        {
            logger.Debug(message);
        }

        public void Debug(string format, params object[] formatArgs)
        {
            logger.Debug(format, formatArgs);
        }

        public void Error(string message)
        {
            logger.Error(message);
        }

        public void Error(string format, params object[] formatArgs)
        {
            logger.Error(format, formatArgs);
        }

        public void Error(Exception exception, string message)
        {
            logger.Error(exception, message);
        }

        public ILogger ForType<T>()
        {
            return this;
        }

        public void Info(string message)
        {
            logger.Info(message);
        }

        public void Info(string format, params object[] formatArgs)
        {
            logger.Info(format, formatArgs);
        }

        public void Warning(string message)
        {
            logger.Warn(message);
        }

        public void Warning(string format, params object[] formatArgs)
        {
            logger.Warn(format, formatArgs);
        }

        public void Warning(Exception exception, string message)
        {
            logger.Warn(exception, message);
        }
    }
}
