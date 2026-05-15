//*********************************************************************************
//      Opera.cs
//*********************************************************************************
// File Name: Opera.cs
// Description: Opera
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using com.mirle.ibg3k0.sc.Service.gRPC;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using Google.Protobuf.Collections;
using Grpc.Core.Logging;
using Mirle.Agvc.Simulator;
using Mirle.Agvc.Simulator.App;
using Nest;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using Google.Protobuf;
using System.Threading;
using System.Threading.Tasks;
using static com.mirle.ibg3k0.stc.Data.SecsData.Sample.S2F49_TRANSFEREXT.REPITEM.CST.CARR;

namespace Mirle.Agvc.Simulator
{
    class Opera
    {
        private EventHandler<string> OnCmdSend;
        private EnumOperaType operaType;
        private LoggerAgent theLoggerAgent;
        private CMDCancelType cmdCanceltype = CMDCancelType.CmdNone;
        public ID_144_STATUS_CHANGE_REP theVehicleInfo;
        public ID_31_TRANS_REQUEST recent31Cmd = null;
        private ID_38_GUIDE_INFO_RESPONSE recent38Guide;
        private ID_37_TRANS_CANCEL_REQUEST iD_37_TRANS_Cmd;
        private GuideData guideData;
        private MiddleAgent agent;
        private bool syncMapInfo = false;
        private bool teachReply = true;

        // ===== 功能一：雙命令Queue機制 - 新增成員變數 =====
        public ID_31_TRANS_REQUEST queued31Cmd = null;        // Queue的命令
        public object cmdQueueLock = new object();              // 命令Queue鎖
        // ===== 新增結束 =====

        // ===== In-Section Load Assignment =====
        /// <summary>
        /// true：收到 ID_36 NextAction.ActType=Load（Reserve成功）時，將 Move 就地升級為 Load。
        /// false：靜默忽略 NextAction，繼續原 Move 命令走到終點。
        /// </summary>
        public bool InSectionLoadEnabled { get; set; } = false;
        // ===== 新增結束 =====

        // ===== Section 計時 - ID_134 LastSecID / LastSecPassedTime =====
        private string lastPassedSecID = "";
        private double lastSecPassedTime = 0;
        private DateTime sectionEnterTime = DateTime.MinValue;
        // ===== 新增結束 =====

        string BlockID = string.Empty;

        SCApplication scApp = null;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private NLog.Logger logger_systemProcess = LogManager.GetLogger("SystemProcess");
        private NLog.Logger logger_error = LogManager.GetLogger("HasError");
        private NLog.Logger logger_roadcontrol = LogManager.GetLogger("RoadControl");

        private int CurrentAct = SCAppConstants.CurrentAction.Initial;

        object sendRecv_LockObj = new object();
        public Opera(MiddleAgent agent)
        {
            scApp = SCApplication.getInstance();
            this.agent = agent;
        }
        public bool Selected_Operate_Type(EnumOperaType selectedOpera)
        {
            bool isSuccess = true;
            operaType = selectedOpera;
            isSuccess = isSuccess && operaType == selectedOpera;
            return isSuccess;
        }

        public bool SetTeachReply(bool reply)
        {
            bool isSuccess = true;
            teachReply = reply;
            isSuccess = isSuccess && teachReply == reply;
            return isSuccess;
        }

        #region Receive
        public bool StartFeedBack31(ID_31_TRANS_REQUEST transRequest,
                                    LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                scApp = SCApplication.getInstance();

                cmdCanceltype = CMDCancelType.CmdNone;
                CurrentAct = SCAppConstants.CurrentAction.None;
                recent31Cmd = transRequest;
                WaitForOneSecond();
                SetBasicInform(theLoggerAgent, OnCmdSend);

                SetNextAction(null);

                return true;
            }
            catch(Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }
        }

