using Grpc.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agvc.Simulator
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Environment.SetEnvironmentVariable("GRPC_TRACE", "http,server,keepalive");
            //// 設定日誌詳細程度為 DEBUG
            Environment.SetEnvironmentVariable("GRPC_VERBOSITY", "DEBUG");
            //private NLog.Logger logger = LogManager.GetCurrentClassLogger();
            // 設定執行緒池大小
            int minWorkerThreads = 100;
            int minCompletionPortThreads = 100;
            //int maxWorkerThreads = 1000;
            //int maxCompletionPortThreads = 1000;

            ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
            //ThreadPool.SetMaxThreads(maxWorkerThreads, maxCompletionPortThreads);
            // 全域例外攔截：UI Thread
            Application.ThreadException += (sender, e) =>
            {
                LogManager.GetCurrentClassLogger().Error(e.Exception.ToString());
                MessageBox.Show("發生錯誤，請查看 crash_ui.log");
            };

            // 全域例外攔截：非 UI Thread
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = (Exception)e.ExceptionObject;
                LogManager.GetCurrentClassLogger().Error(ex.ToString());
                MessageBox.Show("非UI執行緒發生錯誤，請查看 crash_appdomain.log");
            };
            GrpcEnvironment.SetLogger(new NLogGrpcLogger());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
