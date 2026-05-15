using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Service.gRPC
{
    /// <summary>
    /// 繼承並實作gRPC Service提供的server事件
    /// </summary>
    public partial class VehicleMessageRceive : vehicleGreeter.vehicleGreeterBase
    {
        ConcurrentDictionary<string, TriggerEventQueue> DicDeviceTriggerEventQueue = new ConcurrentDictionary<string, TriggerEventQueue>();
        private static readonly string TimestampFormat_17 = "yyyyMMddHHmmssfff";
        public VehicleMessageRceive()
        {
            logger.Info($"initial grpc:VehicleMessageRceive...");
            ProcReceiveOrder = PROCESS_RECEIVE_ORDER.Queue;
            MessageProcByReceiveQueue = new List<string>() { "136", "132" };

        }
        const int EVENT_SIGNALED_INDEX_REMOTE_IS_CANCELED = 1;
        NLog.Logger logger = NLog.LogManager.GetLogger("gRCPWithVh");
        private NLog.Logger loggerGrpcConnction = NLog.LogManager.GetLogger("gRPC_ConnectionRecord");
        public override async Task<WrapperMessage> UnaryMessage(WrapperMessage request, ServerCallContext context)
        {
            try
            {
                var entity_msg_info = getGoogleBeSetToOneOf(request);
                string remote_ip = getRemoteIp(context);
                string event_key = $"{request.VehicleName}#{entity_msg_info.desc.FieldNumber}";
                string event_device = $"{remote_ip}#{request.VehicleName}";

                logger.Info($"[Unary] Receive from: {event_key}, PacketID: {entity_msg_info.desc.FieldNumber}, Seq: {request.SeqNum}");

                await Task.Run(() => recordMessage(remote_ip, request, entity_msg_info));

                gRPCEventArgs arg = new gRPCEventArgs(entity_msg_info.desc.FieldNumber.ToString(), request.SeqNum, entity_msg_info.entityMsg);
                await Task.Run(() => ongRPCReceive(event_device, event_key, arg));

                var waitTask = arg.eventWaitHandle.AsTask();
                var delayTask = Task.Delay(TimeSpan.FromSeconds(20), context.CancellationToken);
                var completedTask = await Task.WhenAny(waitTask, delayTask);

                if (completedTask == delayTask || context.CancellationToken.IsCancellationRequested)
                {
                    if (context.CancellationToken.IsCancellationRequested)
                    {
                        loggerGrpcConnction.Warn("[Unary] Cancelled by client: Token triggered");
                    }
                    else
                    {
                        loggerGrpcConnction.Warn($"[Unary] Timeout waiting for PacketID:{arg.PacketID}, Vehicle:{request.VehicleName}");
                    }

                    arg.eventWaitHandle.Dispose();

                    if (context.CancellationToken.IsCancellationRequested && isNotWaitEvent(arg.PacketID))
                    {
                        return arg.replyWrapperMessage;
                    }
                    else if (context.CancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(context.CancellationToken);
                    }
                    else
                    {
                        throw new TimeoutException($"[Unary] Timeout exceeded while waiting for server response. PacketID:{arg.PacketID}");
                    }
                }
                else
                {
                    await Task.Run(() => recordMessage(remote_ip, arg.replyWrapperMessage));
                    return arg.replyWrapperMessage;
                }
            }
            catch (OperationCanceledException ex)
            {
                loggerGrpcConnction.Warn(ex, $"[Unary] Operation canceled. Possibly due to client disconnect. PacketID:{request?.SeqNum}");
                throw;
            }
            catch (RpcException ex)
            {
                loggerGrpcConnction.Error(ex, $"[Unary][RPC EXCEPTION] Status: {ex.Status.StatusCode}, Detail: {ex.Status.Detail}, PacketID:{request?.SeqNum}");
                throw;
            }
            catch (Exception ex)
            {
                loggerGrpcConnction.Error(ex, $"[Unary][EXCEPTION] Unexpected error occurred. PacketID:{request?.SeqNum}, Detail:\n{request}");
                throw;
            }
        }


        // 將 WaitHandle 轉換為 Task 的擴充方法


        string[] NotWaitPacketID = new string[] { "134", "144", "194" };
        private bool isNotWaitEvent(string packetID)
        {
            if (NotWaitPacketID.Contains(packetID))
                return true;
            else
                return false;
        }

        private void recordMessage(string ip, WrapperMessage message)
        {
            if (message == null) return;
            var entity_msg_info = getGoogleBeSetToOneOf(message);
            if (!entity_msg_info.isSuccess)
                return;
            //message.IpAddress = ip;
            //logger.Info(message);


            DateTime.TryParseExact(message.LocalSystemTime, TimestampFormat_17, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);

            //LogHelper.recodeLog(entity_msg_info.entityMsg,
            //                    ip,
            //                    message.VehicleName,
            //                    entity_msg_info.desc.Name,
            //                    entity_msg_info.desc.FieldNumber,
            //                    message.SeqNum,
            //                    true,
            //                    dateTime);
        }
        private void recordMessage(string ip, WrapperMessage message, (bool isSuccess, Google.Protobuf.Reflection.FieldDescriptor desc, IMessage entityMsg) messageInfo)
        {
            try
            {
                if (message == null) return;
                //var entity_msg_info = getGoogleBeSetToOneOf(message);
                //message.IpAddress = ip;
                //logger.Info(message);
                bool is_success = DateTime.TryParseExact(message.LocalSystemTime, TimestampFormat_17, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime);
                if (is_success)
                    tryRecordWithLocalSystemTimeDifferenceCalculated(message.VehicleName, messageInfo.desc.FieldNumber, messageInfo.desc.FieldNumber, dateTime);
                //LogHelper.recodeLog(messageInfo.entityMsg,
                //                    ip,
                //                    message.VehicleName,
                //                    messageInfo.desc.Name,
                //                    messageInfo.desc.FieldNumber,
                //                    message.SeqNum,
                //                    true,
                //                    dateTime);
            }
            catch (Exception ex)
            {
                loggerGrpcConnction.Error(ex, $"Detail:{message}");
            }
        }

        private void tryRecordWithLocalSystemTimeDifferenceCalculated(string vehicleName, int messageID, int seqNo, DateTime localSystemTime)
        {
            try
            {
                if (messageID == 134) return;
                double with_local_system_time_difference = (DateTime.Now - localSystemTime).TotalMilliseconds;
                //LogHelper.RecordSystemInfo(vehicleName, messageID, seqNo, with_local_system_time_difference);
            }
            catch (Exception ex)
            {
                loggerGrpcConnction.Error(ex, "Exception:");
            }
        }

        public override async Task<WrapperMessage> ClientSteamMessage(IAsyncStreamReader<WrapperMessage> requestStream, ServerCallContext context)
        {
            logger.Info("[Stream] Start receiving stream messages...");
            try
            {
                while (await requestStream.MoveNext(context.CancellationToken))
                {
                    var request = requestStream.Current;
                    try
                    {
                        var entity_msg_info = getGoogleBeSetToOneOf(request);
                        string remote_ip = getRemoteIp(context);
                        string event_device = $"{remote_ip}#{request.VehicleName}";
                        string event_key = $"{remote_ip}#{request.VehicleName}#{entity_msg_info.desc.FieldNumber}";

                        logger.Info($"[Stream] Received: {event_key}, PacketID:{entity_msg_info.desc.FieldNumber}");

                        recordMessage(remote_ip, request, entity_msg_info);

                        var arg = new gRPCEventArgs(entity_msg_info.desc.FieldNumber.ToString(), request.SeqNum, entity_msg_info.entityMsg);
                        ongRPCReceive(event_device, event_key, arg);
                    }
                    catch (Exception innerEx)
                    {
                        loggerGrpcConnction.Error(innerEx, $"[Stream] Exception while processing message: {request}");
                    }
                }
            }
            catch (OperationCanceledException ex)
            {
                loggerGrpcConnction.Warn(ex, "[Stream] Client disconnected or stream cancelled.");
            }
            catch (Exception ex)
            {
                loggerGrpcConnction.Error(ex, "[Stream] Fatal error while reading from stream.");
                throw;
            }
            return new WrapperMessage(); // 回傳空訊息或根據業務邏輯回應
        }

        public override Task<ID_4_USER_AUTHORIZATION_RESPONSE> AuthorizationManagement(ID_104_USER_AUTHORIZATION_REQUEST request, ServerCallContext context)
        {
            ID_4_USER_AUTHORIZATION_RESPONSE result = new ID_4_USER_AUTHORIZATION_RESPONSE();
            var app = SCApplication.getInstance();
            string account = request.Account;
            string password = request.Password;

            //var funcList = app.UserBLL.getFuncCodeByLogin(account, password);
            //if(funcList.Count == 0)
            //{
            //    result.LoginSuccess = false;
            //}
            //else
            //{
            //    result.LoginSuccess = true;
            //    result.GroupName = funcList[0].USER_GRP.Trim();
            //    foreach (var func in funcList) result.AuthorizationList.Add(func.FUNC_CODE.Trim());
            //}

            return Task.FromResult(result);
        }
    }

    public static class WaitHandleExtensions
    {
        public static Task AsTask(this WaitHandle waitHandle)
        {
            var tcs = new TaskCompletionSource<bool>();
            var registration = ThreadPool.RegisterWaitForSingleObject(waitHandle, (state, timedOut) =>
            {
                if (timedOut)
                    tcs.TrySetCanceled();
                else
                    tcs.TrySetResult(true);
            }, null, Timeout.Infinite, true);
            tcs.Task.ContinueWith(_ => registration.Unregister(null));
            return tcs.Task;
        }
    }
    /// <summary>
    /// 額外實作的部分
    /// </summary>
    public partial class VehicleMessageRceive
    {
        private Dictionary<string, EventHandler<gRPCEventArgs>> RealTcpIpReceivedDic = new Dictionary<string, EventHandler<gRPCEventArgs>>();
        public PROCESS_RECEIVE_ORDER ProcReceiveOrder { get; private set; }
        public List<string> MessageProcByReceiveQueue { get; private set; }

        public enum PROCESS_RECEIVE_ORDER
        {
            Normal = 0,
            Queue = 1
        }
        public void addgRPCReceivedHandler(string eventKey, int eventID, EventHandler<gRPCEventArgs> handler)
        {
            string event_id = $"{eventKey}#{eventID}";
            lock (RealTcpIpReceivedDic)
            {
                if (event_id != null)
                {
                    if (RealTcpIpReceivedDic.ContainsKey(event_id))
                    {
                        RealTcpIpReceivedDic[event_id] += handler;
                    }
                    else
                    {
                        RealTcpIpReceivedDic.Add(event_id, handler);
                    }
                }
            }
        }

        private string getRemoteIp(ServerCallContext context)
        {
            string peer = context.Peer;
            var peer_infos = peer.Split(':');
            return peer_infos[1];
        }

        private void ongRPCReceive(string eventDevice, string eventKey, gRPCEventArgs e)
        {
            //LastCommTime = DateTime.Now;
            //StopWatch_FromTheLastCommTime.Restart();

            EventHandler<gRPCEventArgs> tmpEventHandler = null;
            lock (RealTcpIpReceivedDic)
            {
                if (RealTcpIpReceivedDic.ContainsKey(eventKey))
                {
                    tmpEventHandler = RealTcpIpReceivedDic[eventKey];
                }
            }
            if (tmpEventHandler != null)
            {
                ChangeEventStruct eventStruct = new ChangeEventStruct()
                {
                    EventDevice = eventDevice,
                    EventHandler = tmpEventHandler,
                    EventArgs = e
                };
                triggerEvent(eventStruct);
            }
            else
            {
                loggerGrpcConnction.Warn($"接收到事件:{eventKey}，但無人註冊該事件使用");
            }
        }
        private void triggerEvent(ChangeEventStruct eventStruct)
        {
            if (ProcReceiveOrder == PROCESS_RECEIVE_ORDER.Queue)
            {
                //A0.03
                //StreamFunctionProcByReceiveQueue
                //eventStruct.EventArgs.S
                string pack_id = eventStruct.EventArgs.PacketID;
                int ipack_id = 0;
                int.TryParse(pack_id, out ipack_id);
                if (ipack_id > 100 && ipack_id % 2 != 0)   //大於100且為奇數時，代表是EQ的回覆
                {
                    try
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(triggerEventHandler), eventStruct);
                    }
                    catch (Exception ex)
                    {
                        loggerGrpcConnction.Error(ex, "Exception:");
                    }
                }
                else                                    //F是奇數，表示是EQ主動發出
                {
                    if (MessageProcByReceiveQueue == null ||
                        (MessageProcByReceiveQueue != null && MessageProcByReceiveQueue.Count == 0) ||
                        (MessageProcByReceiveQueue != null && MessageProcByReceiveQueue.Contains(pack_id)))
                    {
                        //triggerEventQueue.onNotify(eventStruct);
                        var event_queue = TryGetTriggetEventQueue(eventStruct.EventDevice);
                        event_queue.onNotify(eventStruct);
                        //ThreadPool.QueueUserWorkItem(new WaitCallback(triggerEventHandler), eventStruct);
                    }
                    else
                    {
                        try
                        {
                            ThreadPool.QueueUserWorkItem(new WaitCallback(triggerEventHandler), eventStruct);
                        }
                        catch (Exception ex)
                        {
                            loggerGrpcConnction.Error(ex, "Exception:");
                        }
                    }
                }
            }
            else if (ProcReceiveOrder == PROCESS_RECEIVE_ORDER.Normal)
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(triggerEventHandler), eventStruct);
                }
                catch (Exception ex)
                {
                    loggerGrpcConnction.Error(ex, "Exception:");
                }
            }
            else
            {
                try
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(triggerEventHandler), eventStruct);
                }
                catch (Exception ex)
                {
                    loggerGrpcConnction.Error(ex, "Exception:");
                }
            }
        }
        private TriggerEventQueue TryGetTriggetEventQueue(string eventDeviceID)
        {
            return DicDeviceTriggerEventQueue.GetOrAdd(eventDeviceID, new TriggerEventQueue(eventDeviceID));
        }
        private void triggerEventHandler(Object obj)
        {
            try
            {
                ChangeEventStruct eventStruct = obj as ChangeEventStruct;
                if (eventStruct?.EventHandler != null)
                {
                    int threadID = Thread.CurrentThread.ManagedThreadId;
                    logger.Info($"[Trigger] Executing event handler. Thread:{threadID}, PacketID:{eventStruct.EventArgs.PacketID}");
                    eventStruct.EventHandler(this, eventStruct.EventArgs);
                }
            }
            catch (Exception ex)
            {
                loggerGrpcConnction.Error(ex, $"[Trigger][EXCEPTION] Event handler failure. PacketID:{(obj as ChangeEventStruct)?.EventArgs?.PacketID}");
            }
        }

        public class ChangeEventStruct
        {
            public string EventDevice { get; set; }

            public EventHandler<gRPCEventArgs> EventHandler { get; set; }
            public gRPCEventArgs EventArgs { get; set; }

        }

        private static (bool isSuccess, Google.Protobuf.Reflection.FieldDescriptor desc, IMessage entityMsg) getGoogleBeSetToOneOf(IMessage PackWrapperMsg)
        {
            var descriptor = PackWrapperMsg.Descriptor;
            var oneof = descriptor.Oneofs[0];
            var value = oneof.Accessor.GetCaseFieldDescriptor(PackWrapperMsg);
            if (value == null)
                return (false, null, null);
            var entityMsg = value.Accessor.GetValue(PackWrapperMsg) as IMessage;
            return (true, value, entityMsg);
        }
    }

    public class gRPCEventArgs
    {
        public string PacketID { get; private set; }
        public int iSeqNum { get; private set; }
        public IMessage objPacket { get; private set; }
        public DateTime EventTime { get; private set; }
        public EventWaitHandle eventWaitHandle = new ManualResetEvent(false);
        public WrapperMessage replyWrapperMessage { get; private set; }
        public gRPCEventArgs(string packetID, int _iSeqNum, IMessage _objPacket)
        {
            PacketID = packetID;
            iSeqNum = _iSeqNum;
            objPacket = _objPacket;
            EventTime = DateTime.Now;
        }
        public void setReplyWrapper(WrapperMessage wrapper)
        {
            replyWrapperMessage = wrapper;
        }

        public static gRPCEventArgs Empty
        {
            get { return new gRPCEventArgs(string.Empty, 0, null); }
        }

        public string getEventTime()
        {
            return EventTime.ToString("yyyy-MM-dd HH:mm:ss.fffff");
        }
    }
}
