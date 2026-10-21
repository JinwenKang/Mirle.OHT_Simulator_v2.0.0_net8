//*********************************************************************************
//      MiddleAgent.cs
//*********************************************************************************
// File Name: MiddleAgent.cs
// Description: MiddleAgent
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using System;
using System.Collections.Generic;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using Google.Protobuf.Collections;
using com.mirle.iibg3k0.ttc.Common.TCPIP.DecodRawData;
using System.Threading.Tasks;
using System.Reflection;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Threading;
using System.Windows.Forms;
using com.mirle.ibg3k0.sc.App;
using System.Net;
using Grpc.Core;
using com.mirle.ibg3k0.sc.Service.gRPC;
using Nest;
using System.Timers;
using com.mirle.ibg3k0.sc.Data.VO;
using static Nest.JoinField;
using NLog;
using Mirle.Agvc.Simulator.App;
using System.Diagnostics;
using System.Reflection.Emit;
using static System.Windows.Forms.AxHost;

namespace Mirle.Agvc.Simulator
{
    public class MiddleAgent : IDisposable
    {
        #region Events
        // 這些事件目前 Form1 有訂閱但 MiddleAgent 本身還沒有對應的觸發點，
        // 屬於既有功能的預留位置 (TODO: 連線狀態變化要在 gRPC 連線層觸發)
#pragma warning disable CS0067 // event is declared but never used
        public event EventHandler<string> OnTransferCancelEvent;
        public event EventHandler<string> OnTransferAbortEvent;
        public event EventHandler<string> OnConnected;
        public event EventHandler<string> OnDisConnected;
#pragma warning restore CS0067
        public event EventHandler<string> OnCmdSend;
        public event EventHandler<string> OnCmdReceive;
        #endregion
        private EnumOperaType cmdOpera = 0;
        private bool reject41 = false;
        private LoggerAgent theLoggerAgent;
        private Vehicle vehicle;
        //private ID_144_STATUS_CHANGE_REP theVehicleInfo;
        private MiddlerConfigs middlerConfigs;

        private Opera operaObject;

        //GRPC
        private Server server;
        public VehicleMessageRceive vehicleMessageRceive { get; private set; }
        public VehicleMessageSend vehicleMessageSend { get; private set; }

        public bool AutoApplyReserve { get; set; } = true;

        public Form1 AForm { get; set; } = new Form1();

        private SCApplication scApp = null;

        //public System.Timers.Timer timer;

        //public System.Timers.Timer timer_134;

        public bool dispose_flg = false;

        public System.Timers.Timer timer_test_134;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        // ===== 功能二：OHTC連線切換 - 新增成員變數 =====
        private bool isReconnecting = false;              // 是否正在重連
        private object reconnectLock = new object();      // 重連鎖
        private string currentRemoteIP;                   // 當前OHTC的IP
        private int currentRemotePort;                    // 當前OHTC的Port
        // ===== 新增結束 =====

