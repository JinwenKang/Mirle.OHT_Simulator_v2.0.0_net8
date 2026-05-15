using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service.gRPC
{
    public class VehicleMessageSend
    {
        public ushort m_iSendSeqNum { get; private set; }
        private const ushort SENDSEQNUM_MAX = 999;
        private const ushort SENDSEQNUM_MIN = 1;
        private static readonly string TimestampFormat_14 = "yyyyMMddHHmmss";
        string name = "";
        string ip = "";
        Channel channel = null;
        vehicleGreeter.vehicleGreeterClient client;
        //NLog.Logger logger = NLog.LogManager.GetLogger("gRCPWithVh");
        private NLog.Logger logger = NLog.LogManager.GetLogger("gRPC_ConnectionRecord");
        public event EventHandler Connection;
        public event EventHandler Disconnection;

        private bool isconnection = false;
        public bool isConnection
        {
            get
            {
                return isconnection;
            }
            set
            {
                if (isconnection != value)
                {

                    isconnection = value;
                    logger.Info($" gRPC client IP:{ip} ,is connection:{isconnection} ");

                    if (isconnection)
                    {
                        Connection?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        Disconnection?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }


        public VehicleMessageSend(string _name, string _ip, int port)
        {
            name = _name;
            ip = _ip;
            logger.Info($"Creat gRPC client IP:{ip} PORT:{port}");

            var channelOptions = new List<ChannelOption>
            {
                new ChannelOption("grpc.keepalive_time_ms", 60000),  // keepalive ping 的間隔時間，單位為毫秒
                new ChannelOption("grpc.keepalive_timeout_ms", 10000), // keepalive ping 後等待的超時時間，單位為毫秒
                new ChannelOption("grpc.http2.min_time_between_pings_ms", 10000),  // 連續兩次ping之間最小的間隔時間，單位為毫秒
                new ChannelOption("grpc.http2.max_pings_without_data", 1), // 在沒有資料傳輸的情況下，最多允許的ping數量
                new ChannelOption("grpc.keepalive_permit_without_calls", 1)  // 允許在沒有呼叫的情況下進行keepalive pings
            };

            channel = new Channel($"{ip}:{port}", ChannelCredentials.Insecure, channelOptions);
            client = new vehicleGreeter.vehicleGreeterClient(channel);
        }
        public void Start()
        {
            initialAliveAction();
        }

        // ===== 功能二：OHTC連線切換 - 新增Stop方法 =====
        public async Task Stop()
        {
            try
            {
                logger.Info($"Stopping gRPC client IP:{ip}");
                
                if (channel != null)
                {
                    // 優雅地關閉Channel
                    await channel.ShutdownAsync();
                    isConnection = false;
                    logger.Info($"gRPC client IP:{ip} stopped successfully");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error stopping gRPC client IP:{ip}");
                throw;
            }
        }
        // ===== 新增結束 =====

        private void initialAliveAction()
        {
            //Task.Factory.StartNew(() =>
            //{
            //    connectionCheck();
            //},
            //   TaskCreationOptions.LongRunning);
        }
        const int GRPC_CONNECTION_CHECK_INTERVAL_TIME_MS = 5_000;
        private async void connectionCheck()
        {
            do
            {
                try
                {
                    var deadline = DateTime.UtcNow.AddMilliseconds(GRPC_CONNECTION_CHECK_INTERVAL_TIME_MS);
                    await channel.ConnectAsync(deadline);
                    isConnection = channel.State == ChannelState.Ready;
                }
                catch (TaskCanceledException ex)
                {
                    isConnection = false;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception");
                }
                //System.Threading.SpinWait.SpinUntil(() => false, 5000);
                Thread.Sleep(5000);
            }
            while (true);
        }
        //const int SEND_MESSAGE_TIME_OUT_SECONDS = 30;
        //const int SEND_MESSAGE_CANCEL_TOKEN_TIME_OUT_MILLISECONDS = 30_000;
        private static readonly SemaphoreSlim semaphore = new SemaphoreSlim(200);

        private async Task<(bool isSuccess, WrapperMessage message)> SendMessageInternalAsync(
            WrapperMessage sendWrapperMsg,
            string log_msg,
            bool returnEmptyOnTransEventRep)
        {
            sendWrapperMsg.VehicleName = name;
            sendWrapperMsg.SeqNum = getSendSeqNum();

            const int MAX_RETRY = 2;
            const int DELAY_BETWEEN_RETRY_MS = 100;

            for (int retry = 0; retry <= MAX_RETRY; retry++)
            {
                CancellationTokenSource cts = null;
                try
                {
                    recordMessage(ip, sendWrapperMsg);

                    cts = new CancellationTokenSource(TimeSpan.FromSeconds(SystemParameter.GrpcSendTimeoutTime));
                    var task_reply = client.UnaryMessageAsync(sendWrapperMsg, cancellationToken: cts.Token);

                    if (sendWrapperMsg.ID == WrapperMessage.TransEventRepFieldNumber)
                    {
                        return returnEmptyOnTransEventRep ? (true, new WrapperMessage()) : (false, null);
                    }

                    var reply = await task_reply.ResponseAsync;
                    recordMessage(ip, reply);
                    return (true, reply);
                }
                catch (RpcException ex)
                {
                    logger.Error($"[Retry:{retry}] Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} RpcException: {ex.StatusCode}, {ex.Message}");
                    //if (retry < MAX_RETRY && IsRetriable(ex.StatusCode))
                    if (retry < MAX_RETRY)
                        await Task.Delay(DELAY_BETWEEN_RETRY_MS);
                    else
                        break;
                }
                catch (Exception ex)
                {
                    logger.Error($"[Retry:{retry}] Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Unexpected Exception: {ex}");
                    break;
                }
                finally
                {
                    if (cts != null)
                    {
                        cts.Dispose();
                    }
                }
            }

            return (false, null);
        }

        private bool IsRetriable(StatusCode code)
        {
            return code == StatusCode.Unavailable ||
                   code == StatusCode.DeadlineExceeded ||
                   code == StatusCode.ResourceExhausted;
        }

        public async Task<(bool isSuccess, WrapperMessage message)> SendMessageUnaryAsync(WrapperMessage msg)
        {
            await semaphore.WaitAsync();
            try
            {
                return await SendMessageInternalAsync(msg, "SendMessageUnaryAsync", true);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnary(WrapperMessage msg)
        {
            try
            {
                return SendMessageUnaryAsync(msg).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendMessageUnary failed");
                return (false, null);
            }
        }

        public async Task<(bool isSuccess, WrapperMessage message)> SendMessageUnaryNoRetryAsync(WrapperMessage msg, string log_msg)
        {
            await semaphore.WaitAsync();
            try
            {
                return await SendMessageInternalAsync(msg, log_msg, true);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnaryNoRetry(WrapperMessage msg, string log_msg)
        {
            try
            {
                return SendMessageUnaryNoRetryAsync(msg, log_msg).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendMessageUnaryNoRetry failed");
                return (false, null);
            }
        }

        public async Task<(bool isSuccess, WrapperMessage message)> SendMessageUnaryNewAsync(WrapperMessage msg, string log_msg)
        {
            await semaphore.WaitAsync();
            try
            {
                return await SendMessageInternalAsync(msg, log_msg, false);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnaryNew(WrapperMessage msg, string log_msg)
        {
            try
            {
                return SendMessageUnaryNewAsync(msg, log_msg).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SendMessageUnaryNew failed");
                return (false, null);
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnary_Old(WrapperMessage sendWrapperMsg)
        {
            try
            {
                sendWrapperMsg.VehicleName = name;
                sendWrapperMsg.SeqNum = getSendSeqNum();
                recordMessage(ip, sendWrapperMsg);
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                //var reply = client.UnaryMessage(sendWrapperMsg, deadline: DateTime.UtcNow.AddSeconds(5_000));
                var task_reply = client.UnaryMessageAsync(sendWrapperMsg, deadline: DateTime.UtcNow.AddSeconds(SystemParameter.GrpcSendTimeoutTime), cancellationToken: cancellationToken.Token);
                //if (!SpinWait.SpinUntil(() => task_reply.GetAwaiter().IsCompleted, SEND_MESSAGE_CANCEL_TOKEN_TIME_OUT_MILLISECONDS))
                if (!checktaskReplay(task_reply))
                {
                    cancellationToken.Cancel();
                    logger.Info($"send message:{sendWrapperMsg.SeqNum}, no recive result from grpc server seq num:{sendWrapperMsg.SeqNum}, excute cancel token.cancel token timeout:{SystemParameter.GrpcSendTimeoutTimeMs}");
                    return (false, new WrapperMessage());
                }
                var reply = task_reply.GetAwaiter().GetResult();
                recordMessage(ip, reply);
                return (true, reply);
            }
            catch (RpcException ex)
            {
                logger.Info(ex, $"sendMessageUnary RpcException happend seq num:{sendWrapperMsg.SeqNum}, status:{ex.StatusCode}, send time out:{SystemParameter.GrpcSendTimeoutTime}");
                return (false, new WrapperMessage());
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"sendMessageUnary Exception happend seq num:{sendWrapperMsg.SeqNum}");
                return (false, new WrapperMessage());
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnaryWithLog(WrapperMessage sendWrapperMsg, string log_msg)
        {
            try
            {

                sendWrapperMsg.VehicleName = name;
                sendWrapperMsg.SeqNum = getSendSeqNum();
                recordMessage(ip, sendWrapperMsg);
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                var task_reply = client.UnaryMessageAsync(sendWrapperMsg, deadline: DateTime.UtcNow.AddSeconds(SystemParameter.GrpcSendTimeoutTime), cancellationToken: cancellationToken.Token);
                //if (!SpinWait.SpinUntil(() => task_reply.GetAwaiter().IsCompleted, SEND_MESSAGE_CANCEL_TOKEN_TIME_OUT_MILLISECONDS))
                if (!checktaskReplay(task_reply))
                {
                    cancellationToken.Cancel();
                    logger.Error($"Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Result:[Did not receive reslt from grpc server]");
                    return (false, new WrapperMessage());
                }
                var reply = task_reply.GetAwaiter().GetResult();
                recordMessage(ip, reply);
                return (true, reply);
            }
            catch (RpcException ex)
            {
                logger.Error($"Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Result:[RpcException happend, status:{ex.StatusCode}, send time out:{SystemParameter.GrpcSendTimeoutTime}, Exception:{ex.ToString()}]");
                return (false, new WrapperMessage());
            }
            catch (Exception ex)
            {
                logger.Error($"Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Result:[Exception happend, Message:{ex.ToString()}]");
                return (false, new WrapperMessage());
            }
        }

        public (bool isSuccess, WrapperMessage message) sendMessageUnaryWithLogNoWait(WrapperMessage sendWrapperMsg, string log_msg)
        {
            try
            {

                sendWrapperMsg.VehicleName = name;
                sendWrapperMsg.SeqNum = getSendSeqNum();
                recordMessage(ip, sendWrapperMsg);
                CancellationTokenSource cancellationToken = new CancellationTokenSource();
                client.UnaryMessageAsync(sendWrapperMsg, deadline: DateTime.UtcNow.AddSeconds(SystemParameter.GrpcSendTimeoutTime), cancellationToken: cancellationToken.Token);

                return (true, new WrapperMessage());
            }
            catch (RpcException ex)
            {
                logger.Error($"Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Result:[RpcException happend, status:{ex.StatusCode}, send time out:{SystemParameter.GrpcSendTimeoutTime}, Exception:{ex.ToString()}]");
                return (false, new WrapperMessage());
            }
            catch (Exception ex)
            {
                logger.Error($"Seq No:[{sendWrapperMsg.SeqNum}] {log_msg} Result:[Exception happend, Message:{ex.ToString()}]");
                return (false, new WrapperMessage());
            }
        }


        object _lock = new object();
        bool IS_QUEUE_WORK = false;
        ConcurrentQueue<WrapperMessage> clientStreamQueue = new ConcurrentQueue<WrapperMessage>();
        public bool sendMessageClientStream(WrapperMessage sendWrapperMsg)
        {
            try
            {
                sendWrapperMsg.VehicleName = name;
                sendWrapperMsg.SeqNum = getSendSeqNum();

                if (channel.State == ChannelState.Idle ||
                    channel.State == ChannelState.Shutdown ||
                    channel.State == ChannelState.TransientFailure)
                {
                    Console.WriteLine("開始等待連線");
                    bool is_success = channel.ConnectAsync().Wait(5000);
                    if (!is_success) return false;
                }
                lock (_lock)
                {
                    clientStreamQueue.Enqueue(sendWrapperMsg);
                    if (!IS_QUEUE_WORK)
                    {
                        IS_QUEUE_WORK = true;
                        Task.Run(() => { doWork(); });
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private void doWork()
        {
            while (true)
            {
                try
                {
                    bool is_success = clientStreamQueue.TryDequeue(out var report);
                    if (is_success)
                    {
                        var try_result = tryGetClientStreamCall(channel);
                        if (!try_result.isSuccess) continue;
                        asyncClientStreamingCall.RequestStream.WriteAsync(report).Wait();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    asyncClientStreamingCall = null;
                }
            }
            IS_QUEUE_WORK = false;
        }
        AsyncClientStreamingCall<WrapperMessage, WrapperMessage> asyncClientStreamingCall = null;
        private (bool isSuccess, AsyncClientStreamingCall<WrapperMessage, WrapperMessage> call)
            tryGetClientStreamCall(Channel channel)
        {
            if (asyncClientStreamingCall == null)
            {
                channel.ConnectAsync().Wait(5000);
                asyncClientStreamingCall = client.ClientSteamMessage();
                return (true, asyncClientStreamingCall);
            }
            else
            {
                return (true, asyncClientStreamingCall);
            }
        }

        private void recordMessage(string ip, WrapperMessage message)
        {
            if (message == null) return;
            //var entity_msg_info = getGoogleBeSetToOneOf(message);
            //message.IpAddress = ip;
            //logger.Info(message);
            DateTime.TryParseExact(message.LocalSystemTime, TimestampFormat_14, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);
            //LogHelper.recodeLog(entity_msg_info.entityMsg,
            //                    ip,
            //                    message.VehicleName,
            //                    entity_msg_info.desc.Name,
            //                    entity_msg_info.desc.FieldNumber,
            //                    message.SeqNum,
            //                    false,
            //                    dateTime);
        }
        private static (Google.Protobuf.Reflection.FieldDescriptor desc, IMessage entityMsg) getGoogleBeSetToOneOf(IMessage PackWrapperMsg)
        {
            var descriptor = PackWrapperMsg.Descriptor;
            var oneof = descriptor.Oneofs[0];
            var value = oneof.Accessor.GetCaseFieldDescriptor(PackWrapperMsg);
            var entityMsg = value.Accessor.GetValue(PackWrapperMsg) as IMessage;
            return (value, entityMsg);
        }
        private UInt16 getSendSeqNum()
        {
            m_iSendSeqNum++;
            if (m_iSendSeqNum > SENDSEQNUM_MAX)
            {
                m_iSendSeqNum = SENDSEQNUM_MIN;
            }
            return (m_iSendSeqNum);
        }

        public bool checktaskReplay(AsyncUnaryCall<WrapperMessage> task_reply)
        {
            bool result = false;
            int timeout = SystemParameter.GrpcSendTimeoutTimeMs;
            int interval = 100; // 每次等待 100 毫秒

            for (int elapsed = 0; elapsed < timeout; elapsed += interval)
            {
                if (task_reply.GetAwaiter().IsCompleted)
                {
                    result = true;
                    break;
                }
                Thread.Sleep(interval);
            }

            return result;
        }

    }
}