        public bool StartFeedBack37(ID_37_TRANS_CANCEL_REQUEST transRequest,
                                    LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                iD_37_TRANS_Cmd = Deep_Clone<ID_37_TRANS_CANCEL_REQUEST>(transRequest);
                
                // ===== 功能一：支援取消Queue命令 =====
                lock (cmdQueueLock)
                {
                    // 如果取消的是執行中命令
                    if (recent31Cmd != null && recent31Cmd.CmdID == transRequest.CmdID)
                    {
                        logger.Info($"Vh:[{agent.VhID()}] Cancel executing command, CmdID:[{transRequest.CmdID}]");
                        
                        switch (iD_37_TRANS_Cmd.ActType)
                        {
                            case CMDCancelType.CmdCancel:
                                cmdCanceltype = CMDCancelType.CmdCancel;
                                break;
                            case CMDCancelType.CmdAbort:
                                cmdCanceltype = CMDCancelType.CmdAbort;
                                break;
                        }
                    }
                    // 如果取消的是Queue命令
                    else if (queued31Cmd != null && queued31Cmd.CmdID == transRequest.CmdID)
                    {
                        logger.Info($"Vh:[{agent.VhID()}] Cancel queued command, CmdID:[{queued31Cmd.CmdID}]");
                        queued31Cmd = null;
                        // 回覆137取消成功（在MiddleAgent中實作回覆邏輯）
                    }
                    else
                    {
                        // 命令不存在，回覆失敗
                        logger.Warn($"Vh:[{agent.VhID()}] Cancel command not found, CmdID:[{transRequest.CmdID}]");
                    }
                }
                // ===== 新增結束 =====
                
                WaitForOneSecond();
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }
        }

        public bool StartFeedBack41MapReload()
        {
            try
            {
                //沒有同步圖資
                if(!syncMapInfo)
                {
                    //回覆NG
                    Send_Cmd142_ModeChangeComplete(false);
                }
                //有同步圖資
                else
                {
                    SpinWait.SpinUntil(() => false, 1000);

                    //OHT切換到Manual
                    theVehicleInfo.ModeStatus = VHModeStatus.Manual;
                    Send_Cmd144_StatusChangeReport();

                    SpinWait.SpinUntil(() => false, 1000);

                    //回覆同步圖資是否成功
                    if (teachReply)
                    {
                        Send_Cmd142_ModeChangeComplete(true);
                    }
                    else
                    {
                        Send_Cmd142_ModeChangeComplete(false);
                    }


                    //OHT切換到AutoRemote
                    Send_Cmd136_TransferEventInitial();
                    SpinWait.SpinUntil(() => false, 1000);
                    theVehicleInfo.ModeStatus = VHModeStatus.AutoRemote;
                    Send_Cmd144_StatusChangeReport();
                }

                syncMapInfo = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return false;
            }

            return true;
        }
        #endregion

        #region Send
        private (bool is_success, WrapperMessage rtnmsg) sendRecv(WrapperMessage wrapper)
        {
            lock (sendRecv_LockObj)
            {
                (bool is_success, WrapperMessage rtnmsg) returncode = (false, null);
                bool retrySendRecv = true;
                while (retrySendRecv == true)
                {
                    returncode = agent.vehicleMessageSend.sendMessageUnary(wrapper);
                    retrySendRecv = false;
                }
                return returncode;
            }
        }

        private (bool is_success, WrapperMessage rtnmsg) sendRecvWithLog(WrapperMessage wrapper, string log_msg)
        {
            lock (sendRecv_LockObj)
            {
                (bool is_success, WrapperMessage rtnmsg) returncode = (false, null);
                bool retrySendRecv = true;
                int retryCount = 0;
                while (retrySendRecv == true)
                {
                    returncode = agent.vehicleMessageSend.sendMessageUnaryNew(wrapper, log_msg);
                    if(returncode.is_success)
                    {
                        retrySendRecv = false;
                    }
                    else
                    {
                        retryCount++;
                        if (retryCount >= 10)
                        {
                            retrySendRecv = false;
                        }
                    }
                }
                return returncode;
            }
        }

        private (bool is_success, WrapperMessage rtnmsg) sendRecvWithLogNoRetry(WrapperMessage wrapper, string log_msg)
        {
            lock (sendRecv_LockObj)
            {
                (bool is_success, WrapperMessage rtnmsg) returncode = (false, null);
                bool retrySendRecv = true;
                while (retrySendRecv == true)
                {
                    returncode = agent.vehicleMessageSend.sendMessageUnaryNoRetry(wrapper, log_msg);
                    retrySendRecv = false;
                }
                return returncode;
            }
        }

        public bool Send_Cmd138_RequestGuideData(string from, string to, out ID_38_GUIDE_INFO_RESPONSE receive_gpp)
        {
            receive_gpp = null;

            try
            {
                ID_138_GUIDE_INFO_REQUEST iD_138_SendToOHTC = new ID_138_GUIDE_INFO_REQUEST();
                FromToAdr fromTo = new FromToAdr();
                fromTo.From = from;
                fromTo.To = to;

                iD_138_SendToOHTC.FromToAdrList.Add(fromTo);

                WrapperMessage wrapper = new WrapperMessage
                {
                    ID = WrapperMessage.TransReqFieldNumber,
                    GuideInfoReq = iD_138_SendToOHTC
                };
                var send_result = agent.vehicleMessageSend.sendMessageUnary(wrapper);
                receive_gpp = send_result.message.GuideInfoResp;

                return send_result.isSuccess;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return false;
        }

        public void Send_Cmd142_ModeChangeComplete(bool reply)
        {
            try
            {
                ID_142_MODE_CHANGE_COMPLETE_REPORT iD_142_ChangeComplete = new ID_142_MODE_CHANGE_COMPLETE_REPORT();
                iD_142_ChangeComplete.OperatingVHMode = OperatingVHMode.OperatingMapReload;

                if (teachReply)
                {
                    iD_142_ChangeComplete.ResultCode = 0;
                }
                else
                {
                    iD_142_ChangeComplete.ResultCode = 1;
                    iD_142_ChangeComplete.NgReason = "Test";
                }
                
                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ModeChangeCompleteReqFieldNumber;
                wrappers.ModeChangeCompleteReq = iD_142_ChangeComplete;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[142]";
                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private int resetCount = 0;
        public void Send_Cmd144_StatusChangeReport()
        {
            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_SendToOHTC = new ID_144_STATUS_CHANGE_REP();
                iD_144_SendToOHTC = Deep_Clone<ID_144_STATUS_CHANGE_REP>(theVehicleInfo);
                if (theVehicleInfo.HasCst == VhLoadCarrierStatus.Exist && scApp.isEmpty(theVehicleInfo.CSTID))
                {
                    if (resetCount < 3)
                    {
                        resetCount++;
                    }
                    else
                    {
                        resetCount = 0;
                        theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
                        iD_144_SendToOHTC.HasCst = VhLoadCarrierStatus.NotExist;
                    }
                }
                else
                {
                    if (resetCount != 0)
                    {
                        resetCount = 0;
                    }
                }

                if (theVehicleInfo.HasBox == VhLoadCarrierStatus.Exist && scApp.isEmpty(theVehicleInfo.BOXID))
                {
                    if (resetCount < 3)
                    {
                        resetCount++;
                    }
                    else
                    {
                        resetCount = 0;
                        theVehicleInfo.HasBox = VhLoadCarrierStatus.NotExist;
                        iD_144_SendToOHTC.HasBox = VhLoadCarrierStatus.NotExist;
                    }
                }
                else
                {
                    if (resetCount != 0)
                    {
                        resetCount = 0;
                    }
                }

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_SendToOHTC;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[144]";
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);
                //var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd144_StatusChangeReport(bool has_obstacle, bool reserve_result)
        {
            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_SendToOHTC = new ID_144_STATUS_CHANGE_REP();
                iD_144_SendToOHTC = Deep_Clone<ID_144_STATUS_CHANGE_REP>(theVehicleInfo);
                if (theVehicleInfo.HasCst == VhLoadCarrierStatus.Exist && scApp.isEmpty(theVehicleInfo.CSTID))
                {
                    if (resetCount < 3)
                    {
                        resetCount++;
                    }
                    else
                    {
                        resetCount = 0;
                        theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
                        iD_144_SendToOHTC.HasCst = VhLoadCarrierStatus.NotExist;
                    }
                }
                else
                {
                    if (resetCount != 0)
                    {
                        resetCount = 0;
                    }
                }

                if (theVehicleInfo.HasBox == VhLoadCarrierStatus.Exist && scApp.isEmpty(theVehicleInfo.BOXID))
                {
                    if (resetCount < 3)
                    {
                        resetCount++;
                    }
                    else
                    {
                        resetCount = 0;
                        theVehicleInfo.HasBox = VhLoadCarrierStatus.NotExist;
                        iD_144_SendToOHTC.HasBox = VhLoadCarrierStatus.NotExist;
                    }
                }
                else
                {
                    if (resetCount != 0)
                    {
                        resetCount = 0;
                    }
                }

                if (has_obstacle)
                {
                    iD_144_SendToOHTC.ObstacleStatus = VhStopSingle.StopSingleOn;
                }
                else
                {
                    iD_144_SendToOHTC.ObstacleStatus = VhStopSingle.StopSingleOff;
                }

                if (reserve_result)
                {
                    iD_144_SendToOHTC.ReserveStatus = VhStopSingle.StopSingleOff;
                }
                else
                {
                    iD_144_SendToOHTC.ReserveStatus = VhStopSingle.StopSingleOn;
                }

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_SendToOHTC;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[144]";
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);
                //var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd134_TransferEventReport(object obj)
        {
            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_134_TRANS_EVENT_REP.Speed = theVehicleInfo.Speed;
                iD_134_TRANS_EVENT_REP.XAxis = theVehicleInfo.XAxis;
                iD_134_TRANS_EVENT_REP.YAxis = theVehicleInfo.YAxis;
                iD_134_TRANS_EVENT_REP.Angle = theVehicleInfo.Angle;
                // ===== Section 計時 =====
                iD_134_TRANS_EVENT_REP.LastSecID = lastPassedSecID;
                iD_134_TRANS_EVENT_REP.LastSecPassedTime = lastSecPassedTime;
                // ===== 結束 =====

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[134]";
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);

                //var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd134_TransferEventReport()
        {
            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                var port = scApp.PortMapDao.loadPortMapByID(theVehicleInfo.CurrentAdrID);
                if (port != null)
                {
                    iD_134_TRANS_EVENT_REP.CurrentAdrID = port.StationAddress;
                }
                else
                {
                    iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                }
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_134_TRANS_EVENT_REP.Speed = theVehicleInfo.Speed;
                iD_134_TRANS_EVENT_REP.XAxis = theVehicleInfo.XAxis;
                iD_134_TRANS_EVENT_REP.YAxis = theVehicleInfo.YAxis;
                iD_134_TRANS_EVENT_REP.Angle = theVehicleInfo.Angle;
                // ===== Section 計時 =====
                iD_134_TRANS_EVENT_REP.LastSecID = lastPassedSecID;
                iD_134_TRANS_EVENT_REP.LastSecPassedTime = lastSecPassedTime;
                // ===== 結束 =====

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[134]";
                //if (theVehicleInfo.CurrentSecID != "48403")
                //{
                //    var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);
                //}
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);

                //var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd134_TransferEventReportWithLog()
        {
            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_134_TRANS_EVENT_REP.Speed = theVehicleInfo.Speed;
                iD_134_TRANS_EVENT_REP.XAxis = theVehicleInfo.XAxis;
                iD_134_TRANS_EVENT_REP.YAxis = theVehicleInfo.YAxis;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[134]";

                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLogNoWait(wrappers, log_msg);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd134_TransferEventReport(string sectionAddressCount_Section)
        {
            try
            {
                //while (!scApp.checkVhAddress(agent.VhNo(), sectionAddressCount_Section))
                //{
                //    SpinWait.SpinUntil(() => false, 2000);                    
                //}

                //scApp.updateVhAddress(agent.VhNo(), sectionAddressCount_Section);

                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_134_TRANS_EVENT_REP.Speed = theVehicleInfo.Speed;
                iD_134_TRANS_EVENT_REP.XAxis = theVehicleInfo.XAxis;
                iD_134_TRANS_EVENT_REP.YAxis = theVehicleInfo.YAxis;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void Send_Cmd134_TransferEventReport(int distance)
        {
            try
            {
                ID_134_TRANS_EVENT_REP iD_134_TRANS_EVENT_REP = new ID_134_TRANS_EVENT_REP();
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass;
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = (uint)distance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                var send_result = agent.vehicleMessageSend.sendMessageUnary(wrappers);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventReport(EventType eventType, bool isCmd = false, BCRReadResult bCRReadResult = BCRReadResult.BcrNormal)
        {
            try
            {
                string loadPortId = "";
                string unloadPortId = "";
                if (isCmd == false)
                {
                    // do nothing
                }
                else if (isCmd == true)
                {
                    loadPortId = recent31Cmd.LoadPortID;
                    unloadPortId = recent31Cmd.UnloadPortID;
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();
                Set136Inform(eventType, bCRReadResult, loadPortId, unloadPortId, iD_136_TRANS_EVENT_REP);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);

                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
                //if (iD_36_TRANS_EVENT_RESPONSE.ReplyActiveType != CMDCancelType.CmdNone)
                //{
                //    switch (iD_36_TRANS_EVENT_RESPONSE.ReplyActiveType)
                //    {
                //        case CMDCancelType.CmdCancelIdReadFailed:
                //            Send_Cmd132_TransferCompleteReport(recent31Cmd);
                //            break;
                //        case CMDCancelType.CmdCancelIdMismatch:
                //            Send_Cmd132_TransferCompleteReport(recent31Cmd);
                //            break;
                //        case CMDCancelType.CmdCancelIdReadDuplicate:

                //            break;
                //        case CMDCancelType.CmdCancelIdReadForceFinish:

                //            break;
                //        case CMDCancelType.CmdAbort:

                //            break;
                //        case CMDCancelType.CmdCancel:

                //            break;
                //        case CMDCancelType.CmdRetry:
                //            //recent36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(iD_36_TRANS_EVENT_RESPONSE);
                //            //recent36Response = iD_36_TRANS_EVENT_RESPONSE;
                //            break;
                //        case CMDCancelType.CmdNone:
                //            //recent36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(iD_36_TRANS_EVENT_RESPONSE);
                //            //recent36Response = iD_36_TRANS_EVENT_RESPONSE;
                //            break;
                //    }
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }

        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventReport(EventType eventType, string loadPortId, string unloadPortId, bool isCmd = false, BCRReadResult bCRReadResult = BCRReadResult.BcrNormal)
        {
            try
            {
                if (isCmd == false)
                {
                    loadPortId = "";
                    unloadPortId = "";
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();
                Set136Inform(eventType, bCRReadResult, loadPortId, unloadPortId, iD_136_TRANS_EVENT_REP);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);

                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }

        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventReport_Scan(EventType eventType, bool isCmd = false, BCRReadResult bCRReadResult = BCRReadResult.BcrNormal)
        {
            try
            {
                string loadPortId = "";
                string unloadPortId = "";
                if (isCmd == false)
                {
                    // do nothing
                }
                else if (isCmd == true)
                {
                    loadPortId = recent31Cmd.LoadPortID;
                    unloadPortId = recent31Cmd.UnloadPortID;
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.BOXID = Global_Param.MismatchID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_136_TRANS_EVENT_REP.CSTID = Global_Param.MismatchID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

                iD_136_TRANS_EVENT_REP.BCRReadResult = bCRReadResult;
                iD_136_TRANS_EVENT_REP.LoadPortID = loadPortId;
                iD_136_TRANS_EVENT_REP.UnloadPortID = unloadPortId;
                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);

                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }

        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventReserveReport(EventType eventType, string[] reserveSections, DriveDirction[] reserveDirections, bool isCmd = false,
                                ID_31_TRANS_REQUEST now31Cmd = null)
        {
            try
            {
                string loadPortId = "";
                string unloadPortId = "";
                if (isCmd == false)
                {
                    // do nothing
                }
                else if (isCmd == true)
                {
                    loadPortId = now31Cmd.LoadPortID;
                    unloadPortId = now31Cmd.UnloadPortID;
                }
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.BOXID = theVehicleInfo.BOXID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

                iD_136_TRANS_EVENT_REP.LoadPortID = loadPortId;
                iD_136_TRANS_EVENT_REP.UnloadPortID = unloadPortId;

                GetReserveInfo(reserveSections, reserveDirections, iD_136_TRANS_EVENT_REP.ReserveInfos);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLogNoRetry(wrappers, log_msg);
                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }

        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventBlockReserveReport(EventType eventType, string block_id, ID_31_TRANS_REQUEST now31Cmd = null)
        {
            try
            {
                //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.BOXID = theVehicleInfo.BOXID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;
                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_136_TRANS_EVENT_REP.RequestBlockID = block_id;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLogNoRetry(wrappers, log_msg);
                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }
        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventInitial()
        {
            try
            {
                //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = EventType.Initial;
                iD_136_TRANS_EVENT_REP.BOXID = theVehicleInfo.BOXID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;
                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLogNoRetry(wrappers, log_msg);
                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }
        private ID_36_TRANS_EVENT_RESPONSE Send_Cmd136_TransferEventReleaseBlockReport(EventType eventType, string address, ID_31_TRANS_REQUEST now31Cmd = null)
        {
            try
            {
                //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP = new ID_136_TRANS_EVENT_REP();
                ID_36_TRANS_EVENT_RESPONSE iD_36_TRANS_EVENT_RESPONSE = new ID_36_TRANS_EVENT_RESPONSE();

                iD_136_TRANS_EVENT_REP.EventType = eventType;
                iD_136_TRANS_EVENT_REP.BOXID = theVehicleInfo.BOXID;
                iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;
                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_136_TRANS_EVENT_REP.ReleaseBlockAdrID = address;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.ImpTransEventRepFieldNumber;
                wrappers.ImpTransEventRep = iD_136_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[136]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);
                if (!send_result.isSuccess)
                {
                    iD_36_TRANS_EVENT_RESPONSE = null;
                }
                else
                {
                    iD_36_TRANS_EVENT_RESPONSE = send_result.message.ImpTransEventResp;
                }

                return iD_36_TRANS_EVENT_RESPONSE;
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return null;
        }

        private void GetReserveInfo(string[] reserveSections, DriveDirction[] reserveDirections, RepeatedField<ReserveInfo> reserveInfos)
        {
            if (reserveSections.Length > 0)
            {
                for (int i = 0; i < reserveSections.Length; i++)
                {
                    ReserveInfo reserveInfo = new ReserveInfo();
                    reserveInfo.ReserveSectionID = reserveSections[i];
                    reserveInfo.DriveDirction = reserveDirections[i];

                    reserveInfos.Add(reserveInfo);
                }
            }
        }

        private void Set136Inform(EventType eventType, BCRReadResult bCRReadResult, string loadPortId, string unloadPortId, ID_136_TRANS_EVENT_REP iD_136_TRANS_EVENT_REP)
        {
            string Mismatch_ID = Global_Param.MismatchID;
            string BOXID = "";
            string CSTID = "";
            switch (bCRReadResult)
            {
                case BCRReadResult.BcrMisMatch:
                    BOXID = Mismatch_ID;
                    CSTID = Mismatch_ID;
                    break;
                case BCRReadResult.BcrReadFail:
                    BOXID = "";
                    break;
                case BCRReadResult.BcrNormal:
                    BOXID = theVehicleInfo.BOXID;
                    CSTID = theVehicleInfo.CSTID;
                    break;
            }
            iD_136_TRANS_EVENT_REP.EventType = eventType;
            iD_136_TRANS_EVENT_REP.BOXID = BOXID;
            iD_136_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
            iD_136_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
            iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
            iD_136_TRANS_EVENT_REP.CSTID = CSTID;
            iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

            iD_136_TRANS_EVENT_REP.BCRReadResult = bCRReadResult;
            iD_136_TRANS_EVENT_REP.LoadPortID = loadPortId;
            iD_136_TRANS_EVENT_REP.UnloadPortID = unloadPortId;
        }

        private void Send_Cmd132_TransferCompleteReport(CMDCancelType cMDCancelType = CMDCancelType.CmdNone)
        {
            try
            {
                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                ID_32_TRANS_COMPLETE_RESPONSE iD_32_TRANS_COMPLETE_RESPONSE = new ID_32_TRANS_COMPLETE_RESPONSE();
                if (operaType == EnumOperaType.InterlockError)
                {
                    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoadInterlockError;

                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMove;
                            break;
                        case ActiveType.Unload:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusUnload;
                            setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                        case ActiveType.Scan:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                            setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                    }
                }
                else if (operaType == EnumOperaType.InterlockErrorUnload)
                {
                    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusUnloadInterlockError;

                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMove;
                            break;
                        case ActiveType.Load:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoad;
                            setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                        case ActiveType.Scan:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                            setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                    }
                }
                else if (operaType == EnumOperaType.MTL)
                {
                    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMoveToMtl;
                }
                else if (operaType == EnumOperaType.SystemOut)
                {
                    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusSystemOut;
                }
                else if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
                {
                    if (cmdCanceltype == CMDCancelType.CmdCancel)
                    {
                        iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusCancel;
                    }
                    if (cmdCanceltype == CMDCancelType.CmdAbort)
                    {
                        iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusAbort;
                    }
                    if (theVehicleInfo.HasCst == VhLoadCarrierStatus.Exist)
                    {
                        setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                    }
                    else
                    {
                        setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                    }
                }
                else
                {
                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMove;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Load:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoad;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusInterlockError;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusInterlockError;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadForceFinish)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIddoubleStorage;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            break;
                        case ActiveType.Unload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusUnload;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Loadunload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoadunload;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadForceFinish)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIddoubleStorage;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            //else if (cMDCancelType == CMDCancelType.CmdCancelEmptyRetrieval)
                            //{
                            //    //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                            //    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdemptyRetrival;
                            //    setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            //    iD_132_TRANS_COMPLETE_REPORT.CarBoxID = "";
                            //}
                            break;
                        case ActiveType.Scan:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                    }
                }
                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = theVehicleInfo.SecDistance;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = 0;
                iD_132_TRANS_COMPLETE_REPORT.CmdID = theVehicleInfo.CmdID;
                iD_132_TRANS_COMPLETE_REPORT.BOXID = theVehicleInfo.BOXID;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = theVehicleInfo.CSTID;
                iD_132_TRANS_COMPLETE_REPORT.BOXID = theVehicleInfo.CSTID;
                iD_132_TRANS_COMPLETE_REPORT.LOTID = theVehicleInfo.LOTID;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[132]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void Send_Cmd132_TransferCompleteReport()
        {
            try
            {
                ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT = new ID_132_TRANS_COMPLETE_REPORT();
                ID_32_TRANS_COMPLETE_RESPONSE iD_32_TRANS_COMPLETE_RESPONSE = new ID_32_TRANS_COMPLETE_RESPONSE();

                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusVehicleAbort;
                setCstBox132(iD_132_TRANS_COMPLETE_REPORT);

                iD_132_TRANS_COMPLETE_REPORT.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_132_TRANS_COMPLETE_REPORT.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_132_TRANS_COMPLETE_REPORT.SecDistance = theVehicleInfo.SecDistance;
                iD_132_TRANS_COMPLETE_REPORT.CmdDistance = 0;
                iD_132_TRANS_COMPLETE_REPORT.CmdID = theVehicleInfo.CmdID;
                iD_132_TRANS_COMPLETE_REPORT.BOXID = theVehicleInfo.BOXID;
                iD_132_TRANS_COMPLETE_REPORT.CSTID = theVehicleInfo.CSTID;
                iD_132_TRANS_COMPLETE_REPORT.BOXID = theVehicleInfo.CSTID;
                iD_132_TRANS_COMPLETE_REPORT.LOTID = theVehicleInfo.LOTID;


                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TranCmpRepFieldNumber;
                wrappers.TranCmpRep = iD_132_TRANS_COMPLETE_REPORT;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[132]";

                var send_result = SendCommandWrapperWithLog(wrappers, log_msg);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void setCstBox132(ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT)
        {
            iD_132_TRANS_COMPLETE_REPORT.HasBox = theVehicleInfo.HasBox;
            iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.BOXID;
            iD_132_TRANS_COMPLETE_REPORT.HasCst = theVehicleInfo.HasCst;
            iD_132_TRANS_COMPLETE_REPORT.CarCstID = theVehicleInfo.CSTID;
        }
        #endregion

        #region Process Function
        private void SetNextAction(object obj)
        {
            try
            {
                bool is_routeChange = false;

                if (isCancelAbort())  //收到Cancel或Abort，結束命令
                {
                    SetNoCommandStat();
                    return;
                }

                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Move:
                        MoveFunction(ref is_routeChange);
                        break;
                    case ActiveType.Movetomtl:
                    case ActiveType.Systemout:
                        MtlFunction(ref is_routeChange);
                        break;
                    case ActiveType.Load:
                        LoadFunction(ref is_routeChange);
                        break;
                    case ActiveType.Unload:
                        UnloadFunction(ref is_routeChange);
                        break;
                    case ActiveType.Loadunload:
                        LoadUnloadFunction(ref is_routeChange);
                        break;
                    case ActiveType.Scan:
                        ScanFunction(ref is_routeChange);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void MoveFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.Move;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Move;
                data.Target = recent31Cmd.ToAdr;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                switch (operaType)
                {
                    case EnumOperaType.CancelComplete:
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    default:
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Move)
            {
                SetNoCommandStat();
            }
        }

        private void MtlFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.Move;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Move;
                data.Target = recent31Cmd.ToAdr;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                //SpinWait.SpinUntil(() => false, 1000);
                Thread.Sleep(1000);
                if (recent31Cmd.ActType == ActiveType.Movetomtl)
                {
                    theVehicleInfo.ModeStatus = VHModeStatus.AutoMtl;
                    Send_Cmd144_StatusChangeReport();
                }

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Move)
            {
                SetNoCommandStat();
            }
        }

        private void LoadFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.MoveToLoad;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Load;
                data.Target = recent31Cmd.LoadPortID;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                switch (operaType)
                {
                    case EnumOperaType.CancelComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Load;
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    default:
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.MoveToLoad)
            {
                CurrentAct = SCAppConstants.CurrentAction.Load;

                Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadArrivalToLoadingTime);
                Thread.Sleep(SystemParameter.LoadArrivalToLoadingTime);
                Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadingToLoadCompleteTime);
                Thread.Sleep(SystemParameter.LoadingToLoadCompleteTime);

                string cst_id = recent31Cmd.CSTID;
                string box_id = recent31Cmd.BOXID;
                switch (operaType)
                {
                    case EnumOperaType.Abnormal_BcrReadFail:
                        CurrentAct = SCAppConstants.CurrentAction.Load;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);
                        ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
                        if (the36Response.RenameBOXID != "")
                        {
                            cst_id = string.Empty;
                            box_id = string.Empty;
                            if(!scApp.isEmpty(recent31Cmd.CSTID))
                            {
                                cst_id = the36Response.RenameBOXID;
                            }

                            if (!scApp.isEmpty(recent31Cmd.BOXID))
                            {
                                box_id = the36Response.RenameBOXID;
                            }

                            setCstStat(true, cst_id, box_id);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        CurrentAct = SCAppConstants.CurrentAction.Load;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        if (!scApp.isEmpty(recent31Cmd.CSTID))
                        {
                            cst_id = "Test1";
                        }

                        if (!scApp.isEmpty(recent31Cmd.BOXID))
                        {
                            box_id = "Test1";
                        }

                        setCstStat(true, cst_id, box_id);

                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrMisMatch);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        CurrentAct = SCAppConstants.CurrentAction.Load;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        CurrentAct = SCAppConstants.CurrentAction.Load;

                        Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.InterlockError:
                        CurrentAct = SCAppConstants.CurrentAction.Load;
                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Load)
            {
                switch (operaType)
                {
                    case EnumOperaType.Abnormal_BcrReadFail:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadFailed);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdMismatch);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadDuplicate);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        SetNoCommandStat(CMDCancelType.CmdCancel);
                        break;
                    default:
                        SetNoCommandStat();
                        break;
                }
            }
        }

        private void UnloadFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);

                switch (operaType)
                {
                    case EnumOperaType.CancelComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.AbortComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    default:
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.MoveToUnload;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Unload;
                data.Target = recent31Cmd.UnloadPortID;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.MoveToUnload)
            {
                CurrentAct = SCAppConstants.CurrentAction.Unload;

                Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.UnloadArrivalToUnloadingTime);
                Thread.Sleep(SystemParameter.UnloadArrivalToUnloadingTime);
                Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.UnloadingToUnloadCompleteTime);
                Thread.Sleep(SystemParameter.UnloadingToUnloadCompleteTime);

                switch (operaType)
                {
                    case EnumOperaType.Abnormal_DoubleStorage:
                        Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.InterlockErrorUnload:
                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(false, string.Empty, string.Empty);
                        Send_Cmd144_StatusChangeReport();
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Unload)
            {
                switch (operaType)
                {
                    case EnumOperaType.Abnormal_DoubleStorage:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadForceFinish);
                        break;
                    default:
                        SetNoCommandStat();
                        break;
                }
            }
        }

        private void LoadUnloadFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);

                switch (operaType)
                {
                    case EnumOperaType.CancelComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    default:
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.MoveToLoad;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Load;
                data.Target = recent31Cmd.LoadPortID;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.MoveToLoad)
            {
                CurrentAct = SCAppConstants.CurrentAction.Load;

                Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadArrivalToLoadingTime);
                Thread.Sleep(SystemParameter.LoadArrivalToLoadingTime);
                Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadingToLoadCompleteTime);
                Thread.Sleep(SystemParameter.LoadingToLoadCompleteTime);

                string cst_id = recent31Cmd.CSTID;
                string box_id = recent31Cmd.BOXID;
                switch (operaType)
                {
                    case EnumOperaType.AbortComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);

                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrReadFail:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);
                        ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
                        if (the36Response.RenameBOXID != "")
                        {
                            if (!scApp.isEmpty(recent31Cmd.CSTID))
                            {
                                cst_id = the36Response.RenameBOXID;
                            }

                            if (!scApp.isEmpty(recent31Cmd.BOXID))
                            {
                                box_id = the36Response.RenameBOXID;
                            }

                            setCstStat(true, cst_id, box_id);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        if (!scApp.isEmpty(recent31Cmd.CSTID))
                        {
                            cst_id = "Test1";
                        }

                        if (!scApp.isEmpty(recent31Cmd.BOXID))
                        {
                            box_id = "Test1";
                        }

                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrMisMatch);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.InterlockError:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Load)
            {
                CurrentAct = SCAppConstants.CurrentAction.MoveToUnload;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Unload;
                data.Target = recent31Cmd.UnloadPortID;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.MoveToUnload)
            {
                CurrentAct = SCAppConstants.CurrentAction.Unload;

                Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.UnloadArrivalToUnloadingTime);
                Thread.Sleep(SystemParameter.UnloadArrivalToUnloadingTime);
                Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.UnloadingToUnloadCompleteTime);
                Thread.Sleep(SystemParameter.UnloadingToUnloadCompleteTime);

                switch (operaType)
                {
                    case EnumOperaType.Abnormal_DoubleStorage:
                        Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.InterlockErrorUnload:
                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(false, string.Empty, string.Empty);
                        Send_Cmd144_StatusChangeReport();

                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Unload)
            {
                switch (operaType)
                {
                    case EnumOperaType.Abnormal_BcrReadFail:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadFailed);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdMismatch);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadDuplicate);
                        break;
                    case EnumOperaType.Abnormal_DoubleStorage:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadForceFinish);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        SetNoCommandStat(CMDCancelType.CmdAbort);
                        break;
                    default:
                        SetNoCommandStat();
                        break;
                }
            }
        }

        private void endCmd(object obj)
        {
            AskGuideData data = new AskGuideData();
            data.AskFor = SCAppConstants.CurrentAction.Unload;
            data.Target = recent31Cmd.UnloadPortID;
            AskGuideData(data);

            Send_Cmd132_TransferCompleteReport();
            Thread.Sleep(500);
            SetVehicleStatus(VHActionStatus.NoCommand);
            Send_Cmd144_StatusChangeReport();
            recent31Cmd = null;
        }

        private void load(object obj)
        {
            string cst_id = recent31Cmd.CSTID;
            string box_id = recent31Cmd.BOXID;
            string loadPort = recent31Cmd.LoadPortID;
            string unloadPort = recent31Cmd.UnloadPortID;

            Send_Cmd136_TransferEventReport(EventType.LoadComplete, loadPort, unloadPort, true);

            Thread.Sleep(500);
            setCstStat(true, cst_id, box_id);

            Thread.Sleep(SystemParameter.BCRReadTime);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, loadPort, unloadPort, true);
        }

        private void ScanFunction(ref bool is_routeChange)
        {
            if (CurrentAct == SCAppConstants.CurrentAction.None)
            {
                CurrentAct = SCAppConstants.CurrentAction.Initial;

                SetVehicleStatus(VHActionStatus.Commanding);
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);
                Send_Cmd144_StatusChangeReport();
                //SpinWait.SpinUntil(() => false, 500);
                Thread.Sleep(500);

                SetNextAction(null);
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Initial)
            {
                CurrentAct = SCAppConstants.CurrentAction.MoveToLoad;

                AskGuideData data = new AskGuideData();
                data.AskFor = SCAppConstants.CurrentAction.Load;
                data.Target = recent31Cmd.LoadPortID;
                AskGuideData(data);

                if (!isAtTarget(data))
                {
                    ChangeTheAddressSectionToTarget(ref is_routeChange);
                    if (is_routeChange)
                    {
                        return;
                    }
                }

                switch (operaType)
                {
                    case EnumOperaType.CancelComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;
                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    default:
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.MoveToLoad)
            {
                CurrentAct = SCAppConstants.CurrentAction.Load;

                Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadArrivalToLoadingTime);
                Thread.Sleep(SystemParameter.LoadArrivalToLoadingTime);
                Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                //SpinWait.SpinUntil(() => false, SystemParameter.LoadingToLoadCompleteTime);
                Thread.Sleep(SystemParameter.LoadingToLoadCompleteTime);

                string cst_id = recent31Cmd.CSTID;
                string box_id = recent31Cmd.BOXID;
                switch (operaType)
                {
                    case EnumOperaType.AbortComplete:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);

                        while (cmdCanceltype == CMDCancelType.CmdNone)
                        {
                            //SpinWait.SpinUntil(() => false, 1000);
                            Thread.Sleep(1000);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrReadFail:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);
                        ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
                        if (the36Response.RenameBOXID != "")
                        {
                            if (!scApp.isEmpty(recent31Cmd.CSTID))
                            {
                                cst_id = the36Response.RenameBOXID;
                            }

                            if (!scApp.isEmpty(recent31Cmd.BOXID))
                            {
                                box_id = the36Response.RenameBOXID;
                            }

                            setCstStat(true, cst_id, box_id);
                        }
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        if (!scApp.isEmpty(recent31Cmd.CSTID))
                        {
                            cst_id = Global_Param.MismatchID;
                        }

                        if (!scApp.isEmpty(recent31Cmd.BOXID))
                        {
                            box_id = Global_Param.MismatchID;
                        }

                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrMisMatch);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                    case EnumOperaType.InterlockError:
                        CurrentAct = SCAppConstants.CurrentAction.Unload;

                        SetNextAction(null);
                        break;
                    case EnumOperaType.ScanMismatche:
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrMisMatch);

                        SetNextAction(null);
                        break;
                    case EnumOperaType.ScanFail:
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);

                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(true, cst_id, box_id);
                        Send_Cmd144_StatusChangeReport();

                        //SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Thread.Sleep(SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Load)
            {
                CurrentAct = SCAppConstants.CurrentAction.Unload;

                switch (operaType)
                {
                    case EnumOperaType.InterlockErrorUnload:
                        Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                        //SpinWait.SpinUntil(() => false, SystemParameter.UnloadArrivalToUnloadingTime);
                        Thread.Sleep(SystemParameter.UnloadArrivalToUnloadingTime);
                        Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                        //SpinWait.SpinUntil(() => false, SystemParameter.UnloadingToUnloadCompleteTime);
                        Thread.Sleep(SystemParameter.UnloadingToUnloadCompleteTime);

                        SetNextAction(null);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                    case EnumOperaType.ScanMismatche:
                        Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true, BCRReadResult.BcrMisMatch);
                        //SpinWait.SpinUntil(() => false, 1000);
                        Thread.Sleep(1000);
                        Send_Cmd136_TransferEventReport(EventType.Vhunloading, true, BCRReadResult.BcrMisMatch);
                        //SpinWait.SpinUntil(() => false, 1000);
                        Thread.Sleep(1000);
                        Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true, BCRReadResult.BcrMisMatch);
                        //SpinWait.SpinUntil(() => false, 1000);
                        Thread.Sleep(1000);
                        Send_Cmd144_StatusChangeReport();

                        SetNextAction(null);
                        break;
                    default:
                        Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                        //SpinWait.SpinUntil(() => false, SystemParameter.UnloadArrivalToUnloadingTime);
                        Thread.Sleep(SystemParameter.UnloadArrivalToUnloadingTime);
                        Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                        //SpinWait.SpinUntil(() => false, SystemParameter.UnloadingToUnloadCompleteTime);
                        Thread.Sleep(SystemParameter.UnloadingToUnloadCompleteTime);

                        Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true);
                        //SpinWait.SpinUntil(() => false, 500);
                        Thread.Sleep(500);
                        setCstStat(false, string.Empty, string.Empty);
                        Send_Cmd144_StatusChangeReport();

                        SetNextAction(null);
                        break;
                }
            }
            else if (CurrentAct == SCAppConstants.CurrentAction.Unload)
            {
                switch (operaType)
                {
                    case EnumOperaType.Abnormal_BcrReadFail:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadFailed);
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdMismatch);
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        SetNoCommandStat(CMDCancelType.CmdCancelIdReadDuplicate);
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        SetNoCommandStat(CMDCancelType.CmdAbort);
                        break;
                    default:
                        SetNoCommandStat();
                        break;
                }
            }
        }

        private void setCstStat(bool is_load, string cst_id, string box_id)
        {
            if(is_load)
            {
                if(!scApp.isEmpty(cst_id))
                {
                    theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
                    theVehicleInfo.CSTID = cst_id;
                }

                if(!scApp.isEmpty(box_id))
                {
                    theVehicleInfo.HasBox = VhLoadCarrierStatus.Exist;
                    theVehicleInfo.BOXID = box_id;
                }
            }
            else
            {
                theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
                theVehicleInfo.CSTID = string.Empty;
                theVehicleInfo.HasBox = VhLoadCarrierStatus.NotExist;
                theVehicleInfo.BOXID = string.Empty;
            }
        }

        private void SetNoCommandStat(CMDCancelType cMDCancelType = CMDCancelType.CmdNone)
        {
            Send_Cmd132_TransferCompleteReport(cMDCancelType);
            //SpinWait.SpinUntil(() => false, 500);
            Thread.Sleep(500);
            SetVehicleStatus(VHActionStatus.NoCommand);
            Send_Cmd144_StatusChangeReport();
            
            // ===== 功能一：檢查是否有Queue命令 =====
            bool hasQueuedCmd = false;
            lock (cmdQueueLock)
            {
                // recent31Cmd = null 必須在 lock 內，防止空窗期新 ID_31 被誤判為 NoCommand
                recent31Cmd = null;

                if (queued31Cmd != null)
                {
                    logger.Info($"Vh:[{agent.VhID()}] Start executing queued command, CmdID:[{queued31Cmd.CmdID}]");
                    
                    // 將Queue命令移到執行命令（lock內只做狀態切換，不執行命令）
                    recent31Cmd = queued31Cmd;
                    queued31Cmd = null;
                    
                    // 重置狀態
                    cmdCanceltype = CMDCancelType.CmdNone;
                    CurrentAct = SCAppConstants.CurrentAction.None;
                    hasQueuedCmd = true;
                }
            }
            // lock 外再啟動命令，避免持鎖期間新 ID_31 無法回覆 ID_131
            if (hasQueuedCmd)
            {
                Thread.Sleep(500);
                SetBasicInform(theLoggerAgent, OnCmdSend);
                SetNextAction(null);
            }
            // ===== 新增結束 =====
        }

        private bool AskGuideData(AskGuideData data)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            string to = string.Empty;
            bool is_one_sec = false;

            //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData start, From Address:[{from_adr}] From Section:[{from_sec}] To:[{to}]");

            if (!isInOneSection(data))
            {
                //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData not one section");

                from_adr = scApp.ConfigDataBLL.getMoveFromAddress(from_adr, from_sec, theVehicleInfo.SecDistance);

                //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData get move from address, From Address:[{from_adr}]");

                switch (data.AskFor)
                {
                    case SCAppConstants.CurrentAction.Move:
                        //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData ask for move, Target:[{data.Target}]");
                        to = scApp.ConfigDataBLL.getMoveToAddress(data.Target, agent.VhID(), theVehicleInfo.CmdID);
                        break;
                    case SCAppConstants.CurrentAction.Load:
                    case SCAppConstants.CurrentAction.Unload:
                        //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData ask for load/unload, Target:[{data.Target}]");
                        to = scApp.ConfigDataBLL.getPortMapToAddress(data.Target, agent.VhID(), theVehicleInfo.CmdID);
                        break;
                }

                //logger_systemProcess.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}], AskGuideData ask to address, To Address:[{to}]");

                if(to.Trim().Equals("041002"))
                {
                    logger_error.Info($"Vh:[{agent.VhID()}] Cmd:[{theVehicleInfo.CmdID}] has error");
                }

                if (!Send_Cmd138_RequestGuideData(from_adr, to, out recent38Guide) || recent38Guide == null ||
                    recent38Guide.GuideInfoList == null || recent38Guide.GuideInfoList.Count == 0)
                {
                    while (true)
                    {
                        WaitForOneSecond();

                        if (Send_Cmd138_RequestGuideData(from_adr, to, out recent38Guide) && recent38Guide != null &&
                            recent38Guide.GuideInfoList != null && recent38Guide.GuideInfoList.Count > 0)
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                //logger_systemProcess.Info($"AskGuideData is one section");

                is_one_sec = true;
            }


            SetGuideData(data, is_one_sec);

            return true;
        }

        private bool isInOneSection(AskGuideData data)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string to_adr = string.Empty;
            string from_sec = theVehicleInfo.CurrentSecID;
            int from_distance = (int)theVehicleInfo.SecDistance;
            string sec_id = string.Empty;
            int to_distance = 0;

            PortMap port = null;
            switch (data.AskFor)
            {
                case SCAppConstants.CurrentAction.Move:
                    Section sec = scApp.ConfigDataBLL.getMoveToSection(data.Target, from_sec, out to_distance);
                    if (sec != null)
                    {
                        sec_id = sec.ID;
                        to_adr = sec.To_Adr;
                    }
                    break;
                case SCAppConstants.CurrentAction.Load:
                case SCAppConstants.CurrentAction.Unload:
                    port = scApp.ConfigDataBLL.loadPortMapByID(data.Target);
                    if (port != null)
                    {
                        sec_id = port.StationSection;
                        to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                    }
                    break;
            }

            if (!sec_id.Equals(string.Empty) && from_sec.Trim().Equals(sec_id.Trim()))
            {
                if (from_distance < to_distance && !to_adr.Equals(from_adr))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetGuideData(AskGuideData data, bool is_one_sec)
        {
            guideData = new GuideData();

            if (!is_one_sec)
            {
                guideData.GuideSection = recent38Guide.GuideInfoList[0].GuideSections.ToList();
                guideData.GuideAddress = recent38Guide.GuideInfoList[0].GuideAddresses.ToList();
                guideData.IsOneSection = false;
                //先清除原有的路徑資料
                theVehicleInfo.CurrentGuideSection.Clear();
                theVehicleInfo.CurrentGuideSection.AddRange(guideData.GuideSection);
            }
            else
            {
                guideData.IsOneSection = true;
            }

            if (recent31Cmd.ActType == ActiveType.Move)
            {
                Address adr = scApp.ConfigDataBLL.getAddress(data.Target);
                if(adr != null)
                {
                    guideData.TargetAdr = adr;

                    Section sec;
                    if (adr.Adr_Type == SCAppConstants.AddressType.Normal)
                    {
                        sec = scApp.ConfigDataBLL.getSectionByAdress(adr.Adr_ID);
                        guideData.Distance = 0;
                    }
                    else
                    {
                        sec = scApp.ConfigDataBLL.getSection(adr.Sec_ID);
                        guideData.Distance = int.Parse(adr.Distance);
                    }

                    guideData.TargetSec = sec;
                }
                else
                {
                    PortMap portMap = scApp.ConfigDataBLL.loadPortMapByID(data.Target);
                    guideData.TargetPort = data.Target;

                    adr = scApp.getAdrByID(portMap.StationAddress);
                    Section sec = scApp.ConfigDataBLL.getSection(portMap.StationSection);

                    guideData.TargetAdr = adr;
                    guideData.TargetSec = sec;
                    guideData.Distance = int.Parse(portMap.SEC_DIS_FROM_HEAD);
                }
            }
            else
            {
                PortMap portMap = scApp.ConfigDataBLL.loadPortMapByID(data.Target);
                guideData.TargetPort = data.Target;

                Address adr = scApp.getAdrByID(portMap.StationAddress);
                Section sec = scApp.ConfigDataBLL.getSection(portMap.StationSection);

                guideData.TargetAdr = adr;
                guideData.TargetSec = sec;
                guideData.Distance = int.Parse(portMap.SEC_DIS_FROM_HEAD);
            }

            if (scApp.isEmpty(guideData.TargetPort))
            {
                theVehicleInfo.CurrentDestination = guideData.TargetAdr.Adr_ID;
            }
            else
            {
                theVehicleInfo.CurrentDestination = guideData.TargetPort;
            }
        }

        private void SetVehicleStatus(VHActionStatus vhActionStatus)
        {
            switch (vhActionStatus)
            {
                case VHActionStatus.Commanding:
                    theVehicleInfo.ActionStatus = VHActionStatus.Commanding;
                    break;
                case VHActionStatus.NoCommand:
                    theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                    theVehicleInfo.CmdID = "";
                    if (theVehicleInfo.HasCst == VhLoadCarrierStatus.NotExist)
                    {
                        theVehicleInfo.BOXID = "";
                        theVehicleInfo.CSTID = "";
                        theVehicleInfo.LOTID = "";
                    }
                    break;
            }
        }


        private void ChangeTheAddressSectionToTarget(ref bool is_routeChange)
        {
            try
            {
                is_routeChange = false;
                RouteChangeData data = null;

                if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                if (!scApp.getIsJumpToTarget())
                {
                    if (guideData.GuideSection != null && guideData.GuideSection.Count > 0)
                    {
                        List<VirAddress> virAdrLst = null;
                        bool pv_obstecle = false;
                        int vir_adr_index;

                        if (IsInPreSection(guideData.GuideSection[0]))
                        {
                            virAdrLst = scApp.virAdrLst.Where(v => v.Sec_ID.Trim() == theVehicleInfo.CurrentSecID.Trim()).OrderBy(v => v.Seq_No).ToList();

                            theVehicleInfo.Angle = getSecAngle(theVehicleInfo.CurrentSecID);

                            pv_obstecle = false;
                            for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                            {
                                if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                                if (virAdrLst[vir_adr_index].Distance < theVehicleInfo.SecDistance)
                                {
                                    vir_adr_index++;
                                    continue;
                                }

                                if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "Pre-Guide"))
                                {
                                    Send_Cmd144_StatusChangeReport(true, true);
                                    pv_obstecle = true;
                                    //SpinWait.SpinUntil(() => false, 2000);
                                    Thread.Sleep(2000);
                                    continue;
                                }

                                if (pv_obstecle)
                                {
                                    pv_obstecle = false;
                                    Send_Cmd144_StatusChangeReport(false, true);
                                }

                                theVehicleInfo.Speed = 200;

                                scApp.updateVhAddress(agent.VhID(), virAdrLst[vir_adr_index].Loc_ID);

                                theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                                theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                                theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());

                                if (!virAdrLst[vir_adr_index].Is_Virtual)
                                {
                                    theVehicleInfo.CurrentAdrID = virAdrLst[vir_adr_index].Loc_ID;
                                }

                                checkDisIs25();

                                //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                Thread.Sleep(SystemParameter.MoveStepTime);

                                vir_adr_index++;
                            }
                        }

                        int sectionAddressCount = 0;
                        for (sectionAddressCount = 0; sectionAddressCount < guideData.GuideSection.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            if(is_routeChange && data != null)
                            {
                                logger_roadcontrol.Info($"Vh:[{agent.VhID()}] Target Port:[{data.TargetPort}] change route.");
                                RouteChange(data);
                                return;
                            }
                            else if(data != null)
                            {
                                data = null;
                            }

                            theVehicleInfo.Speed = 200;

                            string[] reserveSections = { guideData.GuideSection[sectionAddressCount] };
                            DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                            string section = guideData.GuideSection[sectionAddressCount];
                            string address = guideData.GuideAddress[sectionAddressCount];

                            bool is_block = false;
                            BlockData blockData = null;

                            List<BlockData> startBlockDataLst = scApp.ConfigDataBLL.loadBlockByStartSection(section);
                            if (startBlockDataLst != null && startBlockDataLst.Count > 0)
                            {
                                for (int index = sectionAddressCount + 1; index < guideData.GuideSection.Count; index++)
                                {
                                    string end_sec = guideData.GuideSection[index];
                                    string end_add = guideData.GuideAddress[index];


                                    blockData = startBlockDataLst.Where(b => b.End_Section.Trim() == end_add.Trim()).FirstOrDefault();

                                    if (blockData != null)
                                    {
                                        break;
                                    }
                                }

                                if (blockData != null)
                                {
                                    is_block = true;
                                }
                            }

                            bool is_pass = false;
                            if (is_block && blockData != null)
                            {
                                ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventBlockReserveReport(EventType.BlockReq, blockData.Block_ID);
                                //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                Thread.Sleep(SystemParameter.MoveStepTime);
                                if (recent36Response != null)
                                {
                                    if (IsChangeRoute(recent36Response))
                                    {
                                        data = new RouteChangeData();
                                        data.ActType = recent36Response.NextAction.ActType;
                                        data.TargetPort = recent36Response.NextAction.TargetPort;
                                        data.CstID = recent36Response.NextAction.CstID;
                                        data.CstType = recent36Response.NextAction.CstType;
                                        is_routeChange = true;

                                        //如果沒有給路權則立刻變換路徑
                                        if (recent36Response.IsBlockPass != PassType.Pass)
                                        {
                                            logger_roadcontrol.Info($"Vh:[{agent.VhID()}] 36 request change route Target Port:[{data.TargetPort}] Reserve:[{blockData.Block_ID}][Fail] change route now.");
                                            RouteChange(data);
                                            return;
                                        }
                                        //如果有給路權則走完這段才變換路徑
                                        else
                                        {
                                            logger_roadcontrol.Info($"Vh:[{agent.VhID()}] 36 request change route Target Port:[{data.TargetPort}] Reserve:[{blockData.Block_ID}][Success] change route after move.");
                                            BlockID = blockData.Block_ID;
                                            is_pass = true;
                                        }
                                    }
                                    else if (recent36Response.IsBlockPass == PassType.Pass)
                                    {
                                        BlockID = blockData.Block_ID;
                                        is_pass = true;
                                    }
                                }
                            }
                            else
                            {
                                ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);
                                if (recent36Response != null)
                                {
                                    if (IsChangeRoute(recent36Response))
                                    {
                                        data = new RouteChangeData();
                                        data.ActType = recent36Response.NextAction.ActType;
                                        data.TargetPort = recent36Response.NextAction.TargetPort;
                                        data.CstID = recent36Response.NextAction.CstID;
                                        data.CstType = recent36Response.NextAction.CstType;
                                        is_routeChange = true;

                                        //如果沒有給路權則立刻變換路徑
                                        if (recent36Response.IsReserveSuccess != ReserveResult.Success)
                                        {
                                            logger_roadcontrol.Info($"Vh:[{agent.VhID()}] 36 request change route Target Port:[{data.TargetPort}] Reserve:[{string.Join(",", reserveSections)}][Fail] change route now.");
                                            RouteChange(data);
                                            return;
                                        }
                                        //如果有給路權則走完這段才變換路徑
                                        else
                                        {
                                            logger_roadcontrol.Info($"Vh:[{agent.VhID()}] 36 request change route Target Port:[{data.TargetPort}] Reserve:[{string.Join(",", reserveSections)}][Success] change route after move.");
                                            is_pass = true;
                                        }
                                    }
                                    else if(recent36Response.IsReserveSuccess == ReserveResult.Success)
                                    {
                                        is_pass = true;
                                        // ===== IsAskGuideAgain =====
                                        if (recent36Response.IsAskGuideAgain)
                                        {
                                            HandleAskGuideAgain();
                                        }
                                        // ===== 結束 =====
                                    }
                                }
                            }

                            //SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);
                            Thread.Sleep(SystemParameter.Report136Time);

                            if (is_pass)
                            {
                                theVehicleInfo.Angle = getSecAngle(section);                                
                                //theVehicleInfo.SecDistance = 25;

                                //System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(Send_Cmd134_TransferEventReport), null);

                                Send_Cmd144_StatusChangeReport(false, true);

                                virAdrLst = null;
                                virAdrLst = scApp.virAdrLst.Where(v => v.Sec_ID.Trim() == section.Trim()).OrderBy(v => v.Seq_No).ToList();
                                theVehicleInfo.SecDistance = 0; // 進入新 Section 時重置 SecDistance，避免殘留上一段的大距離值導致擋車檢查距離計算溢出
                                pv_obstecle = false;
                                for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                                {
                                    if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                                    if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "Guide"))
                                    {
                                        Send_Cmd144_StatusChangeReport(true, true);
                                        pv_obstecle = true;
                                        //SpinWait.SpinUntil(() => false, 2000);
                                        Thread.Sleep(2000);
                                        continue;
                                    }

                                    scApp.updateVhAddress(agent.VhID(), virAdrLst[vir_adr_index].Loc_ID);

                                    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                    theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                                    theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                                    theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());
                                    if (!virAdrLst[vir_adr_index].Is_Virtual)
                                    {
                                        theVehicleInfo.CurrentAdrID = virAdrLst[vir_adr_index].Loc_ID;
                                    }

                                    checkDisIs25();

                                    if (pv_obstecle)
                                    {
                                        pv_obstecle = false;
                                        Send_Cmd144_StatusChangeReport(false, true);
                                    }

                                    //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                    Thread.Sleep(SystemParameter.MoveStepTime);
                                    vir_adr_index++;

                                    if (!scApp.isEmpty(BlockID) && !virAdrLst[vir_adr_index].Is_Virtual)
                                    {
                                        blockData = scApp.ConfigDataBLL.loadBlockByID(BlockID);
                                        if (SCApplication.isMatche(blockData.End_Section, virAdrLst[vir_adr_index].Loc_ID))
                                        {
                                            BlockID = string.Empty;
                                            Task.Run(() =>
                                            {
                                                //SpinWait.SpinUntil(() => false, 5000);
                                                Thread.Sleep(5000);
                                                Send_Cmd136_TransferEventReleaseBlockReport(EventType.BlockRelease, virAdrLst[vir_adr_index].Loc_ID);
                                            });
                                        }
                                    }
                                }

                                sectionAddressCount++;
                                // ===== Section 計時：記錄走完的 Section =====
                                lastPassedSecID = section;
                                if (sectionEnterTime != DateTime.MinValue)
                                {
                                    lastSecPassedTime = (long)(DateTime.Now - sectionEnterTime).TotalMilliseconds;
                                    sectionEnterTime = DateTime.MinValue;
                                }
                                logger_roadcontrol.Info($"Vh:[{agent.VhID()}] Section complete: [{section}], elapsed: {lastSecPassedTime} ms");
                                // ===== 結束 =====
                            }
                            else
                            {
                                Send_Cmd144_StatusChangeReport(false, false);
                            }
                        }
                    }

                    //if(guideData.Distance > 0)
                    if (!isAtTarget())
                    {
                        while (true)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            string[] reserveSections = { guideData.TargetSec.ID };
                            DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);

                            if (recent36Response != null)
                            {
                                if (IsChangeRoute(recent36Response))
                                {
                                    data = new RouteChangeData();
                                    data.ActType = recent36Response.NextAction.ActType;
                                    data.TargetPort = recent36Response.NextAction.TargetPort;
                                    data.CstID = recent36Response.NextAction.CstID;
                                    data.CstType = recent36Response.NextAction.CstType;
                                    is_routeChange = true;

                                    logger_roadcontrol.Info($"Vh:[{agent.VhID()}] 36 request change route Target Port:[{data.TargetPort}] Reserve:[{string.Join(",", reserveSections)}][Fail] change route now.");

                                    RouteChange(data);
                                    
                                    return;
                                }
                                else if(recent36Response.IsReserveSuccess == ReserveResult.Success)
                                {
                                    // ===== IsAskGuideAgain =====
                                    if (recent36Response.IsAskGuideAgain)
                                    {
                                        HandleAskGuideAgain();
                                    }
                                    // ===== 結束 =====
                                    Send_Cmd144_StatusChangeReport(false, true);
                                    break;
                                }
                                else
                                {
                                    Send_Cmd144_StatusChangeReport(false, false);
                                }
                            }

                            //SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);
                            Thread.Sleep(SystemParameter.Report136Time);
                        }

                        if (!guideData.IsOneSection)
                        {
                            theVehicleInfo.SecDistance = 0;
                            theVehicleInfo.CurrentSecID = guideData.TargetSec.ID;
                        }

                        List<VirAddress> virAdrLst = scApp.virAdrLst.Where(v => v.Sec_ID.Trim() == guideData.TargetSec.ID.Trim()).OrderBy(v => v.Seq_No).ToList();

                        theVehicleInfo.Angle = getSecAngle(guideData.TargetSec.ID);

                        bool pv_obstecle = false;
                        int vir_adr_index;
                        // ===== Section 計時：記錄進入最終 Section 的時間 =====
                        sectionEnterTime = DateTime.Now;
                        // ===== 結束 =====
                        for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            if (virAdrLst[vir_adr_index].Distance < theVehicleInfo.SecDistance)
                            {
                                vir_adr_index++;
                                continue;
                            }

                            //如果 Address 已經超過目的地，將 Flag On 起，於判斷是否遇到障礙時使用
                            // 註：原 isBeyondDest 旗標宣告但未實際讀取，先移除避免誤導；若後續真的要用再補回
                            int dis = int.MaxValue;
                            if (recent31Cmd.ActType == ActiveType.Move)
                            {
                                Address adr = scApp.ConfigDataBLL.getAddress(recent31Cmd.ToAdr);
                                if (adr != null && adr.Adr_Type == SCAppConstants.AddressType.Control)
                                {
                                    dis = int.Parse(adr.Distance);
                                }
                            }
                            else
                            {
                                PortMap port = scApp.ConfigDataBLL.loadPortMapByID(guideData.TargetPort);
                                if (port != null)
                                {
                                    dis = int.Parse(port.SEC_DIS_FROM_HEAD);
                                }
                            }

                            if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "After-Guide", dis))
                            {
                                Send_Cmd144_StatusChangeReport(true, true);
                                pv_obstecle = true;
                                //SpinWait.SpinUntil(() => false, 2000);
                                Thread.Sleep(2000);
                                continue;
                            }

                            if (pv_obstecle)
                            {
                                pv_obstecle = false;
                                Send_Cmd144_StatusChangeReport(false, true);
                            }

                            theVehicleInfo.Speed = 200;

                            var is_next_target = isNextAddressTarget(virAdrLst[vir_adr_index], out double pos_x, out double pos_y, out uint distance);
                            if (is_next_target.result)
                            {
                                if (is_next_target.is_on_adr)
                                {
                                    scApp.updateVhAddress(agent.VhID(), virAdrLst[vir_adr_index].Loc_ID);

                                    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                    theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                                    theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                                    theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());
                                    if (!virAdrLst[vir_adr_index].Is_Virtual)
                                    {
                                        theVehicleInfo.CurrentAdrID = virAdrLst[vir_adr_index].Loc_ID;
                                    }
                                }
                                else
                                {
                                    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                    theVehicleInfo.XAxis = pos_x;
                                    theVehicleInfo.YAxis = pos_y;
                                    theVehicleInfo.SecDistance = distance;
                                }

                                //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                Thread.Sleep(SystemParameter.MoveStepTime);

                                break;
                            }
                            else
                            {
                                //if(vir_adr_index == 0)
                                //{
                                //    theVehicleInfo.SecDistance = 25;
                                //    System.Threading.ThreadPool.QueueUserWorkItem(new WaitCallback(Send_Cmd134_TransferEventReport), null);
                                //}

                                scApp.updateVhAddress(agent.VhID(), virAdrLst[vir_adr_index].Loc_ID);

                                theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                                theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                                theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());
                                if (!virAdrLst[vir_adr_index].Is_Virtual)
                                {
                                    theVehicleInfo.CurrentAdrID = virAdrLst[vir_adr_index].Loc_ID;
                                    theVehicleInfo.Angle = setAngleForNextSec(virAdrLst[vir_adr_index].Loc_ID, virAdrLst[vir_adr_index].Sec_ID, theVehicleInfo.Angle);
                                }

                                checkDisIs25();
                            }

                            vir_adr_index++;
                        }
                    }

                    //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                    Thread.Sleep(SystemParameter.MoveStepTime);
                }
                else
                {
                    if (guideData.TargetPort == null || guideData.TargetPort.Equals(string.Empty))
                    {
                        theVehicleInfo.CurrentSecID = guideData.TargetSec.ID;
                        theVehicleInfo.CurrentAdrID = guideData.TargetAdr.Adr_ID;
                        theVehicleInfo.XAxis = double.Parse(guideData.TargetAdr.POS_X);
                        theVehicleInfo.YAxis = double.Parse(guideData.TargetAdr.POS_Y);
                        theVehicleInfo.SecDistance = uint.Parse(guideData.TargetAdr.Distance) - 2;
                        theVehicleInfo.Angle = getSecAngle(guideData.TargetSec.ID);
                    }
                    else
                    {
                        PortMap portMap = scApp.PortMapDao.loadPortMapByID(guideData.TargetPort);
                        if (portMap != null)
                        {
                            theVehicleInfo.CurrentSecID = portMap.StationSection;
                            theVehicleInfo.CurrentAdrID = portMap.StationAddress;
                            theVehicleInfo.XAxis = double.Parse(portMap.POS_X);
                            theVehicleInfo.YAxis = double.Parse(portMap.POS_Y);
                            theVehicleInfo.SecDistance = uint.Parse(portMap.SEC_DIS_FROM_HEAD);
                            theVehicleInfo.Angle = getSecAngle(portMap.StationSection);
                        }
                        else
                        {
                            theVehicleInfo.CurrentSecID = guideData.TargetSec.ID;
                            theVehicleInfo.CurrentAdrID = guideData.TargetAdr.Adr_ID;
                            theVehicleInfo.XAxis = double.Parse(guideData.TargetAdr.POS_X);
                            theVehicleInfo.YAxis = double.Parse(guideData.TargetAdr.POS_Y);
                            theVehicleInfo.SecDistance = uint.Parse(guideData.TargetAdr.Distance);
                            theVehicleInfo.Angle = getSecAngle(guideData.TargetSec.ID);
                        }
                    }

                    Send_Cmd144_StatusChangeReport(false, true);
                    //SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                    Thread.Sleep(SystemParameter.MoveStepTime);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                if(!is_routeChange)
                {
                    theVehicleInfo.Speed = 0;
                    theVehicleInfo.CurrentGuideSection.Clear();
                    theVehicleInfo.CurrentDestination = string.Empty;
                }
            }
        }

        private bool isAtTarget(AskGuideData data)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            int from_distance = (int)theVehicleInfo.SecDistance;
            string sec_id = string.Empty;
            string to_adr = string.Empty;
            int to_distance = 0;
            PortMap port = null;

            switch (data.AskFor)
            {
                case SCAppConstants.CurrentAction.Move:
                    to_adr = data.Target;
                    Section sec = scApp.ConfigDataBLL.getMoveToSection(data.Target, from_sec, out to_distance);
                    if (sec != null)
                    {
                        sec_id = sec.ID;
                    }
                    break;
                case SCAppConstants.CurrentAction.Load:
                case SCAppConstants.CurrentAction.Unload:
                    port = scApp.ConfigDataBLL.loadPortMapByID(data.Target);
                    if (port != null)
                    {
                        sec_id = port.StationSection;
                        to_adr = port.StationAddress;
                        to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                    }
                    break;
            }

            if (scApp.isMatche(from_sec, sec_id) && scApp.isMatche(from_adr, to_adr) && from_distance == to_distance)
            {
                return true;
            }

            return false;
        }

        private bool isAtTarget()
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            int from_distance = (int)theVehicleInfo.SecDistance;

            if (recent31Cmd.ActType == ActiveType.Move)
            {
                Address adr = scApp.ConfigDataBLL.getAddress(recent31Cmd.ToAdr);
                if (adr != null)
                {
                    if (adr.Adr_Type == SCAppConstants.AddressType.Normal && scApp.isMatche(adr.Adr_ID, from_adr) && from_distance == 0)
                    {
                        return true;
                    }
                    else if (adr.Adr_Type == SCAppConstants.AddressType.Control)
                    {
                        int dis = int.Parse(adr.Distance);

                        if (scApp.isMatche(from_sec, adr.Sec_ID) && scApp.isMatche(from_adr, adr.Adr_ID) && from_distance == dis)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    PortMap port = scApp.ConfigDataBLL.loadPortMapByID(guideData.TargetPort);
                    if (port != null)
                    {
                        int dis = int.Parse(port.SEC_DIS_FROM_HEAD);
                        if (scApp.isMatche(from_sec, port.StationSection) && scApp.isMatche(from_adr, port.StationAddress) && from_distance == dis)
                        {
                            return true;
                        }
                    }
                }
            }
            else
            {
                PortMap port = scApp.ConfigDataBLL.loadPortMapByID(guideData.TargetPort);
                if (port != null)
                {
                    int dis = int.Parse(port.SEC_DIS_FROM_HEAD);
                    if (scApp.isMatche(from_sec, port.StationSection) && scApp.isMatche(from_adr, port.StationAddress) && from_distance == dis)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        // ===== IsAskGuideAgain 處理 =====
        /// <summary>
        /// 收到 ID_36 IsAskGuideAgain=True 時，重新發送 ID_138 向 OHTC 申請路徑資訊。
        /// 若命令已經進入 RouteChange 流程，則由 LoadFunction(Initial) 自動處理。
        /// </summary>
        private void HandleAskGuideAgain()
        {
            try
            {
                logger_roadcontrol.Info($"Vh:[{agent.VhID()}] [IsAskGuideAgain=True] Re-requesting guide via ID_138.");
                AskGuideData askData = new AskGuideData();
                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Load:
                        askData.AskFor = SCAppConstants.CurrentAction.Load;
                        askData.Target = recent31Cmd.LoadPortID;
                        break;
                    case ActiveType.Unload:
                        askData.AskFor = SCAppConstants.CurrentAction.Unload;
                        askData.Target = recent31Cmd.UnloadPortID;
                        break;
                    default: // Move, MoveToMtl, Scan, etc.
                        askData.AskFor = SCAppConstants.CurrentAction.Move;
                        askData.Target = recent31Cmd.ToAdr;
                        break;
                }
                AskGuideData(askData);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        // ===== 新增結束 =====

        private bool IsChangeRoute(ID_36_TRANS_EVENT_RESPONSE recent36Response)
        {
            string target;
            if (scApp.isEmpty(guideData.TargetPort))
            {
                target = guideData.TargetAdr.Adr_ID;
            }
            else
            {
                target = guideData.TargetPort;
            }

            if (recent31Cmd.ActType == ActiveType.Move && recent36Response.NextAction != null &&
                recent36Response.NextAction.ActType == ActiveType.Move &&   // 只處理 Move→Move 改路；Move→Load 由 InSectionLoad 判斷處理
                !scApp.isEmpty(recent36Response.NextAction.TargetPort) && !scApp.isMatche(target, recent36Response.NextAction.TargetPort))
            {
                return true;
            }

            // ===== In-Section Load Assignment =====
            // 開啟模式 + 目前命令為 Move + Reserve 成功 + NextAction 為 Load + 無 Pre-Dispatch pending 命令
            if (InSectionLoadEnabled &&
                recent31Cmd.ActType == ActiveType.Move &&
                recent36Response.IsReserveSuccess == ReserveResult.Success &&
                recent36Response.NextAction != null &&
                recent36Response.NextAction.ActType == ActiveType.Load &&
                !scApp.isEmpty(recent36Response.NextAction.TargetPort) &&
                queued31Cmd == null)   // 與 Pre-Dispatch 互斥
            {
                logger_roadcontrol.Info($"Vh:[{agent.VhID()}] [InSectionLoad] NextAction.ActType=Load detected, " +
                    $"Port:[{recent36Response.NextAction.TargetPort}] CstID:[{recent36Response.NextAction.CstID}]");
                return true;
            }
            else if (!InSectionLoadEnabled &&
                recent31Cmd.ActType == ActiveType.Move &&
                recent36Response.NextAction != null &&
                recent36Response.NextAction.ActType == ActiveType.Load)
            {
                logger_roadcontrol.Info($"Vh:[{agent.VhID()}] [InSectionLoad] Disabled, ignore NextAction.ActType=Load " +
                    $"Port:[{recent36Response.NextAction.TargetPort}]");
            }
            // ===== 新增結束 =====

            return false;
        }

        private void RouteChange(RouteChangeData data)
        {
            try
            {
                switch (data.ActType)
                {
                    case ActiveType.Move:
                        CurrentAct = SCAppConstants.CurrentAction.Initial;
                        recent31Cmd.ToAdr = data.TargetPort;

                        System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SetNextAction), null);
                        break;
                    case ActiveType.Load:
                        CurrentAct = SCAppConstants.CurrentAction.None;  // 從 None 開始，走完整 LoadFunction 流程
                        recent31Cmd.LoadPortID = data.TargetPort;
                        recent31Cmd.ActType = ActiveType.Load;
                        // ===== In-Section Load Assignment =====
                        // 將 NextAction 帶入的 CstID 補充到命令中
                        if (!string.IsNullOrEmpty(data.CstID))
                        {
                            recent31Cmd.CSTID = data.CstID;
                        }
                        logger_roadcontrol.Info($"Vh:[{agent.VhID()}] [InSectionLoad] Move→Load upgrade. " +
                            $"Port:[{data.TargetPort}] CstID:[{data.CstID}] CmdID:[{recent31Cmd.CmdID}]");
                        // ===== 新增結束 =====
                        System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(SetNextAction), null);
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private (bool result, bool is_on_adr) isNextAddressTarget(VirAddress virAdr, out double pos_x, out double pos_y, out uint distance)
        {
            pos_x = 0;
            pos_y = 0;
            distance = 0;

            if (recent31Cmd.ActType == ActiveType.Move)
            {
                Address adr = scApp.ConfigDataBLL.getAddress(recent31Cmd.ToAdr);
                if(adr != null)
                {
                    if (adr.Adr_Type == SCAppConstants.AddressType.Normal && guideData.TargetAdr.Adr_ID.Trim().Equals(virAdr.Loc_ID.Trim()))
                    {
                        return (true, true);
                    }
                    else if (adr.Adr_Type == SCAppConstants.AddressType.Control)
                    {
                        int dis = int.Parse(adr.Distance);
                        if (virAdr.Distance > dis)
                        {
                            pos_x = double.Parse(adr.POS_X);
                            pos_y = double.Parse(adr.POS_Y);
                            distance = uint.Parse(dis.ToString());

                            return (true, false);
                        }
                    }
                }
                else
                {
                    PortMap port = scApp.ConfigDataBLL.loadPortMapByID(guideData.TargetPort);
                    if (port != null)
                    {
                        int dis = int.Parse(port.SEC_DIS_FROM_HEAD);
                        if (virAdr.Distance > dis)
                        {
                            pos_x = double.Parse(port.POS_X);
                            pos_y = double.Parse(port.POS_Y);
                            distance = uint.Parse(dis.ToString());

                            return (true, false);
                        }
                    }
                }
            }
            else
            {
                PortMap port = scApp.ConfigDataBLL.loadPortMapByID(guideData.TargetPort);
                if (port != null)
                {
                    int dis = int.Parse(port.SEC_DIS_FROM_HEAD);
                    if (virAdr.Distance > dis)
                    {
                        pos_x = double.Parse(port.POS_X);
                        pos_y = double.Parse(port.POS_Y);
                        distance = uint.Parse(dis.ToString());

                        return (true, false);
                    }
                }
            }

            return (false, false);
        }

        private bool IsInPreSection(string guide_sec)
        {
            if (scApp.isMatche(guide_sec, theVehicleInfo.CurrentSecID))
            {
                return false;
            }

            Section sec = scApp.ConfigDataBLL.getSection(theVehicleInfo.CurrentSecID);
            if (sec != null && scApp.isMatche(theVehicleInfo.CurrentAdrID, sec.To_Adr) && theVehicleInfo.SecDistance == 0)
            {
                return false;
            }

            if (theVehicleInfo.SecDistance == 0)
            {
                List<Section> secLst = scApp.ConfigDataBLL.getSectionByFromAdress(theVehicleInfo.CurrentAdrID);
                foreach (Section tmp in secLst)
                {
                    if (scApp.isMatche(tmp.ID, guide_sec))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool isCancelAbort()
        {
            if (cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
            {
                return true;
            }

            return false;
        }

        private void checkDisIs25()
        {
            if (theVehicleInfo.SecDistance == 25)
            {
                Send_Cmd134_TransferEventReport();
            }
        }

        private static void WaitForOneSecond()
        {
            //SpinWait.SpinUntil(() => false, 1000);
            Thread.Sleep(1000);
        }

        private void SetBasicInform(LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            this.theLoggerAgent = theLoggerAgent;
            this.OnCmdSend = OnCmdSend;

            theVehicleInfo.CmdID = recent31Cmd.CmdID;
            theVehicleInfo.LOTID = recent31Cmd.LOTID;
        }



        public void setCst(string cst_id)
        {
            theVehicleInfo.CSTID = cst_id;
            theVehicleInfo.CarCstID = cst_id;
            if (!cst_id.Trim().Equals(string.Empty))
            {
                theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
            }
            else
            {
                theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
            }
        }

        public double getSecAngle(string sec_id)
        {
            double angle = 0;

            Section sec = scApp.ConfigDataBLL.getSection(sec_id);
            if (sec != null)
            {
                Address from = scApp.ConfigDataBLL.getAddress(sec.From_Adr);
                Address to = scApp.ConfigDataBLL.getAddress(sec.To_Adr);

                if (from != null && to != null)
                {
                    int direction;
                    bool is_straight = isStraight(from, to, out direction);

                    if (is_straight)
                    {
                        if (direction == SCAppConstants.Direction.Right)
                        {
                            angle = 0;
                        }
                        else if (direction == SCAppConstants.Direction.Left)
                        {
                            angle = 0;
                        }
                        else if (direction == SCAppConstants.Direction.Up)
                        {
                            angle = 90;
                        }
                        else if (direction == SCAppConstants.Direction.Down)
                        {
                            angle = 90;
                        }
                    }
                    else
                    {
                        angle = 0;
                    }
                }
            }

            return angle;
        }

        public bool isStraight(Address from, Address to, out int direction)
        {
            direction = SCAppConstants.Direction.Right;

            int from_x = int.Parse(from.POS_X);
            int from_y = int.Parse(from.POS_Y);
            int to_x = int.Parse(to.POS_X);
            int to_y = int.Parse(to.POS_Y);

            if (from_x == to_x)
            {
                if (to_y > from_y)
                {
                    direction = SCAppConstants.Direction.Up;
                }
                else
                {
                    direction = SCAppConstants.Direction.Down;
                }

                return true;
            }
            else if (from_y == to_y)
            {
                if (to_x > from_x)
                {
                    direction = SCAppConstants.Direction.Right;
                }
                else
                {
                    direction = SCAppConstants.Direction.Left;
                }

                return true;
            }

            return false;
        }

        private double setAngleForNextSec(string adr_id, string sec_id, double angle)
        {
            Section sec = scApp.ConfigDataBLL.getSection(sec_id);
            if (sec != null && scApp.isMatche(sec.From_Adr, adr_id))
            {
                return angle;
            }

            List<Section> secLst = scApp.ConfigDataBLL.getSectionByFromAdress(adr_id);
            if (secLst != null && secLst.Count == 1)
            {
                angle = getSecAngle(secLst[0].ID);
            }

            return angle;
        }

        public void setPauseStat(ID_39_PAUSE_REQUEST receive)
        {
            if(receive.EventType == PauseEvent.Pause)
            {
                theVehicleInfo.PauseStatus = VhStopSingle.StopSingleOn;
            }
            else
            {
                theVehicleInfo.PauseStatus = VhStopSingle.StopSingleOff;
            }

            Send_Cmd144_StatusChangeReport();
        }
        #endregion

        private (bool isSuccess, WrapperMessage message) SendCommandWrapperWithLog(WrapperMessage wrapper, string log_msg)
        {
            string rtnMsg = string.Empty;
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            (bool is_success, WrapperMessage rtn_msg) result = sendRecvWithLog(wrapper, log_msg);
            if (!result.is_success)
            {
                theLoggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", "OHTC Not Reply=>" + msg));
                logger.Error($"{log_msg} OHTC Not Reply");
            }

            return result;
        }

        private (bool isSuccess, WrapperMessage message) SendCommandWrapperWithLogNoRetry(WrapperMessage wrapper, string log_msg)
        {
            string rtnMsg = string.Empty;
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            (bool is_success, WrapperMessage rtn_msg) result = sendRecvWithLogNoRetry(wrapper, log_msg);
            if (!result.is_success)
            {
                theLoggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", "OHTC Not Reply=>" + msg));
                logger.Error($"{log_msg} OHTC Not Reply");
            }

            return result;
        }

        /// <summary>
        /// Deep_Clone : Using for clone all the object by [Serializable]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="RealObject"></param>
        /// <returns></returns>
        public T Deep_Clone<T>(T RealObject) where T : IMessage<T>
        {
            try
            {
                // protobuf 內建 deep clone，取代已棄用且有 CVE 風險的 BinaryFormatter
                return RealObject == null ? RealObject : RealObject.Clone();
            }
            catch (Exception ex)
            {
                string err = ex.Message + Environment.NewLine + ex.StackTrace;
                return RealObject;
            }
        }

        public bool hasCmd()
        {
            if(theVehicleInfo.ActionStatus == VHActionStatus.Commanding)
            {
                return true;
            }

            if(recent31Cmd != null)
            {
                return true;
            }

            return false;
        }

        public void setSyncMapInfo()
        {
            syncMapInfo = true;
        }

        // ===== 功能一：雙命令Queue機制 - 新增公開方法 =====
        
        /// <summary>
        /// 取得當前命令Queue狀態
        /// </summary>
        public CmdQueueStatus GetCurrentCmdQueueStatus()
        {
            lock (cmdQueueLock)
            {
                if (recent31Cmd == null)
                    return CmdQueueStatus.NoCommand;
                else if (queued31Cmd == null)
                    return CmdQueueStatus.OneExecuting;
                else
                    return CmdQueueStatus.OneQueueing;
            }
        }

        /// <summary>
        /// 取得Queue命令資訊（用於顯示或查詢）
        /// </summary>
        public ID_31_TRANS_REQUEST GetQueuedCommand()
        {
            lock (cmdQueueLock)
            {
                return queued31Cmd;
            }
        }
        
        // ===== 新增結束 =====

    }
}