        public MiddleAgent(MiddlerConfigs middlerConfigs)
        {
            try
            {
                bool isSuccess = true;
                this.middlerConfigs = middlerConfigs;

                theLoggerAgent = LoggerAgent.Instance;

                vehicle = new Vehicle();
                operaObject = new Opera(this);

                isSuccess = isSuccess && vehicle.Vehicle_Initialize();
                operaObject.theVehicleInfo = vehicle.Vehicle_Data;
                operaObject.theVehicleInfo.CurrentAdrID = middlerConfigs.CurrentAdrID;
                operaObject.theVehicleInfo.CurrentSecID = middlerConfigs.CurrentSecID;

                if (middlerConfigs.CurrentX != 0 || middlerConfigs.CurrentY != 0 || !string.IsNullOrWhiteSpace(middlerConfigs.CurrentAdrID))
                {
                    operaObject.theVehicleInfo.XAxis = middlerConfigs.CurrentX;
                    operaObject.theVehicleInfo.YAxis = middlerConfigs.CurrentY;
                }

                // ===== In-Section Load Assignment =====
                operaObject.InSectionLoadEnabled = middlerConfigs.InSectionLoadAssignment;
                logger.Info($"Vh:[{middlerConfigs.VhID}] [InSectionLoad] Init mode: {(middlerConfigs.InSectionLoadAssignment ? "ON" : "OFF")}");
                // ===== 新增結束 =====
                operaObject.theVehicleInfo.Angle = operaObject.getSecAngle(middlerConfigs.CurrentSecID);

                scApp = SCApplication.getInstance();
                scApp.updateVhAddress(VhID(), operaObject.theVehicleInfo.CurrentAdrID);

                // 初始化當前OHTC連線資訊
                currentRemoteIP = middlerConfigs.RemoteIp;
                currentRemotePort = middlerConfigs.RemotePort;

                Address adr = scApp.getAdrByID(middlerConfigs.CurrentAdrID);
                if (adr != null)
                {
                    if (middlerConfigs.CurrentX == 0 && middlerConfigs.CurrentY == 0)
                    {
                        operaObject.theVehicleInfo.XAxis = double.Parse(adr.POS_X);
                        operaObject.theVehicleInfo.YAxis = double.Parse(adr.POS_Y);
                    }
                }

                initialClient();
                initialServer();

                RegisteredTcpIpProcEvent();

                //timer = new System.Timers.Timer(60000);
                //timer.Elapsed += OnTimedEvent;
                //timer.AutoReset = true;
                //timer.Enabled = true;

                //timer_134 = new System.Timers.Timer(500);
                //timer_134.Elapsed += OnTimed134Event;
                //timer_134.AutoReset = true;
                //timer_134.Enabled = true;

                operaObject.Send_Cmd134_TransferEventReport();
                operaObject.Send_Cmd144_StatusChangeReport();

                initialRegularReport();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        private void initialServer()
        {
            try
            {
                // 1. 定義包含嚴格 PING 策略的 Server Options
                var serverOptions = new List<ChannelOption>
{
    //// 保持嚴格的【接收】策略
    //new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 180000), // 或 300000
    //new ChannelOption("grpc.http2.max_pings_strikes", 1),

    //// 【新增】明確禁用伺服器【發送】Keepalive PING
    //// 將 KeepaliveTimeMs 設為極大值，相當於禁用
    //new ChannelOption("grpc.keepalive_time_ms", int.MaxValue),
    //// 或者嘗試禁用 permit_without_calls (如果伺服器端支持此字串選項)
    //new ChannelOption("grpc.server.keepalive_permit_without_calls", 0), // 不確定此選項名稱是否標準


                       // --- 主要設定：強制執行客戶端 Ping 的間隔 ---

    // 1. 【關鍵】設定伺服器允許接收 Client Ping 的最小間隔時間 (單位：毫秒)
    //    這裡設定為 10 秒 (10000ms)。如果客戶端發送 Ping 的間隔短於此值
    //    (例如您設定的 5 秒)，伺服器應將其視為不當行為並可能發送 GOAWAY。
    new ChannelOption("grpc.min_recv_ping_interval_without_data_ms", 180000),

    // 2. 【建議】允許伺服器在沒有進行中的 RPC 呼叫時，也能接收並處理 Keepalive Ping
    //    這樣即使連線閒置，上述的最小間隔策略依然能生效。設為 1 (true)。
    new ChannelOption("grpc.keepalive_permit_without_calls", 1),

    // --- 輔助設定：禁用伺服器主動發送 Keepalive Ping ---
    // 這些是為了更清晰地隔離問題，專注於伺服器如何「回應」客戶端的 Ping

    // 3. 【維持】禁用伺服器基於閒置時間主動發送 Keepalive Ping
    //    將 Keepalive Time 設為極大值。
    new ChannelOption("grpc.keepalive_time_ms", int.MaxValue),

    // --- 其他可選設定 (沿用或調整) ---

    // 4. (可選) 限制伺服器發送的 Ping 在未收到 ACK 時的最大容忍次數
    //    (您之前設為 1)
    new ChannelOption("grpc.http2.max_pings_strikes", 2),

    // 5. (可選) 伺服器等待 Keepalive Ping 回應的超時時間 (單位：毫秒)
    //    當 permit_without_calls 為 1 時，這個參數對頻率限制的影響較小，
    //    但設定一個合理值（例如 20 秒）是個好習慣。
    // new ChannelOption("grpc.keepalive_timeout_ms", 20000),

    // 6. (可選) 限制伺服器主動發送 Ping 的最小間隔 (單位：毫秒)
    //    由於上面已禁用伺服器主動 Ping，此項目前影響不大。
    // new ChannelOption("grpc.http2.min_ping_interval_without_data_ms", 180000), // 3 分鐘

};

                vehicleMessageRceive = new VehicleMessageRceive();
                server = new Grpc.Core.Server(serverOptions)
                {
                    Services = { vehicleGreeter.BindService(vehicleMessageRceive) },
                    Ports = { new ServerPort(System.Net.IPAddress.Any.ToString(), middlerConfigs.LocalPort, ServerCredentials.Insecure) }
                    //Ports = { new ServerPort("0.0.0.0", middlerConfigs.LocalPort, ServerCredentials.Insecure) }
                };
                server.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        //send ip
        private void initialClient()
        {
            try
            {
                vehicleMessageSend = new VehicleMessageSend(middlerConfigs.VhID, middlerConfigs.RemoteIp, middlerConfigs.RemotePort);
                vehicleMessageSend.Start();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        //receive ip
        public void RegisteredTcpIpProcEvent()
        {
            try
            {
                //string event_key = $"{middlerConfigs.RemoteIp}#{middlerConfigs.VhID}";
                string event_key = $"{middlerConfigs.VhID}";
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.TransReqFieldNumber, Receive_Cmd31_TransferRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.TransCancelReqFieldNumber, Receive_Cmd37_TransferCancelRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.PauseReqFieldNumber, Receive_Cmd39_PauseRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.ModeChangeReqFieldNumber, Receive_Cmd41_ModeChange);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.StatusReqFieldNumber, Receive_Cmd43_StatusRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.AlarmResetReqFieldNumber, Receive_Cmd91_AlarmResetRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.TavellingDataRepFieldNumber, Receive_Cmd13_TravelingDataRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.ID15SectionDataSynchronizationRequestFieldNumber, Receive_Cmd15_StatusRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.ID17AddressDataSynchronizationRequestFieldNumber, Receive_Cmd17_StatusRequest);
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.ID19PortDataSynchronizationRequestFieldNumber, Receive_Cmd19_StatusRequest);
                // ===== 功能二：註冊99命令 =====
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.OHTCSwitchReqFieldNumber, Receive_Cmd99_OHTCSwitchRequest);
                // ===== 新增結束 =====
                // ===== ID27/127 Parameters =====
                vehicleMessageRceive.addgRPCReceivedHandler(event_key, WrapperMessage.ParametersReqFieldNumber, Receive_Cmd27_ParametersRequest);
                // ===== 新增結束 =====
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        public async void Dispose()
        {
            //timer.Stop();
            //timer= null;
            dispose_flg = true;
            await server.ShutdownAsync();
            vehicleMessageSend = null;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            //operaObject.Send_Cmd134_TransferEventReport();
            operaObject.Send_Cmd144_StatusChangeReport();
        }

        private long syncPoint = 0;
        private void OnTimed134Event(Object source, ElapsedEventArgs e)
        {
            if (System.Threading.Interlocked.Exchange(ref syncPoint, 1) == 0)
            {
                try
                {
                    operaObject.Send_Cmd134_TransferEventReport();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
                finally
                {
                    System.Threading.Interlocked.Exchange(ref syncPoint, 0);
                }
            }
        }

        private void initialRegularReport()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    // 故意 fire-and-forget：此 Task.Factory.StartNew 本身就是執行緒入口，
                    // 啟動後不需要等待結果。用 discard 標示意圖、消除 CS4014 警告。
                    _ = RegularReport();
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                }
            },
            TaskCreationOptions.LongRunning);
        }

        private async Task RegularReport()
        {
            Stopwatch sw_144 = new Stopwatch();
            Stopwatch sw_134 = new Stopwatch();
            sw_144.Restart();
            sw_134.Restart();
            while (true)
            {
                //SpinWait.SpinUntil(() => false, 100);
                await Task.Delay(100);

                if (dispose_flg)
                {
                    break;
                }

                try
                {
                    if (sw_134.ElapsedMilliseconds > com.mirle.ibg3k0.sc.App.SystemParameter.Report134Time)
                    {
                        operaObject.Send_Cmd134_TransferEventReport();
                        sw_134.Restart();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    sw_134.Restart();
                }

                try
                {
                    if (sw_144.ElapsedMilliseconds > com.mirle.ibg3k0.sc.App.SystemParameter.Report144Time)
                    {
                        operaObject.Send_Cmd144_StatusChangeReport();
                        sw_144.Restart();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex);
                    sw_144.Restart();
                }
            }
        }

        public void SetAlarmToOHBC(string alarm_code, ErrorStatus stat, ErrorLevel alarmLevel)
        {
            Send_Cmd194_AlarmReport(alarm_code, stat, alarmLevel);
        }

        public void SetStopSignal()
        {
            operaObject.theVehicleInfo.ErrorStatus = VhStopSingle.StopSingleOn;
            operaObject.SetCmd134ReportEnabled(false);

            if (operaObject.recent31Cmd != null)
            {
                // 有命令執行中：同步回報 ID_132 CmpStatusVehicleAbort，避免與下一筆 ID_31 並行造成狀態/位置上報交錯
                logger.Info($"Vh:[{VhID()}] StopSet with executing ID_31 (CmdID:[{operaObject.recent31Cmd.CmdID}]), trigger Send_Cmd132_AbnormalEndByStopSet");
                operaObject.Send_Cmd132_AbnormalEndByStopSet();
            }
            else
            {
                // 無命令：直接送 Cmd144 狀態變更
                Send_Cmd144_StatusChangeReport(0);
            }
        }

        public void ClearStopSignal()
        {
            operaObject.theVehicleInfo.ErrorStatus = VhStopSingle.StopSingleOff;
            Send_Cmd136_TransferEventInitial();
            Send_Cmd144_StatusChangeReport(0);
            operaObject.SetCmd134ReportEnabled(true, true);
        }

        public void setCstID(string cst_id)
        {
            operaObject.theVehicleInfo.CSTID = cst_id;
            operaObject.theVehicleInfo.CarCstID = cst_id;
            if (!cst_id.Trim().Equals(string.Empty))
            {
                operaObject.theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
            }
            else
            {
                operaObject.theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
            }

            operaObject.setCst(cst_id);
        }

        // ===== In-Section Load Assignment =====
        /// <summary>
        /// 即時切換 In-Section Load Assignment 開關。
        /// true：OHT 接受 NextAction.ActType=Load，將 Move 就地升級為 Load。
        /// false：OHT 靜默忽略 NextAction，繼續原 Move 命令走到終點。
        /// </summary>
        public void SetInSectionLoadEnabled(bool enabled)
        {
            operaObject.InSectionLoadEnabled = enabled;
            logger.Info($"Vh:[{VhID()}] [InSectionLoad] Mode changed to: {(enabled ? "ON" : "OFF")}");
        }
        // ===== 新增結束 =====

        public void startTimer(int tick_time)
        {
            if (timer_test_134 != null)
            {
                timer_test_134.Stop();
                timer_test_134 = null;
            }

            timer_test_134 = new System.Timers.Timer(tick_time);
            timer_test_134.Elapsed += OnTestTimedEvent;
            timer_test_134.AutoReset = true;
            timer_test_134.Start();
        }

        public void stopTimer()
        {
            if (timer_test_134 != null)
            {
                timer_test_134.Stop();
                timer_test_134 = null;
            }
        }

        private void OnTestTimedEvent(Object source, ElapsedEventArgs e)
        {
            operaObject.Send_Cmd134_TransferEventReportWithLog();
        }

        public static Google.Protobuf.IMessage unPackWrapperMsg(byte[] raw_data)
        {
            WrapperMessage WarpperMsg = ToObject<WrapperMessage>(raw_data);
            return WarpperMsg;
        }

        public static T ToObject<T>(byte[] buf) where T : Google.Protobuf.IMessage<T>, new()
        {
            if (buf == null)
                return default(T);

            Google.Protobuf.MessageParser<T> parser = new Google.Protobuf.MessageParser<T>(() => new T());
            return parser.ParseFrom(buf);
        }

        #region Receive
        // ===== 功能一：雙命令Queue機制 - 修改Receive_Cmd31 =====
        public void Receive_Cmd31_TransferRequest(object sender, gRPCEventArgs e)
        {
            ID_31_TRANS_REQUEST transRequest = (ID_31_TRANS_REQUEST)e.objPacket;
            //Need to check reply ok or no
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            
            // 檢查是否為測試拒絕模式
            if (cmdOpera == EnumOperaType.RejectCommand)
            {
                Send_Cmd131_TransferResponse(transRequest, e.iSeqNum, 1, e, "reject test");
                return;
            }

            if (operaObject.IsPauseActive())
            {
                logger.Warn($"Vh:[{VhID()}] Command Rejected, CmdID:[{transRequest.CmdID}], Reason: Vehicle is paused");
                Send_Cmd131_TransferResponse(transRequest, e.iSeqNum, 1, e, "Vehicle is paused");
                return;
            }
            
            // 使用Queue機制處理命令
            // ★ 重要：lock 內只做狀態判斷與回覆 ID_131，不執行命令本體。
            //   StartFeedBack31 是長時間同步執行（走行、Reserve等），若在 lock 內執行，
            //   Cmd_B 的 ID_31 到達時會因搶 lock 而 block，導致 gRPC timeout 無法回覆 ID_131。
            bool shouldExecute = false;
            lock (operaObject.cmdQueueLock)
            {
                CmdQueueStatus status = GetCmdQueueStatus();
                
                switch (status)
                {
                    case CmdQueueStatus.NoCommand:
                        // 先在 lock 內標記命令已佔用，避免 lock 釋放後至 StartFeedBack31 前的空窗
                        operaObject.recent31Cmd = transRequest;
                        Send_Cmd131_TransferResponse(transRequest, e.iSeqNum, 0, e);
                        shouldExecute = true;
                        break;
                        
                    case CmdQueueStatus.OneExecuting:
                        // 有一個執行中，Queue第二個命令
                        QueueCommand(transRequest, e);
                        break;
                        
                    case CmdQueueStatus.OneQueueing:
                        // 已有執行中+Queue，拒絕新命令
                        RejectCommand(transRequest, e);
                        break;
                }
            }
            // lock 外執行命令，確保 ID_31 Cmd_B 能即時取得 lock 並回覆 ID_131
            if (shouldExecute)
            {
                operaObject.StartFeedBack31(transRequest, theLoggerAgent, OnCmdSend);
            }
        }
        
        /// <summary>
        /// 取得命令Queue狀態
        /// </summary>
        private CmdQueueStatus GetCmdQueueStatus()
        {
            if (operaObject.recent31Cmd == null)
                return CmdQueueStatus.NoCommand;
            else if (operaObject.GetQueuedCommand() == null)
                return CmdQueueStatus.OneExecuting;
            else
                return CmdQueueStatus.OneQueueing;
        }
        
        /// <summary>
        /// 接受並執行命令
        /// </summary>
        private void AcceptAndExecuteCommand(ID_31_TRANS_REQUEST cmd, gRPCEventArgs e)
        {
            logger.Info($"Vh:[{VhID()}] Accept and execute command, CmdID:[{cmd.CmdID}]");
            
            // 回覆131接受
            Send_Cmd131_TransferResponse(cmd, e.iSeqNum, 0, e);
            
            // 開始執行命令
            operaObject.StartFeedBack31(cmd, theLoggerAgent, OnCmdSend);
        }
        
        /// <summary>
        /// Queue命令
        /// </summary>
        private void QueueCommand(ID_31_TRANS_REQUEST cmd, gRPCEventArgs e)
        {
            // 直接設定Queue命令（已在lock內）
            operaObject.queued31Cmd = cmd;
            
            logger.Info($"Vh:[{VhID()}] Command Queued, CmdID:[{cmd.CmdID}]");
            
            // 回覆131接受 (ReplyCode=0，SEMI規範無Queue狀態碼，非零皆視為拒絕)
            Send_Cmd131_TransferResponse(cmd, e.iSeqNum, 0, e, "Command queued");
        }
        
        /// <summary>
        /// 拒絕命令
        /// </summary>
        private void RejectCommand(ID_31_TRANS_REQUEST cmd, gRPCEventArgs e)
        {
            var executingCmd = operaObject.recent31Cmd;
            var queuedCmd = operaObject.GetQueuedCommand();
            
            logger.Warn($"Vh:[{VhID()}] Command Rejected, CmdID:[{cmd.CmdID}], " +
                       $"Reason: Already has executing command:[{executingCmd?.CmdID}] " +
                       $"and queued command:[{queuedCmd?.CmdID}]");
            
            // 回覆131拒絕
            Send_Cmd131_TransferResponse(cmd, e.iSeqNum, 1, e, "Already has executing and queued command");
        }
        // ===== 新增結束 =====

        public void Send_Cmd131_TransferResponse(ID_31_TRANS_REQUEST iD_31_TRANS_REQUEST, int seqNum, int replyCode, gRPCEventArgs e, string reason = "")
        {
            try
            {
                ID_131_TRANS_RESPONSE iD_131_TRANS_RESPONSE = new ID_131_TRANS_RESPONSE();
                iD_131_TRANS_RESPONSE.CmdID = iD_31_TRANS_REQUEST.CmdID;
                iD_131_TRANS_RESPONSE.ReplyCode = replyCode;
                iD_131_TRANS_RESPONSE.NgReason = reason;
                iD_131_TRANS_RESPONSE.ActType = iD_31_TRANS_REQUEST.ActType;
                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransResp = iD_131_TRANS_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd37_TransferCancelRequest(object sender, gRPCEventArgs e)
        {

            ID_37_TRANS_CANCEL_REQUEST receive = (ID_37_TRANS_CANCEL_REQUEST)e.objPacket;
            bool result = true;
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            int replyCode = result ? 0 : 1;
            Send_Cmd137_TransferCancelResponse(e.iSeqNum, replyCode, receive, e);
            if (!scApp.isMatche(receive.ActType, "CmdReserveRecycle"))
            {
                operaObject.StartFeedBack37(receive, theLoggerAgent, OnCmdSend);
            }
            
        }

        public void Send_Cmd137_TransferCancelResponse(int seqNum, int replyCode, ID_37_TRANS_CANCEL_REQUEST receive, gRPCEventArgs e)
        {
            try
            {
                ID_137_TRANS_CANCEL_RESPONSE iD_137_TRANS_CANCEL_RESPONSE = new ID_137_TRANS_CANCEL_RESPONSE();
                iD_137_TRANS_CANCEL_RESPONSE.ReplyCode = replyCode;
                iD_137_TRANS_CANCEL_RESPONSE.ActType = receive.ActType;
                iD_137_TRANS_CANCEL_RESPONSE.CmdID = receive.CmdID;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransCancelRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TransCancelResp = iD_137_TRANS_CANCEL_RESPONSE;
                wrappers.IpAddress = LocalIP();
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd39_PauseRequest(object sender, gRPCEventArgs e)
        {
            ID_39_PAUSE_REQUEST receive = (ID_39_PAUSE_REQUEST)e.objPacket;
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            int replyCode = 0;
            Send_Cmd139_PauseResponse(e.iSeqNum, replyCode, receive, e);

            operaObject.setPauseStat(receive);
        }

        public void Send_Cmd139_PauseResponse(int seqNum, int replyCode, ID_39_PAUSE_REQUEST receive, gRPCEventArgs e)
        {
            try
            {
                ID_139_PAUSE_RESPONSE iD_139_PAUSE_RESPONSE = new ID_139_PAUSE_RESPONSE();
                iD_139_PAUSE_RESPONSE.EventType = receive.EventType;
                iD_139_PAUSE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.PauseRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.PauseResp = iD_139_PAUSE_RESPONSE;
                wrappers.IpAddress = LocalIP();
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        public void Receive_Cmd41_ModeChange(object sender, gRPCEventArgs e)
        {
            string msg = $"[Cmd_41_RECEIVE] [PacketID = {e.PacketID}][SeqNum = {e.iSeqNum}][ObjPacket = {e.objPacket}]";
            OnCmdReceive?.Invoke(this, msg);
            // 用字串常數取代 MethodBase.GetCurrentMethod() 反射，省下每筆命令的 stack frame 查找
            theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:Receive_Cmd41_ModeChange", "Device", "CarrierID"
                , msg));


            ID_41_MODE_CHANGE_REQ receive = (ID_41_MODE_CHANGE_REQ)e.objPacket;

            int replyCode = reject41 ? 1 : 0;
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd141_ModeChangeResponse(e.iSeqNum, replyCode, e);
            
            // 如果是 Reject 41 模式，直接返回不執行後續動作
            if (reject41)
            {
                return;
            }

            //TODO: Auto/Manual
            switch (receive.OperatingVHMode)
            {
                case OperatingVHMode.OperatingAuto:
                    operaObject.theVehicleInfo.ModeStatus = VHModeStatus.AutoRemote;
                    vehicle.ChangeDataOfVehicle = operaObject.theVehicleInfo;
                    operaObject.Send_Cmd144_StatusChangeReport();
                    break;
                case OperatingVHMode.OperatingManual:
                    operaObject.theVehicleInfo.ModeStatus = VHModeStatus.Manual;
                    vehicle.ChangeDataOfVehicle = operaObject.theVehicleInfo;
                    operaObject.Send_Cmd144_StatusChangeReport();
                    break;
                case OperatingVHMode.OperatingMapReload:
                    operaObject.StartFeedBack41MapReload();
                    break;
            }
        }

        public void Send_Cmd141_ModeChangeResponse(int seqNum, int replyCode, gRPCEventArgs e)
        {
            try
            {
                ID_141_MODE_CHANGE_RESPONSE iD_141_MODE_CHANGE_RESPONSE = new ID_141_MODE_CHANGE_RESPONSE();
                iD_141_MODE_CHANGE_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ModeChangeRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ModeChangeResp = iD_141_MODE_CHANGE_RESPONSE;
                wrappers.IpAddress = LocalIP();
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        private void Receive_Cmd13_TravelingDataRequest(object sender, gRPCEventArgs e)
        {
            ID_13_TAVELLING_DATA_REP receive = (ID_13_TAVELLING_DATA_REP)e.objPacket;
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd113_TravelingDataResponse(e.iSeqNum, e);
        }
        public void Send_Cmd113_TravelingDataResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_113_TAVELLING_DATA_RESPONSE iD_113_TAVELLING_DATA_RESPONSE = new ID_113_TAVELLING_DATA_RESPONSE();
                iD_113_TAVELLING_DATA_RESPONSE.ReplyCode = 0;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TavellingDataRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.TavellingDataResp = iD_113_TAVELLING_DATA_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private void Receive_Cmd15_StatusRequest(object sender, gRPCEventArgs e)
        {
            ID_15_SECTION_DATA_SYNCHRONIZATION_REQUEST receive = (ID_15_SECTION_DATA_SYNCHRONIZATION_REQUEST)e.objPacket; // Cmd15's object is empty
            operaObject.setSyncMapInfo();
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd115_StatusResponse(e.iSeqNum, e);
        }
        public void Send_Cmd115_StatusResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_115_SECTION_DATA_RESPONSE iD_115_SECTION_DATA_RESPONSE = new ID_115_SECTION_DATA_RESPONSE();
                iD_115_SECTION_DATA_RESPONSE.ReplyCode = 0;
                iD_115_SECTION_DATA_RESPONSE.NgReason = "";

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ID115SectionDataResponseFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ID115SectionDataResponse = iD_115_SECTION_DATA_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private void Receive_Cmd17_StatusRequest(object sender, gRPCEventArgs e)
        {
            ID_17_ADDRESS_DATA_SYNCHRONIZATION_REQUEST receive = (ID_17_ADDRESS_DATA_SYNCHRONIZATION_REQUEST)e.objPacket; // Cmd17's object is empty
            operaObject.setSyncMapInfo();
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd117_StatusResponse(e.iSeqNum, e);
        }
        public void Send_Cmd117_StatusResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_117_ADDRESS_DATA_SYNCHRONIZATION_RESPONSE iD_117_ADDRESS_DATA_RESPONSE = new ID_117_ADDRESS_DATA_SYNCHRONIZATION_RESPONSE();
                iD_117_ADDRESS_DATA_RESPONSE.ReplyCode = 0;
                iD_117_ADDRESS_DATA_RESPONSE.NgReason = "";

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ID117AddressDataSynchronizationResponseFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ID117AddressDataSynchronizationResponse = iD_117_ADDRESS_DATA_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private void Receive_Cmd19_StatusRequest(object sender, gRPCEventArgs e)
        {
            ID_19_PORT_DATA_SYNCHRONIZATION_REQUEST receive = (ID_19_PORT_DATA_SYNCHRONIZATION_REQUEST)e.objPacket; // Cmd19's object is empty
            operaObject.setSyncMapInfo();
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd119_StatusResponse(e.iSeqNum, e);
        }
        public void Send_Cmd119_StatusResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_119_PORT_DATA_SYNCHRONIZATION_RESPONSE iD_119_PORT_DATA_RESPONSE = new ID_119_PORT_DATA_SYNCHRONIZATION_RESPONSE();
                iD_119_PORT_DATA_RESPONSE.ReplyCode = 0;
                iD_119_PORT_DATA_RESPONSE.NgReason = "";

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ID119PortDataSynchronizationResponseFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ID119PortDataSynchronizationResponse = iD_119_PORT_DATA_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }
        private void Receive_Cmd43_StatusRequest(object sender, gRPCEventArgs e)
        {
            ID_43_STATUS_REQUEST receive = (ID_43_STATUS_REQUEST)e.objPacket; // Cmd43's object is empty
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            Send_Cmd143_StatusResponse(e.iSeqNum, e);
        }

        public void Send_Cmd143_StatusResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_143_STATUS_RESPONSE iD_143_STATUS_RESPONSE = new ID_143_STATUS_RESPONSE();
                iD_143_STATUS_RESPONSE.ActionStatus = operaObject.theVehicleInfo.ActionStatus;
                iD_143_STATUS_RESPONSE.BlockingStatus = operaObject.theVehicleInfo.BlockingStatus;
                iD_143_STATUS_RESPONSE.CmdID = operaObject.theVehicleInfo.CmdID;
                iD_143_STATUS_RESPONSE.CSTID = operaObject.theVehicleInfo.CSTID;
                iD_143_STATUS_RESPONSE.BOXID = operaObject.theVehicleInfo.BOXID;
                iD_143_STATUS_RESPONSE.CurrentAdrID = operaObject.theVehicleInfo.CurrentAdrID;
                iD_143_STATUS_RESPONSE.CurrentSecID = operaObject.theVehicleInfo.CurrentSecID;
                iD_143_STATUS_RESPONSE.DrivingDirection = operaObject.theVehicleInfo.DrivingDirection;
                iD_143_STATUS_RESPONSE.ErrorStatus = operaObject.theVehicleInfo.ErrorStatus;
                iD_143_STATUS_RESPONSE.ModeStatus = operaObject.theVehicleInfo.ModeStatus;
                iD_143_STATUS_RESPONSE.ObstacleStatus = operaObject.theVehicleInfo.ObstacleStatus;
                iD_143_STATUS_RESPONSE.ObstDistance = operaObject.theVehicleInfo.ObstDistance;
                iD_143_STATUS_RESPONSE.ObstVehicleID = operaObject.theVehicleInfo.ObstVehicleID;
                iD_143_STATUS_RESPONSE.PauseStatus = operaObject.theVehicleInfo.PauseStatus;
                iD_143_STATUS_RESPONSE.PowerStatus = operaObject.theVehicleInfo.PowerStatus;
                iD_143_STATUS_RESPONSE.ReserveStatus = operaObject.theVehicleInfo.ReserveStatus;
                iD_143_STATUS_RESPONSE.SecDistance = operaObject.theVehicleInfo.SecDistance;
                iD_143_STATUS_RESPONSE.StoppedBlockID = operaObject.theVehicleInfo.StoppedBlockID;
                iD_143_STATUS_RESPONSE.HasCst = operaObject.theVehicleInfo.HasCst;
                iD_143_STATUS_RESPONSE.HasBox = operaObject.theVehicleInfo.HasBox;
                iD_143_STATUS_RESPONSE.CarCstID = operaObject.theVehicleInfo.CarCstID;
                iD_143_STATUS_RESPONSE.CurrentGuideSection.AddRange(operaObject.theVehicleInfo.CurrentGuideSection);
                iD_143_STATUS_RESPONSE.CurrentDestination = operaObject.theVehicleInfo.CurrentDestination;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatusReqRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.StatusReqResp = iD_143_STATUS_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }

        // ===== ID27/127 Parameters =====
        private void Receive_Cmd27_ParametersRequest(object sender, gRPCEventArgs e)
        {
            try
            {
                theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:Receive_Cmd27_ParametersRequest", "Device", "CarrierID",
                    $"[Cmd_27_RECEIVE] [PacketID = 27][SeqNum = {e.iSeqNum}]"));
                ID_27_PARAMETERS_REQUEST receive = (ID_27_PARAMETERS_REQUEST)e.objPacket;
                SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
                Send_Cmd127_ParametersResponse(e.iSeqNum, e);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:Receive_Cmd27_ParametersRequest", "Device", "CarrierID",
                    $"[Cmd_27_EXCEPTION] {ex.Message}"));
            }
        }

        public void Send_Cmd127_ParametersResponse(int seqNum, gRPCEventArgs e)
        {
            try
            {
                ID_127_PARAMETERS_RESPONSE iD_127_PARAMETERS_RESPONSE = new ID_127_PARAMETERS_RESPONSE();
                iD_127_PARAMETERS_RESPONSE.StraightSpeedSetting = operaObject.StraightSpeedSetting;
                iD_127_PARAMETERS_RESPONSE.RTrackSpeedSetting = operaObject.RTrackSpeedSetting;
                iD_127_PARAMETERS_RESPONSE.UTrackSpeedSetting = operaObject.UTrackSpeedSetting;
                iD_127_PARAMETERS_RESPONSE.STrackSpeedSetting = operaObject.STrackSpeedSetting;
                iD_127_PARAMETERS_RESPONSE.PIOOnOffState = operaObject.PioOnOffState;
                iD_127_PARAMETERS_RESPONSE.BCROnOffState = operaObject.BcrOnOffState;
                iD_127_PARAMETERS_RESPONSE.CARRIERSUPPORTTYPE = operaObject.CarrierSupportType;

                // 回報車子本身的 gRPC / MQTT endpoint 給 MCS
                var vehicleLocalIp = string.IsNullOrWhiteSpace(middlerConfigs.LocalIp) ? "127.0.0.1" : middlerConfigs.LocalIp;
                var vehicleLocalPort = middlerConfigs.LocalPort;

                iD_127_PARAMETERS_RESPONSE.OHTCGrpcEedpoint = new Endpoint
                {
                    IP = vehicleLocalIp,
                    Port = vehicleLocalPort
                };
                iD_127_PARAMETERS_RESPONSE.MQTTBrokerEedpoint = new Endpoint
                {
                    IP = vehicleLocalIp,
                    Port = vehicleLocalPort
                };

                WrapperMessage wrappers = new WrapperMessage(); 
                wrappers.ID = WrapperMessage.ParametersRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.ParametersResp = iD_127_PARAMETERS_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();

                theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:Send_Cmd127_ParametersResponse", "Device", "CarrierID",
                    $"[SEND] [ID = 127][SeqNum = {seqNum}][is_success = {is_success}]"));
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:Send_Cmd127_ParametersResponse", "Device", "CarrierID",
                    $"[Cmd_127_EXCEPTION] {ex.Message}"));
            }
        }
        // ===== 新增結束 =====

        public void Receive_Cmd91_AlarmResetRequest(object sender, gRPCEventArgs e)
        {
            ID_91_ALARM_RESET_REQUEST receive = (ID_91_ALARM_RESET_REQUEST)e.objPacket;
            //TODO: Reset alarm
            SpinWait.SpinUntil(() => false, com.mirle.ibg3k0.sc.App.SystemParameter.OHTCReplyDelayTime);
            int replyCode = 0;
            Send_Cmd191_AlarmResetResponse(e.iSeqNum, replyCode, e);
            //SpinWait.SpinUntil(() => false, 1000);
            Thread.Sleep(1000);
        }

        public void Send_Cmd191_AlarmResetResponse(int seqNum, int replyCode, gRPCEventArgs e)
        {
            try
            {
                ID_191_ALARM_RESET_RESPONSE iD_191_ALARM_RESET_RESPONSE = new ID_191_ALARM_RESET_RESPONSE();
                iD_191_ALARM_RESET_RESPONSE.ReplyCode = replyCode;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.AlarmResetRespFieldNumber;
                wrappers.SeqNum = seqNum;
                wrappers.AlarmResetResp = iD_191_ALARM_RESET_RESPONSE;
                wrappers.IpAddress = middlerConfigs.LocalIp;
                e.setReplyWrapper(wrappers);
                bool is_success = e.eventWaitHandle.Set();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }
        }
        #endregion

        #region Send
        public void Send_Cmd194_AlarmReport(string alarmCode, ErrorStatus status, ErrorLevel alarmLevel)
        {
            try
            {
                //TODO: Report alram
                ID_194_ALARM_REPORT iD_194_ALARM_REPORT = new ID_194_ALARM_REPORT();
                iD_194_ALARM_REPORT.ErrCode = alarmCode;
                iD_194_ALARM_REPORT.ErrStatus = status;
                iD_194_ALARM_REPORT.ErrorLevel = alarmLevel;
                iD_194_ALARM_REPORT.ErrDescription = "Alarm Description By OP";

                WrapperMessage wrapper = new WrapperMessage
                {
                    ID = WrapperMessage.AlarmRepFieldNumber,
                    AlarmRep = iD_194_ALARM_REPORT
                };
                var send_result = vehicleMessageSend.sendMessageUnary(wrapper);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        public void Send_Cmd122_TeachingData(string file_path)
        {
            try
            {
                List<TeachingData> teachData = scApp.ReadTeachingDataFromCsv(file_path);

                //TODO: Report alram
                ID_122_PORT_DATA_UPALOAD_REQUEST iD_194_DataUpload = new ID_122_PORT_DATA_UPALOAD_REQUEST();

                List<Port_info> portInfoLst = new List<Port_info>();
                
                // Convert TeachingData to Port_info
                foreach (var data in teachData)
                {
                    Port_info portInfo = new Port_info
                    {
                        AddressID = data.AddressID ?? "",
                        Barcode = data.Barcode ?? "",
                        GrabY = double.TryParse(data.GrabY, out double grabY) ? grabY : 0,
                        GrabT = double.TryParse(data.GrabT, out double grabT) ? grabT : 0,
                        GrabZ = double.TryParse(data.GrabZ, out double grabZ) ? grabZ : 0,
                        NEEDPIO = data.NEEDPIO == "1" || data.NEEDPIO?.ToLower() == "true",
                        PIOID = uint.TryParse(data.PIOID, out uint pioId) ? pioId : 0,
                        PIOCHANNEL = int.TryParse(data.PIOCHANNEL, out int pioChannel) ? pioChannel : 0,
                        PIOPORTNUMBER = int.TryParse(data.PIOPORTNUMBER, out int pioPortNumber) ? pioPortNumber : 0,
                        StationNumber = data.StationNumber ?? "",
                        StationSection = data.StationSection ?? "",
                        POSX = int.TryParse(data.POSX, out int posX) ? posX : 0,
                        POSY = int.TryParse(data.POSY, out int posY) ? posY : 0,
                        SECDIS = int.TryParse(data.SECDIS, out int secDis) ? secDis : 0,
                        PORTTYPE = int.TryParse(data.PORTTYPE, out int portType) ? portType : 0,
                        AREATYPE = int.TryParse(data.AREATYPE, out int areaType) ? areaType : 0,
                        TA1TIMEOUT = int.TryParse(data.TA1TIMEOUT, out int ta1Timeout) ? ta1Timeout : 0
                    };
                    portInfoLst.Add(portInfo);
                }

                iD_194_DataUpload.PortList.AddRange(portInfoLst);
                iD_194_DataUpload.PortCount = teachData.Count;

                // Add fixed PorttypeList data
                iD_194_DataUpload.PorttypeList.AddRange(Cmd122FixData());

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.ID122PortDataUploadRequest = iD_194_DataUpload;
                string log_msg = $"Vehicle No:[{this.VhID()}] Send Message:[122]";
                var send_result = vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);
            }
            catch (Exception ex)
            {
                var msg = ex.StackTrace;
            }
        }

        private List<StationPortType_info> Cmd122FixData()
        {
            var portTypeList = new List<StationPortType_info>
                {
                    new StationPortType_info {  },
                    new StationPortType_info { CargoType = 1 },
                    new StationPortType_info { CargoType = 2 },
                    new StationPortType_info { PortType = 1, GoldenPortType = 1 },
                    new StationPortType_info { PortType = 1, CargoType = 1, BarcodeOffset = -2.7, GrabOffsetY = 2.5, GrabOffsetT = -0.2, GrabOffsetZ = 62.6, GoldenPortType = 1, OHTType = 2 },
                    new StationPortType_info { PortType = 1, CargoType = 2, BarcodeOffset = -4.4, GrabOffsetY = -2.5, GrabOffsetT = -0.2, GrabOffsetZ = 54.5, GoldenPortType = 1, OHTType = 2 },
                    new StationPortType_info { PortType = 2, GoldenPortType = 2 },
                    new StationPortType_info { PortType = 2, CargoType = 1, BarcodeOffset = -2.6, GrabOffsetY = 6, GrabOffsetT = -0.5, GrabOffsetZ = 62.8, GoldenPortType = 2, OHTType = 1 },
                    new StationPortType_info { PortType = 2, CargoType = 2, BarcodeOffset = -2, GrabOffsetY = 2.8, GrabOffsetT = -0.5, GrabOffsetZ = 54.7, GoldenPortType = 2, OHTType = 1 },
                    new StationPortType_info { PortType = 3 },
                    new StationPortType_info { PortType = 3, CargoType = 1 },
                    new StationPortType_info { PortType = 3, CargoType = 2 },
                    new StationPortType_info { PortType = 4 },
                    new StationPortType_info { PortType = 4, CargoType = 1, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 137.4, SpeedPattern = 2, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 4, CargoType = 2, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 130.4, SpeedPattern = 2, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 5 },
                    new StationPortType_info { PortType = 5, CargoType = 1, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 137.4, SpeedPattern = 1, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 5, CargoType = 2, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 130.4, SpeedPattern = 1, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 6 },
                    new StationPortType_info { PortType = 6, CargoType = 1, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 137.4, SpeedPattern = 2, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 6, CargoType = 2, BarcodeOffset = -1.6, GrabOffsetY = 4.4, GrabOffsetT = -0.4, GrabOffsetZ = 130.4, SpeedPattern = 2, GoldenPortType = 5 },
                    new StationPortType_info { PortType = 7 },
                    new StationPortType_info { PortType = 7, CargoType = 1 },
                    new StationPortType_info { PortType = 7, CargoType = 2 },
                    new StationPortType_info { PortType = 8, GoldenPortType = 8 },
                    new StationPortType_info { PortType = 8, CargoType = 1, BarcodeOffset = -2.3, GrabOffsetY = 1.8, GrabOffsetT = -0.7, GrabOffsetZ = 82, SpeedPattern = 1, GoldenPortType = 8 },
                    new StationPortType_info { PortType = 8, CargoType = 2, GoldenPortType = 8 },
                    new StationPortType_info { PortType = 9 },
                    new StationPortType_info { PortType = 9, CargoType = 1 },
                    new StationPortType_info { PortType = 9, CargoType = 2 },
                    new StationPortType_info { PortType = 10 },
                    new StationPortType_info { PortType = 10, CargoType = 1 },
                    new StationPortType_info { PortType = 10, CargoType = 2 },
                    new StationPortType_info { PortType = 11 },
                    new StationPortType_info { PortType = 11, CargoType = 1 },
                    new StationPortType_info { PortType = 11, CargoType = 2 },
                    new StationPortType_info { PortType = 12 },
                    new StationPortType_info { PortType = 12, CargoType = 1 },
                    new StationPortType_info { PortType = 12, CargoType = 2 },
                    new StationPortType_info { PortType = 13 },
                    new StationPortType_info { PortType = 13, CargoType = 1 },
                    new StationPortType_info { PortType = 13, CargoType = 2 },
                    new StationPortType_info { PortType = 14 },
                    new StationPortType_info { PortType = 14, CargoType = 1 },
                    new StationPortType_info { PortType = 14, CargoType = 2 },
                    new StationPortType_info { PortType = 15 },
                    new StationPortType_info { PortType = 15, CargoType = 1 },
                    new StationPortType_info { PortType = 15, CargoType = 2 },
                    new StationPortType_info { PortType = 16 },
                    new StationPortType_info { PortType = 16, CargoType = 1 },
                    new StationPortType_info { PortType = 16, CargoType = 2 },
                    new StationPortType_info { PortType = 17 },
                    new StationPortType_info { PortType = 17, CargoType = 1 },
                    new StationPortType_info { PortType = 17, CargoType = 2 },
                    new StationPortType_info { PortType = 18 },
                    new StationPortType_info { PortType = 18, CargoType = 1 },
                    new StationPortType_info { PortType = 18, CargoType = 2 },
                    new StationPortType_info { PortType = 19 },
                    new StationPortType_info { PortType = 19, CargoType = 1 },
                    new StationPortType_info { PortType = 19, CargoType = 2 }
                };

            return portTypeList;
        }
        #endregion



        private void SendCommandWrapper(WrapperMessage wrapper)
        {
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            // 用字串常數取代 MethodBase.GetCurrentMethod() 反射
            theLoggerAgent.LogMsg("Comm", new LogFormat("Comm", "1", "MiddleAgent:SendCommandWrapper", "Device", "CarrierID"
                 , msg));

            Task.Run(() => vehicleMessageSend.sendMessageUnary(wrapper));
        }



        public void Send_Cmd144_StatusChangeReport(int sequenceNum)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_SendToOHTC = new ID_144_STATUS_CHANGE_REP();
                iD_144_SendToOHTC = vehicle.Vehicle_Data;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_SendToOHTC;
                wrappers.SeqNum = sequenceNum;
                wrappers.IpAddress = LocalIP();
                SendCommandWrapper(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                var msg = ex.StackTrace;
            }

        }

        public bool OperatypeSet(EnumOperaType operaType)
        {
            cmdOpera = operaType;
            operaObject.Selected_Operate_Type(operaType);
            return true;
        }

        public bool Reject41Set(bool isReject)
        {
            reject41 = isReject;
            return true;
        }

        public bool TeachingReplySet(bool reply)
        {
            return operaObject.SetTeachReply(reply);
        }

        public string RemoteIP()
        {
            return middlerConfigs.RemoteIp;
        }
        public string LocalIP()
        {
            return middlerConfigs.LocalIp;
        }
        public int RemotePort()
        {
            return middlerConfigs.RemotePort;
        }

        public string VhID()
        {
            return middlerConfigs.VhID;
        }

        public void SetVehicleToManual(int mode, bool sendInitialWhenAutoRemote = true)
        {
            if (mode == 0)      //Set to Manual
            {
                operaObject.theVehicleInfo.ModeStatus = VHModeStatus.Manual;
            }
            else if (mode == 1) //Set to AutoRemote
            {
                if (sendInitialWhenAutoRemote)
                {
                    Send_Cmd136_TransferEventInitial();
                }
                operaObject.theVehicleInfo.ModeStatus = VHModeStatus.AutoRemote;
            }
            Send_Cmd144_StatusChangeReport(0);
        }
        public void Send_Cmd136_TransferEventInitial()
        {
            operaObject.Send_Cmd136_TransferEventInitial();
        }
        // ===== 功能二：OHTC連線切換 - 新增方法 =====

        /// <summary>
        /// Task 3.4: 切換OHTC連線
        /// </summary>
        public async Task<bool> SwitchOHTCConnection(string newIP, int newPort)
        {
            lock (reconnectLock)
            {
                if (isReconnecting)
                {
                    logger.Warn($"Already reconnecting, skip this request. New IP:{newIP}, Port:{newPort}");
                    return false;
                }
                isReconnecting = true;
            }

            try
            {
                logger.Info($"Start switching OHTC connection from {currentRemoteIP}:{currentRemotePort} to {newIP}:{newPort}");

                // 1. 停止舊的Client
                if (vehicleMessageSend != null)
                {
                    await vehicleMessageSend.Stop();
                    logger.Info("Old gRPC client stopped");
                }

                // 2. 等待資源釋放
                await Task.Delay(1000);

                // 3. 更新配置
                currentRemoteIP = newIP;
                currentRemotePort = newPort;
                middlerConfigs.RemoteIp = newIP;
                middlerConfigs.RemotePort = newPort;

                // 4. 重新初始化Client
                vehicleMessageSend = new VehicleMessageSend(middlerConfigs.VhID, newIP, newPort);
                vehicleMessageSend.Start();
                logger.Info($"New gRPC client created and started: {newIP}:{newPort}");

                // 5. 上報狀態變化
                Send_Cmd144_StatusChangeReport(0);

                logger.Info($"OHTC connection switched successfully to {newIP}:{newPort}");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error switching OHTC connection to {newIP}:{newPort}");
                return false;
            }
            finally
            {
                lock (reconnectLock)
                {
                    isReconnecting = false;
                }
            }
        }

        /// <summary>
        /// Task 3.4: 取得當前OHTC連線資訊
        /// </summary>
        public (string ip, int port) GetCurrentOHTCConnection()
        {
            return (currentRemoteIP, currentRemotePort);
        }

        /// <summary>
        /// Task 3.5: 接收99命令 - OHTC切換請求
        /// </summary>
        public async void Receive_Cmd99_OHTCSwitchRequest(object sender, gRPCEventArgs e)
        {
            try
            {
                ID_99_OHTC_SWITCH_REQUEST cmd99 = (ID_99_OHTC_SWITCH_REQUEST)e.objPacket;
                logger.Info($"Received Cmd99 OHTC Switch Request: NewIP={cmd99.NewOHTCIP}, NewPort={cmd99.NewOHTCPort}");

                int replyCode = 1;  // 預設為失敗
                string ngReason = "";
                bool shouldSwitch = false;
                string newIP = "";
                int newPort = 0;

                // 檢查是否有命令在執行
                lock (operaObject.cmdQueueLock)
                {
                    if (operaObject.recent31Cmd != null || operaObject.queued31Cmd != null)
                    {
                        replyCode = 1;
                        ngReason = "Cannot switch OHTC while command is executing or queued";
                        logger.Warn($"Reject Cmd99: {ngReason}");
                    }
                    else
                    {
                        // 驗證IP和Port
                        if (string.IsNullOrWhiteSpace(cmd99.NewOHTCIP))
                        {
                            replyCode = 1;
                            ngReason = "Invalid IP address";
                            logger.Warn($"Reject Cmd99: {ngReason}");
                        }
                        else if (cmd99.NewOHTCPort <= 0 || cmd99.NewOHTCPort > 65535)
                        {
                            replyCode = 1;
                            ngReason = "Invalid Port number";
                            logger.Warn($"Reject Cmd99: {ngReason}");
                        }
                        else
                        {
                            // 準備執行切換
                            shouldSwitch = true;
                            newIP = cmd99.NewOHTCIP;
                            newPort = cmd99.NewOHTCPort;
                        }
                    }
                }

                // 在lock外執行切換
                if (shouldSwitch)
                {
                    bool success = await SwitchOHTCConnection(newIP, newPort);
                    if (success)
                    {
                        replyCode = 0;  // 成功
                        ngReason = "";
                        logger.Info($"Cmd99 executed successfully");
                    }
                    else
                    {
                        replyCode = 1;
                        ngReason = "Failed to switch OHTC connection";
                        logger.Error($"Cmd99 execution failed: {ngReason}");
                    }
                }

                // 回覆199
                Send199Response(replyCode, ngReason);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error handling Cmd99 OHTC Switch Request");
                Send199Response(1, $"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Task 3.5: 送出199回應
        /// </summary>
        private async void Send199Response(int replyCode, string ngReason)
        {
            try
            {
                ID_199_OHTC_SWITCH_RESPONSE cmd199 = new ID_199_OHTC_SWITCH_RESPONSE
                {
                    ReplyCode = replyCode,
                    NgReason = ngReason ?? ""
                };

                WrapperMessage wrapperMessage = new WrapperMessage
                {
                    OHTCSwitchResp = cmd199
                };

                await vehicleMessageSend.SendMessageUnaryAsync(wrapperMessage);
                logger.Info($"Sent Cmd199 OHTC Switch Response: ReplyCode={replyCode}, NgReason={ngReason}");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error sending Cmd199 OHTC Switch Response");
            }
        }

        // ===== 新增結束 =====

    }



}
