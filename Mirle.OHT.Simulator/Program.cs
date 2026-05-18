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
            // ThreadPool min thread 設定
            // 目標：支援 300 台車模擬。每車有多個 Thread.Sleep polling 迴圈 + gRPC + Timer。
            // 預估同時活躍 thread 數 = 300 * 3 ≈ 900，給 1024 buffer 避免 ThreadPool 增長延遲。
            // 注意：min thread 越高，閒置時 process 記憶體越多；正式上線可依車數調整。
            const int minWorkerThreads = 1024;
            const int minCompletionPortThreads = 1024;
            ThreadPool.SetMinThreads(minWorkerThreads, minCompletionPortThreads);
            // NLog Error target 實際寫入路徑（見 NLog.config）：
            //   D:\OHTC_Sim_Log\yyyyMMdd\Error.log
            const string LOG_HINT = @"D:\OHTC_Sim_Log\{yyyyMMdd}\Error.log";

            // 全域例外攔截：UI Thread
            Application.ThreadException += (sender, e) =>
            {
                LogManager.GetCurrentClassLogger().Error(e.Exception, "[UI Thread] Unhandled exception");
                MessageBox.Show(
                    $"UI 執行緒發生例外：{e.Exception.GetType().Name}{Environment.NewLine}{Environment.NewLine}" +
                    $"{e.Exception.Message}{Environment.NewLine}{Environment.NewLine}" +
                    $"完整紀錄請看：{LOG_HINT}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            // 全域例外攔截：非 UI Thread
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                LogManager.GetCurrentClassLogger().Error(ex, "[Non-UI Thread] Unhandled exception (IsTerminating={0})", e.IsTerminating);
                MessageBox.Show(
                    $"非 UI 執行緒發生例外：{ex?.GetType().Name}{Environment.NewLine}{Environment.NewLine}" +
                    $"{ex?.Message}{Environment.NewLine}{Environment.NewLine}" +
                    $"完整紀錄請看：{LOG_HINT}",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };
            GrpcEnvironment.SetLogger(new NLogGrpcLogger());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
