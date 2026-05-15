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
// 2022/01/13    Steven Hong    N/A            A0.01   增加多車模擬及不撞車功能
// 2024/04/11    Steven Hong    N/A            A0.02   增加設定車上的CST
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
using static Mirle.Agvc.Simulator.App.SCAppConstants;

namespace Mirle.Agvc.Simulator
{
    class Opera
    {
        private EventHandler<string> OnCmdSend;
        private EnumOperaType operaType;
        private LoggerAgent theLoggerAgent;
        private CMDCancelType cmdCanceltype = CMDCancelType.CmdNone;
        public ID_144_STATUS_CHANGE_REP theVehicleInfo;
        //private ID_36_TRANS_EVENT_RESPONSE recent36Response;
        private ID_31_TRANS_REQUEST recent31Cmd;
        private ID_38_GUIDE_INFO_RESPONSE recent38Guide;
        private ID_37_TRANS_CANCEL_REQUEST iD_37_TRANS_Cmd;
        private GuideData guideData;
        private MiddlerConfigs middlerConfigs;
        private MiddleAgent agent;

        string BlockID = string.Empty;

        SCApplication scApp = null;

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

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

        public bool StartFeedBack31(ID_31_TRANS_REQUEST transRequest,
                                    LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                scApp = SCApplication.getInstance();

                cmdCanceltype = CMDCancelType.CmdNone;
                recent31Cmd = transRequest;
                WaitForOneSecond();
                SetBasicInform(theLoggerAgent, OnCmdSend);
                
                switch (operaType)
                {
                    case EnumOperaType.NormalComplete:
                        NormalComplete();
                        break;
                    case EnumOperaType.CancelComplete:
                        CancelComplete();
                        break;
                    case EnumOperaType.AbortComplete:
                        AbortComplete();
                        break;
                    case EnumOperaType.Abnormal_BcrReadFail:
                        Abnormal_BcrReadFail();
                        break;
                    case EnumOperaType.Abnormal_BcrMismatch:
                        Abnormal_BcrMismatch();
                        break;
                    case EnumOperaType.Abnormal_BcrDuplicate:
                        Abnormal_BcrDuplicate();
                        break;
                    case EnumOperaType.Abnormal_DoubleStorage:
                        Abnormal_DoubleStorage();
                        break;
                    case EnumOperaType.Abnormal_EmptyRetrieval:
                        Abnormal_EmptyRetrieval();
                        break;
                    case EnumOperaType.InterlockError:
                        InterlockError();
                        break;
                    case EnumOperaType.InterlockErrorUnload:
                        InterlockErrorUnload();
                        break;
                    case EnumOperaType.MTL:
                        MTL();
                        break;
                    case EnumOperaType.ScanMismatche:
                        ScanMismatch();
                        break;
                    case EnumOperaType.ScanFail:
                        ScanReadFail();
                        break;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool StartFeedBack37(ID_37_TRANS_CANCEL_REQUEST transRequest,
                                    LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            try
            {
                iD_37_TRANS_Cmd = Deep_Clone<ID_37_TRANS_CANCEL_REQUEST>(transRequest);
                switch (iD_37_TRANS_Cmd.ActType)
                {
                    case CMDCancelType.CmdCancel:
                        cmdCanceltype = CMDCancelType.CmdCancel;
                        break;
                    case CMDCancelType.CmdAbort:
                        cmdCanceltype = CMDCancelType.CmdAbort;
                        break;
                }
                WaitForOneSecond();
                return true;
            }
            catch
            {
                return false;
            }
        }

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
                while (retrySendRecv == true)
                {
                    returncode = agent.vehicleMessageSend.sendMessageUnaryNew(wrapper, log_msg);
                    retrySendRecv = false;
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

        private static void WaitForOneSecond()
        {
            SpinWait.SpinUntil(() => false, 1000);
        }

        private void SetBasicInform(LoggerAgent theLoggerAgent, EventHandler<string> OnCmdSend)
        {
            this.theLoggerAgent = theLoggerAgent;
            this.OnCmdSend = OnCmdSend;

            theVehicleInfo.CmdID = recent31Cmd.CmdID;
            theVehicleInfo.BOXID = recent31Cmd.BOXID;
            theVehicleInfo.CSTID = recent31Cmd.CSTID;
            theVehicleInfo.LOTID = recent31Cmd.LOTID;
        }

        private void SetGuideData(bool is_first, bool is_one_sec)
        {
            guideData = new GuideData();

            if(!is_one_sec)
            {
                guideData.GuideSection = recent38Guide.GuideInfoList[0].GuideSections.ToList();
                guideData.GuideAddress = recent38Guide.GuideInfoList[0].GuideAddresses.ToList();
                guideData.IsOneSection = false;

                theVehicleInfo.CurrentGuideSection.AddRange(guideData.GuideSection);
            }
            else
            {
                guideData.IsOneSection = true;
            }

            if (recent31Cmd.ActType == ActiveType.Move)
            {
                Address adr = scApp.ConfigDataBLL.getAddress(recent31Cmd.ToAdr);
                guideData.TargetAdr = adr;

                Section sec;
                if (adr.Adr_Type == SCAppConstants.AddressType.Normal)
                {
                    sec = scApp.ConfigDataBLL.getSectionByAdress(adr.Adr_ID);
                }
                else
                {
                    sec = scApp.ConfigDataBLL.getSection(adr.Sec_ID);
                }

                guideData.TargetSec = sec;
            }
            else
            {
                PortMap portMap;
                if (is_first)
                {
                    portMap = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.LoadPortID);
                    guideData.TargetPort = recent31Cmd.LoadPortID;
                }
                else
                {
                    portMap = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.UnloadPortID);
                    guideData.TargetPort = recent31Cmd.UnloadPortID;
                }

                Address adr = scApp.getAdrByID(portMap.StationAddress);
                Section sec = scApp.ConfigDataBLL.getSection(portMap.StationSection);

                guideData.TargetAdr = adr;
                guideData.TargetSec = sec;
            }
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

        // All OK
        private void NormalComplete()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Move:
                    try
                    {
                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        AskGuideData(true);
                        SetVehicleStatus(VHActionStatus.Commanding);
                        SpinWait.SpinUntil(() => false, 500);
                        Send_Cmd144_StatusChangeReport();
                        SpinWait.SpinUntil(() => false, 500);
                        if(!isAtTarget(true))
                        {
                            ChangeTheAddressSectionToUnloadAddress();
                        }
                        SpinWait.SpinUntil(() => false, 500);
                    }
                    finally
                    {
                        Send_Cmd132_TransferCompleteReport();
                        SpinWait.SpinUntil(() => false, 500);
                        SetVehicleStatus(VHActionStatus.NoCommand);
                        Send_Cmd144_StatusChangeReport();
                    }
                    break;
                case ActiveType.Load:
                    try
                    {
                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        SetVehicleStatus(VHActionStatus.Commanding);
                        SpinWait.SpinUntil(() => false, 500);
                        Send_Cmd144_StatusChangeReport();
                        SpinWait.SpinUntil(() => false, 500);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        LoadProcess(true);
                        SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        SpinWait.SpinUntil(() => false, 500);
                    }
                    finally
                    {
                        Send_Cmd132_TransferCompleteReport();
                        SpinWait.SpinUntil(() => false, 500);
                        SetVehicleStatus(VHActionStatus.NoCommand);
                        Send_Cmd144_StatusChangeReport();
                    }
                    break;
                case ActiveType.Unload:
                    try
                    {
                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        SetVehicleStatus(VHActionStatus.Commanding);
                        SpinWait.SpinUntil(() => false, 500);
                        Send_Cmd144_StatusChangeReport();
                        SpinWait.SpinUntil(() => false, 500);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        UnloadProcess(true);
                        SpinWait.SpinUntil(() => false, 500);
                    }
                    finally
                    {
                        Send_Cmd132_TransferCompleteReport();
                        SpinWait.SpinUntil(() => false, 500);
                        SetVehicleStatus(VHActionStatus.NoCommand);
                        Send_Cmd144_StatusChangeReport();
                    }
                    break;
                case ActiveType.Loadunload:
                    try
                    {
                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        SetVehicleStatus(VHActionStatus.Commanding);
                        SpinWait.SpinUntil(() => false, 500);
                        Send_Cmd144_StatusChangeReport();
                        SpinWait.SpinUntil(() => false, 500);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        LoadProcess(true);
                        SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        //SpinWait.SpinUntil(() => false, 1000);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        UnloadProcess(false);
                        SpinWait.SpinUntil(() => false, 500);
                    }
                    finally
                    {
                        Send_Cmd132_TransferCompleteReport();
                        SpinWait.SpinUntil(() => false, 500);
                        SetVehicleStatus(VHActionStatus.NoCommand);
                        SpinWait.SpinUntil(() => false, 500);
                        Send_Cmd144_StatusChangeReport();
                    }
                    break;
                case ActiveType.Scan:
                    try
                    {
                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        SetVehicleStatus(VHActionStatus.Commanding);
                        SpinWait.SpinUntil(() => false, 100);
                        Send_Cmd144_StatusChangeReport();
                        SpinWait.SpinUntil(() => false, 100);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        LoadProcess(true);
                        SpinWait.SpinUntil(() => false, SystemParameter.BCRReadTime);
                        Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                        SpinWait.SpinUntil(() => false, 100);

                        if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                        UnloadProcess(false);
                        SpinWait.SpinUntil(() => false, 100);
                    }
                    finally
                    {
                        Send_Cmd132_TransferCompleteReport();
                        SpinWait.SpinUntil(() => false, 100);
                        SetVehicleStatus(VHActionStatus.NoCommand);
                        SpinWait.SpinUntil(() => false, 100);
                        Send_Cmd144_StatusChangeReport();
                    }
                    break;
            }
           
        }
        private void ScanMismatch() {
            SetVehicleStatus(VHActionStatus.Commanding);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd144_StatusChangeReport();
            SpinWait.SpinUntil(() => false, 100);
            LoadProcess(true);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd136_TransferEventReport_Scan(EventType.Bcrread, true, BCRReadResult.BcrNormal);
            SpinWait.SpinUntil(() => false, 100);
            UnloadProcess_Scan();
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd132_TransferCompleteReport();
            SpinWait.SpinUntil(() => false, 100);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd144_StatusChangeReport();
        }

        private void ScanReadFail()
        {
            SetVehicleStatus(VHActionStatus.Commanding);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd144_StatusChangeReport();
            SpinWait.SpinUntil(() => false, 100);
            LoadProcess(true);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);
            SpinWait.SpinUntil(() => false, 100);
            UnloadProcess(false);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd132_TransferCompleteReport();
            SpinWait.SpinUntil(() => false, 100);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 100);
            Send_Cmd144_StatusChangeReport();
        }

        private void SetVehicleStatus(VHActionStatus vhActionStatus )
        {
            switch (vhActionStatus)
            {
                case VHActionStatus.Commanding:
                    theVehicleInfo.ActionStatus = VHActionStatus.Commanding;
                    break;
                case VHActionStatus.NoCommand:
                    theVehicleInfo.ActionStatus = VHActionStatus.NoCommand;
                    theVehicleInfo.CmdID = "";
                    theVehicleInfo.BOXID = "";
                    theVehicleInfo.CSTID = "";
                    theVehicleInfo.LOTID = "";
                    break;
            }
        }

        //TESTING
        private void CancelComplete()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Move:
                    AskGuideData(true);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }

                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Load:
                    AskGuideData(true);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Unload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    UnloadProcess(true);
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Loadunload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Scan:
                    AskGuideData(true);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
            }
        }
        //TESTING
        private void AbortComplete()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Unload:
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Loadunload:
                    LoadProcess(true);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Scan:
                    LoadProcess(true);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    while (cmdCanceltype == CMDCancelType.CmdNone)
                    {
                        SpinWait.SpinUntil(() => false, 1000);
                    }
                    Send_Cmd132_TransferCompleteReport();
                    break;
            }
        }

        private void MTL()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Movetomtl:
                    AskGuideData(true);
                    ChangeTheAddressSectionToUnloadAddress();
                    SpinWait.SpinUntil(() => false, 1000);
                    theVehicleInfo.ModeStatus = VHModeStatus.AutoMtl;
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    break;
                case ActiveType.Systemout:
                    AskGuideData(true);
                    ChangeTheAddressSectionToUnloadAddress();
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    break;
            }

        }

        // Testing
        private void InterlockError()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Move:
                    AskGuideData(true);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    //AskGuideData(theVehicleInfo.CurrentAdrID, recent31Cmd.ToAdr);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);

                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }

                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Load:
                    AskGuideData(true);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);

                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToLoadAddress();
                    }
                        
                    //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Unload:
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    UnloadProcess(true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Loadunload:
                    AskGuideData(true);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);

                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToLoadAddress();
                    }

                    //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Scan:
                    AskGuideData(true);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToLoadAddress();
                    }
                    //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                    Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
            }
        }

        private void InterlockErrorUnload()
        {
            switch (recent31Cmd.ActType)
            {
                case ActiveType.Move:
                    AskGuideData(true);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    //AskGuideData(theVehicleInfo.CurrentAdrID, recent31Cmd.ToAdr);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    } 
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Load:
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    LoadProcess(true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Unload:
                    AskGuideData(false);
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    if (!isAtTarget(true))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }
                    //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                    Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Loadunload:
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    LoadProcess(true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    if (!isAtTarget(false))
                    {
                        ChangeTheAddressSectionToUnloadAddress();
                    }
                    //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
                    Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
                case ActiveType.Scan:
                    SetVehicleStatus(VHActionStatus.Commanding);
                    SpinWait.SpinUntil(() => false, 100);
                    Send_Cmd144_StatusChangeReport();
                    SpinWait.SpinUntil(() => false, 100);
                    LoadProcess(true);
                    SpinWait.SpinUntil(() => false, 100);
                    Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
                    SpinWait.SpinUntil(() => false, 100);
                    Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
                    SpinWait.SpinUntil(() => false, 2000);
                    Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
                    SpinWait.SpinUntil(() => false, 1000);
                    Send_Cmd132_TransferCompleteReport();
                    SpinWait.SpinUntil(() => false, 1000);
                    SetVehicleStatus(VHActionStatus.NoCommand);
                    Send_Cmd144_StatusChangeReport();
                    break;
            }
        }

        // Testing
        private void Abnormal_BcrReadFail()
        {
            SetVehicleStatus(VHActionStatus.Commanding);
            Send_Cmd144_StatusChangeReport();
            AskGuideData(true);

            ChangeTheAddressSectionToLoadAddress();
            //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
            SpinWait.SpinUntil(() => false, 2000);
            Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
            SpinWait.SpinUntil(() => false, 12000);
            Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
            SpinWait.SpinUntil(() => false, 1000);
            theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
            Send_Cmd144_StatusChangeReport();


            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrReadFail);
            ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if(the36Response.RenameBOXID != "")
            {
                theVehicleInfo.CarBoxID = the36Response.RenameBOXID;
                theVehicleInfo.BOXID = the36Response.RenameBOXID;
                theVehicleInfo.CSTID = the36Response.RenameBOXID;
            }
            Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadFailed);
            //if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            //{
            //    UnloadProcess();
            //    Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadFailed);
            //}
            //else if (the36Response.ReplyActiveType == CMDCancelType.CmdCancelIdReadFailed)
            //{
            //    //Send_Cmd132_TransferCompleteReport(transRequest, CMDCancelType.CmdCancelIdReadFailed);
            //    Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadFailed);
            //}
            SpinWait.SpinUntil(() => false, 1000);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd144_StatusChangeReport();
        }

        private void Abnormal_BcrMismatch()
        {
            AskGuideData(true);
            ChangeTheAddressSectionToLoadAddress();
            //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
            SpinWait.SpinUntil(() => false, 2000);
            Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
            SpinWait.SpinUntil(() => false, 12000);
            Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
            SpinWait.SpinUntil(() => false, 1000);
            theVehicleInfo.BOXID = "Test1";
            theVehicleInfo.CSTID = "Test1";
            theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
            Send_Cmd144_StatusChangeReport();
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrMisMatch);
            //ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdMismatch);
            //if (the36Response.RenameBOXID != "")
            //{
            //    theVehicleInfo.CarBoxID = the36Response.RenameBOXID;
            //}
            //if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            //{
            //    UnloadProcess();
            //    Send_Cmd132_TransferCompleteReport();
            //}
            //else if (the36Response.ReplyActiveType == CMDCancelType.CmdCancelIdMismatch)
            //{
            //    Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdMismatch);
            //}
            SpinWait.SpinUntil(() => false, 1000);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd144_StatusChangeReport();
        }

        private void Abnormal_BcrDuplicate()
        {
            LoadProcess(true);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true, BCRReadResult.BcrNormal);
            //ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            //if (the36Response.RenameBOXID != "")
            //{
            //    theVehicleInfo.CarBoxID = the36Response.RenameBOXID;
            //}
            Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadDuplicate);
            //if (the36Response.ReplyActiveType == CMDCancelType.CmdNone)
            //{
            //    UnloadProcess();
            //    Send_Cmd132_TransferCompleteReport();
            //}
            //else if (the36Response.ReplyActiveType == CMDCancelType.CmdCancelIdReadDuplicate)
            //{
            //    Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadDuplicate);
            //}
            SpinWait.SpinUntil(() => false, 1000);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd144_StatusChangeReport();
        }

        private void Abnormal_DoubleStorage()
        {
            LoadProcess(true);
            Send_Cmd136_TransferEventReport(EventType.Bcrread, true);
            SpinWait.SpinUntil(() => false, 1000);
            if (recent31Cmd.ActType == ActiveType.Scan)
            {
                // Didn't have the unload move , but need to unload the box to the same place where it was loaded.
            }
            else
            {
                ChangeTheAddressSectionToUnloadAddress();
            }
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancelIdReadForceFinish);
            SpinWait.SpinUntil(() => false, 1000);
            SetVehicleStatus(VHActionStatus.NoCommand);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd144_StatusChangeReport();

        }

        private void DoubleStorageLoop()
        {
            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.DoubleStorage, true);
            //ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (recent36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                //do nothing.
                while (cmdCanceltype == CMDCancelType.CmdNone)
                {
                    SpinWait.SpinUntil(() => false, 1000);
                }
                Send_Cmd132_TransferCompleteReport();
            }
            else if (recent36Response.ReplyActiveType == CMDCancelType.CmdRetry)
            {
                DoubleStorageLoop();
            }
        }

        private void Abnormal_EmptyRetrieval()
        {
            AskGuideData(true);
            ChangeTheAddressSectionToLoadAddress();
            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
            SpinWait.SpinUntil(() => false, 1000);
            //EmptyRetrievalLoop(transRequest);
            Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd132_TransferCompleteReport(CMDCancelType.CmdCancel);
            //Send_Cmd132_TransferCompleteReport(transRequest);
        }

        private void EmptyRetrievalLoop()
        {
            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReport(EventType.EmptyRetrieval, true);
            //ID_36_TRANS_EVENT_RESPONSE the36Response = Deep_Clone<ID_36_TRANS_EVENT_RESPONSE>(recent36Response);
            if (recent36Response.ReplyActiveType == CMDCancelType.CmdNone)
            {
                //do nothing.
                while (cmdCanceltype == CMDCancelType.CmdNone)
                {
                    SpinWait.SpinUntil(() => false, 1000);
                }
                Send_Cmd132_TransferCompleteReport();
            }
            else if (recent36Response.ReplyActiveType == CMDCancelType.CmdRetry)
            {
                EmptyRetrievalLoop();
            }
        }

        private void LoadProcess(bool is_first)
        {
            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

            AskGuideData(is_first);
            if (!isAtTarget(is_first))
            {
                ChangeTheAddressSectionToLoadAddress();
            }    
               
            //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);

            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

            Send_Cmd136_TransferEventReport(EventType.LoadArrivals, true);
            SpinWait.SpinUntil(() => false, SystemParameter.LoadArrivalToLoadingTime);
            Send_Cmd136_TransferEventReport(EventType.Vhloading, true);
            SpinWait.SpinUntil(() => false, SystemParameter.LoadingToLoadCompleteTime);
            Send_Cmd136_TransferEventReport(EventType.LoadComplete, true);
            SpinWait.SpinUntil(() => false, 500);
            theVehicleInfo.BOXID = recent31Cmd.BOXID;
            theVehicleInfo.CSTID = recent31Cmd.CSTID;
            theVehicleInfo.HasCst = VhLoadCarrierStatus.Exist;
            Send_Cmd144_StatusChangeReport();
        }

        private void UnloadProcess(bool is_first)
        {
            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

            AskGuideData(is_first);

            if (recent31Cmd.ActType == ActiveType.Scan)
            {
                // Didn't have the unload move , but need to unload the box to the same place where it was loaded.
            }
            else
            {
                if (!isAtTarget(is_first))
                {
                    ChangeTheAddressSectionToUnloadAddress();
                }  
                //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);
            }

            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true);
            SpinWait.SpinUntil(() => false, SystemParameter.UnloadArrivalToUnloadingTime);
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, true);
            SpinWait.SpinUntil(() => false, SystemParameter.UnloadingToUnloadCompleteTime);
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true);
            SpinWait.SpinUntil(() => false, 500);
            theVehicleInfo.BOXID = string.Empty;
            theVehicleInfo.CSTID = string.Empty;
            theVehicleInfo.HasCst = VhLoadCarrierStatus.NotExist;
            Send_Cmd144_StatusChangeReport();
        }
        private void UnloadProcess_Scan()
        {
            //ChangeTheAddressSectionToUnloadAddress();
            Send_Cmd136_TransferEventReport(EventType.UnloadArrivals, true,BCRReadResult.BcrMisMatch);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd136_TransferEventReport(EventType.Vhunloading, true, BCRReadResult.BcrMisMatch);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd136_TransferEventReport(EventType.UnloadComplete, true, BCRReadResult.BcrMisMatch);
            SpinWait.SpinUntil(() => false, 1000);
            Send_Cmd144_StatusChangeReport();
        }


        private void ChangeTheAddressSectionToUnloadAddress()
        {
            try
            {
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
                                    SpinWait.SpinUntil(() => false, 2000);
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

                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);

                                vir_adr_index++;
                            }
                        }

                        int sectionAddressCount = 0;
                        for (sectionAddressCount = 0; sectionAddressCount < guideData.GuideSection.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

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
                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                if (recent36Response.IsBlockPass == PassType.Pass)
                                {
                                    BlockID = blockData.Block_ID;
                                    is_pass = true;
                                }
                            }
                            else
                            {
                                ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);
                                if (recent36Response != null && recent36Response.IsReserveSuccess == ReserveResult.Success)
                                {
                                    is_pass = true;
                                }
                            }

                            SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);

                            if (is_pass)
                            {
                                theVehicleInfo.Angle = getSecAngle(section);

                                theVehicleInfo.SecDistance = 25;

                                Send_Cmd134_TransferEventReport();

                                Send_Cmd144_StatusChangeReport(false, true);

                                virAdrLst = null;
                                virAdrLst = scApp.virAdrLst.Where(v => v.Sec_ID.Trim() == section.Trim()).OrderBy(v => v.Seq_No).ToList();
                                pv_obstecle = false;
                                for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                                {
                                    if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                                    if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "Guide"))
                                    {
                                        Send_Cmd144_StatusChangeReport(true, true);
                                        pv_obstecle = true;
                                        SpinWait.SpinUntil(() => false, 2000);
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

                                    if (pv_obstecle)
                                    {
                                        pv_obstecle = false;
                                        Send_Cmd144_StatusChangeReport(false, true);
                                    }

                                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                    vir_adr_index++;

                                    if (!scApp.isEmpty(BlockID) && !virAdrLst[vir_adr_index].Is_Virtual)
                                    {
                                        blockData = scApp.ConfigDataBLL.loadBlockByID(BlockID);
                                        if (SCApplication.isMatche(blockData.End_Section, virAdrLst[vir_adr_index].Loc_ID))
                                        {
                                            BlockID = string.Empty;
                                            Task.Run(() =>
                                            {
                                                SpinWait.SpinUntil(() => false, 5000);
                                                Send_Cmd136_TransferEventReleaseBlockReport(EventType.BlockRelease, virAdrLst[vir_adr_index].Loc_ID);
                                            });
                                        }
                                    }
                                }

                                sectionAddressCount++;
                            }
                            else
                            {
                                Send_Cmd144_StatusChangeReport(false, false);
                            }
                        }
                    }

                    if ((recent31Cmd.ActType == ActiveType.Move && guideData.TargetAdr.Adr_Type == SCAppConstants.AddressType.Control) ||
                        recent31Cmd.ActType != ActiveType.Move)
                    {
                        while (true)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            string[] reserveSections = { guideData.TargetSec.ID };
                            DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);
                            if (recent36Response != null && recent36Response.IsReserveSuccess == ReserveResult.Success)
                            {
                                Send_Cmd144_StatusChangeReport(false, true);
                                break;
                            }
                            else
                            {
                                Send_Cmd144_StatusChangeReport(false, false);
                            }

                            SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);
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
                        for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            if (virAdrLst[vir_adr_index].Distance < theVehicleInfo.SecDistance)
                            {
                                vir_adr_index++;
                                continue;
                            }

                            if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "After-Guide"))
                            {
                                Send_Cmd144_StatusChangeReport(true, true);
                                pv_obstecle = true;
                                SpinWait.SpinUntil(() => false, 2000);
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

                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);

                                break;
                            }
                            else
                            {
                                if (vir_adr_index == 0)
                                {
                                    theVehicleInfo.SecDistance = 25;
                                    Send_Cmd134_TransferEventReport();
                                }

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
                            }

                            vir_adr_index++;

                            #region 舊方法
                            //if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhNo(), virAdrLst[vir_adr_index].Loc_ID))
                            //{
                            //    Send_Cmd144_StatusChangeReport(true, true);
                            //    pv_obstecle = true;
                            //    SpinWait.SpinUntil(() => false, 2000);
                            //    continue;
                            //}

                            //scApp.updateVhAddress(agent.VhNo(), virAdrLst[vir_adr_index].Loc_ID);

                            //theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                            //theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                            //theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());
                            //if (!virAdrLst[vir_adr_index].Is_Virtual)
                            //{
                            //    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                            //    theVehicleInfo.CurrentAdrID = getRealeAddress(virAdrLst[vir_adr_index].Loc_ID, virAdrLst[vir_adr_index].Sec_ID);
                            //}

                            //if (pv_obstecle)
                            //{
                            //    pv_obstecle = false;
                            //    Send_Cmd144_StatusChangeReport(false, true);
                            //}

                            //Send_Cmd134_TransferEventReportWithCheck();
                            //SpinWait.SpinUntil(() => false, step_time);

                            //if (recent31Cmd.ActType == ActiveType.Move && guideData.TargetAdr.Adr_ID.Trim().Equals(virAdrLst[vir_adr_index].Loc_ID.Trim()))
                            //{
                            //    break;
                            //}
                            //else if (recent31Cmd.ActType != ActiveType.Move && guideData.TargetPort.Trim().Equals(virAdrLst[vir_adr_index].Loc_ID.Trim()))
                            //{
                            //    break;
                            //}

                            //vir_adr_index++;
                            #endregion
                        }
                    }

                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                }
                else
                {
                    if (guideData.TargetPort == null || guideData.TargetPort.Equals(string.Empty))
                    {
                        theVehicleInfo.CurrentSecID = guideData.TargetSec.ID;
                        theVehicleInfo.CurrentAdrID = guideData.TargetAdr.Adr_ID;
                        theVehicleInfo.XAxis = double.Parse(guideData.TargetAdr.POS_X);
                        theVehicleInfo.YAxis = double.Parse(guideData.TargetAdr.POS_Y);
                        theVehicleInfo.SecDistance = uint.Parse(guideData.TargetAdr.Distance);
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
                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                }
            }
            catch(Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                theVehicleInfo.Speed = 0;
                theVehicleInfo.CurrentGuideSection.Clear();
            }
        }

        private void ChangeTheAddressSectionToLoadAddress()
        {
            try
            {
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
                                    SpinWait.SpinUntil(() => false, 2000);
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

                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);

                                vir_adr_index++;
                            }
                        }

                        int sectionAddressCount = 0;
                        for (sectionAddressCount = 0; sectionAddressCount < guideData.GuideSection.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

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
                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                if (recent36Response.IsBlockPass == PassType.Pass)
                                {
                                    BlockID = blockData.Block_ID;
                                    is_pass = true;
                                }
                            }
                            else
                            {
                                ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);
                                if (recent36Response != null &&  recent36Response.IsReserveSuccess == ReserveResult.Success)
                                {
                                    is_pass = true;
                                }
                            }

                            SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);

                            if (is_pass)
                            {
                                theVehicleInfo.Angle = getSecAngle(section);
                                theVehicleInfo.SecDistance = 25;

                                Send_Cmd134_TransferEventReport();

                                Send_Cmd144_StatusChangeReport(false, true);

                                virAdrLst = scApp.virAdrLst.Where(v => v.Sec_ID.Trim() == section.Trim()).OrderBy(v => v.Seq_No).ToList();
                                pv_obstecle = false;
                                for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                                {
                                    if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                                    if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "Guide"))
                                    {
                                        Send_Cmd144_StatusChangeReport(true, true);
                                        pv_obstecle = true;
                                        SpinWait.SpinUntil(() => false, 2000);
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

                                    if (pv_obstecle)
                                    {
                                        pv_obstecle = false;
                                        Send_Cmd144_StatusChangeReport(false, true);
                                    }

                                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                                    vir_adr_index++;

                                    if (!scApp.isEmpty(BlockID) && !virAdrLst[vir_adr_index].Is_Virtual)
                                    {
                                        blockData = scApp.ConfigDataBLL.loadBlockByID(BlockID);
                                        if (SCApplication.isMatche(blockData.End_Section, virAdrLst[vir_adr_index].Loc_ID))
                                        {
                                            BlockID = string.Empty;
                                            Task.Run(() =>
                                            {
                                                SpinWait.SpinUntil(() => false, 5000);
                                                Send_Cmd136_TransferEventReleaseBlockReport(EventType.BlockRelease, virAdrLst[vir_adr_index].Loc_ID);
                                            });
                                        }
                                    }
                                }

                                sectionAddressCount++;
                            }
                            else
                            {
                                Send_Cmd144_StatusChangeReport(false, false);
                            }
                        }
                    }

                    if ((recent31Cmd.ActType == ActiveType.Move && guideData.TargetAdr.Adr_Type == SCAppConstants.AddressType.Control) ||
                            recent31Cmd.ActType != ActiveType.Move)
                    {
                        while (true)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            string[] reserveSections = { guideData.TargetSec.ID };
                            DriveDirction[] reserveDirection = { DriveDirction.DriveDirForward };

                            ID_36_TRANS_EVENT_RESPONSE recent36Response = Send_Cmd136_TransferEventReserveReport(EventType.ReserveReq, reserveSections, reserveDirection, true, recent31Cmd);
                            if (recent36Response != null && recent36Response.IsReserveSuccess == ReserveResult.Success)
                            {
                                Send_Cmd144_StatusChangeReport(false, true);
                                break;
                            }
                            else
                            {
                                Send_Cmd144_StatusChangeReport(false, false);
                            }

                            SpinWait.SpinUntil(() => false, SystemParameter.Report136Time);
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
                        for (vir_adr_index = 0; vir_adr_index < virAdrLst.Count;)
                        {
                            if (isCancelAbort()) return;  //收到Cancel或Abort，結束命令

                            if (virAdrLst[vir_adr_index].Distance < theVehicleInfo.SecDistance)
                            {
                                vir_adr_index++;
                                continue;
                            }

                            if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhID(), theVehicleInfo.SecDistance, virAdrLst[vir_adr_index], guideData.GuideSection, "After-Guide"))
                            {
                                Send_Cmd144_StatusChangeReport(true, true);
                                pv_obstecle = true;
                                SpinWait.SpinUntil(() => false, 2000);
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
                                        theVehicleInfo.Angle = setAngleForNextSec(virAdrLst[vir_adr_index].Loc_ID, virAdrLst[vir_adr_index].Sec_ID, theVehicleInfo.Angle);
                                    }
                                }
                                else
                                {
                                    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                                    theVehicleInfo.XAxis = pos_x;
                                    theVehicleInfo.YAxis = pos_y;
                                    theVehicleInfo.SecDistance = distance;
                                }

                                SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);

                                break;
                            }
                            else
                            {
                                if (vir_adr_index == 0)
                                {
                                    theVehicleInfo.SecDistance = 25;
                                    Send_Cmd134_TransferEventReport();
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
                            }

                            vir_adr_index++;

                            #region 舊放法
                            //if (!scApp.getDisableLocCheck() && !scApp.checkVhAddress(agent.VhNo(), virAdrLst[vir_adr_index].Loc_ID))
                            //{
                            //    Send_Cmd144_StatusChangeReport(true, true);
                            //    pv_obstecle = true;
                            //    SpinWait.SpinUntil(() => false, 2000);
                            //    continue;
                            //}

                            //scApp.updateVhAddress(agent.VhNo(), virAdrLst[vir_adr_index].Loc_ID);

                            //theVehicleInfo.XAxis = double.Parse(virAdrLst[vir_adr_index].POS_X);
                            //theVehicleInfo.YAxis = double.Parse(virAdrLst[vir_adr_index].POS_Y);
                            //theVehicleInfo.SecDistance = uint.Parse(virAdrLst[vir_adr_index].Distance.ToString());
                            //if (!virAdrLst[vir_adr_index].Is_Virtual)
                            //{
                            //    theVehicleInfo.CurrentSecID = virAdrLst[vir_adr_index].Sec_ID;
                            //    theVehicleInfo.CurrentAdrID = getRealeAddress(virAdrLst[vir_adr_index].Loc_ID, virAdrLst[vir_adr_index].Sec_ID);
                            //}

                            //if (pv_obstecle)
                            //{
                            //    pv_obstecle = false;
                            //    Send_Cmd144_StatusChangeReport(false, true);
                            //}

                            //Send_Cmd134_TransferEventReportWithCheck();
                            //SpinWait.SpinUntil(() => false, step_time);

                            //if (recent31Cmd.ActType == ActiveType.Move && guideData.TargetAdr.Adr_ID.Trim().Equals(virAdrLst[vir_adr_index].Loc_ID.Trim()))
                            //{
                            //    break;
                            //}
                            //else if (guideData.TargetPort.Trim().Equals(virAdrLst[vir_adr_index].Loc_ID.Trim()))
                            //{
                            //    break;
                            //}

                            //vir_adr_index++;
                            #endregion
                        }
                    }

                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                }
                else
                {
                    if (guideData.TargetPort == null || guideData.TargetPort.Equals(string.Empty))
                    {
                        theVehicleInfo.CurrentSecID = guideData.TargetSec.ID;
                        theVehicleInfo.CurrentAdrID = guideData.TargetAdr.Adr_ID;
                        theVehicleInfo.XAxis = double.Parse(guideData.TargetAdr.POS_X);
                        theVehicleInfo.YAxis = double.Parse(guideData.TargetAdr.POS_Y);
                        theVehicleInfo.SecDistance = uint.Parse(guideData.TargetAdr.Distance);
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
                    SpinWait.SpinUntil(() => false, SystemParameter.MoveStepTime);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            finally
            {
                theVehicleInfo.Speed = 0;
                theVehicleInfo.CurrentGuideSection.Clear();
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

            return (false, false);
        }

        private bool IsInPreSection(string guide_sec)
        {
            if(scApp.isMatche(guide_sec, theVehicleInfo.CurrentSecID))
            {
                return false;
            }

            Section sec = scApp.ConfigDataBLL.getSection(theVehicleInfo.CurrentSecID);
            if(sec != null && scApp.isMatche(theVehicleInfo.CurrentAdrID, sec.To_Adr) && theVehicleInfo.SecDistance == 0)
            {
                return false;
            }

            if(theVehicleInfo.SecDistance == 0)
            {
                List<Section> secLst = scApp.ConfigDataBLL.getSectionByFromAdress(theVehicleInfo.CurrentAdrID);
                foreach(Section tmp in secLst)
                {
                    if(scApp.isMatche(tmp.ID, guide_sec))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool AskGuideData(bool is_start)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            string to = string.Empty;
            bool is_one_sec = false;

            if(!isInOneSection(is_start))
            {
                if (is_start)
                {
                    from_adr = scApp.ConfigDataBLL.getMoveFromAddress(from_adr, from_sec, theVehicleInfo.SecDistance);

                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                            to = scApp.ConfigDataBLL.getMoveToAddress(recent31Cmd.ToAdr, string.Empty, string.Empty);
                            break;
                        case ActiveType.Load:
                        case ActiveType.Loadunload:
                        case ActiveType.Scan:
                            to = scApp.ConfigDataBLL.getPortMapToAddress(recent31Cmd.LoadPortID, string.Empty, string.Empty);
                            break;
                        case ActiveType.Unload:
                            to = scApp.ConfigDataBLL.getPortMapToAddress(recent31Cmd.UnloadPortID, string.Empty, string.Empty);
                            break;
                    }
                }
                else
                {
                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                        case ActiveType.Unload:
                        case ActiveType.Load:
                        case ActiveType.Scan:
                            return false;
                        case ActiveType.Loadunload:
                            Section sec = scApp.ConfigDataBLL.getSection(from_sec);
                            if (sec != null)
                            {
                                from_adr = sec.To_Adr;
                            }
                            to = scApp.ConfigDataBLL.getPortMapToAddress(recent31Cmd.UnloadPortID, string.Empty, string.Empty);
                            break;
                    }
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
                is_one_sec = true;
            }
            

            SetGuideData(is_start, is_one_sec);

            return true;
        }

        private bool isInOneSection(bool is_start)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            int from_distance = (int)theVehicleInfo.SecDistance;
            string sec_id = string.Empty;
            int to_distance = 0;

            if (is_start)
            {
                PortMap port = null;
                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Move:
                        Section sec = scApp.ConfigDataBLL.getMoveToSection(recent31Cmd.ToAdr, from_sec, out to_distance);
                        if(sec != null)
                        {
                            sec_id = sec.ID;
                        }
                        break;
                    case ActiveType.Load:
                    case ActiveType.Loadunload:
                    case ActiveType.Scan:
                        port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.LoadPortID);
                        if(port != null)
                        {
                            sec_id = port.StationSection;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                    case ActiveType.Unload:
                        port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.UnloadPortID);
                        if (port != null)
                        {
                            sec_id = port.StationSection;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                }

                if(!sec_id.Equals(string.Empty) && from_sec.Trim().Equals(sec_id.Trim()))
                {
                    if(from_distance < to_distance)
                    { 
                        return true;
                    }
                }
            }
            else
            {
                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Move:
                    case ActiveType.Unload:
                    case ActiveType.Load:
                    case ActiveType.Scan:
                        return false;
                    case ActiveType.Loadunload:
                        PortMap port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.UnloadPortID);
                        if (port != null)
                        {
                            sec_id = port.StationSection;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                }

                if (!sec_id.Equals(string.Empty) && from_sec.Trim().Equals(sec_id.Trim()))
                {
                    if (from_distance < to_distance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private string getRealeAddress(string adr_id, string sec_id)
        {
            string rtn_adr_id = adr_id;

            try
            {
                Address adr = scApp.ConfigDataBLL.getAddress(adr_id);
                if (adr != null && adr.Adr_Type == SCAppConstants.AddressType.Normal)
                {
                    rtn_adr_id = adr.Adr_ID;
                }
                else
                {
                    Section sec = scApp.ConfigDataBLL.getSection(sec_id);
                    if(sec != null)
                    {
                        rtn_adr_id = sec.From_Adr;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return rtn_adr_id;
        }

        private bool isCancelAbort()
        {
            if(cmdCanceltype == CMDCancelType.CmdCancel || cmdCanceltype == CMDCancelType.CmdAbort)
            {
                return true;
            }

            return false;
        }

        private bool isAtTarget(bool is_start)
        {
            string from_adr = theVehicleInfo.CurrentAdrID;
            string from_sec = theVehicleInfo.CurrentSecID;
            int from_distance = (int)theVehicleInfo.SecDistance;
            string sec_id = string.Empty;
            string to_adr = string.Empty;
            int to_distance = 0;

            if (is_start)
            {
                PortMap port = null;
                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Move:
                        to_adr = recent31Cmd.ToAdr;
                        Section sec = scApp.ConfigDataBLL.getMoveToSection(recent31Cmd.ToAdr, from_sec, out to_distance);
                        if (sec != null)
                        {
                            sec_id = sec.ID;
                        }
                        break;
                    case ActiveType.Load:
                    case ActiveType.Loadunload:
                    case ActiveType.Scan:
                        port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.LoadPortID);
                        if (port != null)
                        {
                            sec_id = port.StationSection;
                            to_adr = port.StationAddress;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                    case ActiveType.Unload:
                        port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.UnloadPortID);
                        if (port != null)
                        {
                            sec_id = port.StationSection;
                            to_adr = port.StationAddress;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                }
            }
            else
            {
                switch (recent31Cmd.ActType)
                {
                    case ActiveType.Move:
                    case ActiveType.Unload:
                    case ActiveType.Load:
                    case ActiveType.Scan:
                        return false;
                    case ActiveType.Loadunload:
                        PortMap port = scApp.ConfigDataBLL.loadPortMapByID(recent31Cmd.UnloadPortID);
                        if (port != null)
                        {
                            sec_id = port.StationSection;
                            to_adr = port.StationAddress;
                            to_distance = int.Parse(port.SEC_DIS_FROM_HEAD);
                        }
                        break;
                }
            }

            if (scApp.isMatche(from_sec, sec_id) && scApp.isMatche(from_adr, to_adr) && from_distance == to_distance)
            {
                return true;
            }

            return false;
        }

        public double getSecAngle(string sec_id)
        {
            double angle = 0;

            Section sec = scApp.ConfigDataBLL.getSection(sec_id);
            if(sec != null)
            {
                Address from = scApp.ConfigDataBLL.getAddress(sec.From_Adr);
                Address to = scApp.ConfigDataBLL.getAddress(sec.To_Adr);

                if(from != null && to != null)
                {
                    int direction;
                    bool is_straight = isStraight(from, to, out direction);

                    if(is_straight)
                    {
                        if(direction == SCAppConstants.Direction.Right)
                        {
                            angle = 0;
                        }
                        else if(direction == SCAppConstants.Direction.Left)
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
                if(to_y > from_y)
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
            if(sec != null && scApp.isMatche(sec.From_Adr, adr_id))
            {
                return angle;
            }

            List<Section> secLst = scApp.ConfigDataBLL.getSectionByFromAdress(adr_id);
            if(secLst != null && secLst.Count == 1)
            {
                angle = getSecAngle(secLst[0].ID);
            }

            return angle;
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

        public void Send_Cmd144_StatusChangeReport()
        {
            try
            {
                ID_144_STATUS_CHANGE_REP iD_144_SendToOHTC = new ID_144_STATUS_CHANGE_REP();
                iD_144_SendToOHTC = Deep_Clone<ID_144_STATUS_CHANGE_REP>(theVehicleInfo);
                if(theVehicleInfo.HasCst == VhLoadCarrierStatus.Exist)
                {
                    setHaveCstBox144(iD_144_SendToOHTC);
                }
                else if (theVehicleInfo.HasCst == VhLoadCarrierStatus.NotExist)
                {
                    setEmptyCstBox144(iD_144_SendToOHTC);
                }
                //scApp.updateVhAddress(agent.VhNo(), theVehicleInfo.CurrentAdrID);

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.StatueChangeRepFieldNumber;
                wrappers.StatueChangeRep = iD_144_SendToOHTC;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[144]";
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLog(wrappers, log_msg);
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
                if (theVehicleInfo.HasCst == VhLoadCarrierStatus.Exist)
                {
                    setHaveCstBox144(iD_144_SendToOHTC);
                }
                else if (theVehicleInfo.HasCst == VhLoadCarrierStatus.NotExist)
                {
                    setEmptyCstBox144(iD_144_SendToOHTC);
                }
                
                if(has_obstacle)
                {
                    iD_144_SendToOHTC.ObstacleStatus = VhStopSingle.StopSingleOn;
                }
                else
                {
                    iD_144_SendToOHTC.ObstacleStatus = VhStopSingle.StopSingleOff;
                }

                if(reserve_result)
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
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLog(wrappers, log_msg);
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
                iD_134_TRANS_EVENT_REP.CurrentAdrID = theVehicleInfo.CurrentAdrID;
                iD_134_TRANS_EVENT_REP.CurrentSecID = theVehicleInfo.CurrentSecID;
                iD_134_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
                iD_134_TRANS_EVENT_REP.Speed = theVehicleInfo.Speed;
                iD_134_TRANS_EVENT_REP.XAxis = theVehicleInfo.XAxis;
                iD_134_TRANS_EVENT_REP.YAxis = theVehicleInfo.YAxis;
                iD_134_TRANS_EVENT_REP.Angle = theVehicleInfo.Angle;

                WrapperMessage wrappers = new WrapperMessage();
                wrappers.ID = WrapperMessage.TransEventRepFieldNumber;
                wrappers.TransEventRep = iD_134_TRANS_EVENT_REP;

                string log_msg = $"Vehicle No:[{agent.VhID()}] Send Message:[134]";
                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLog(wrappers, log_msg);

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

                var send_result = agent.vehicleMessageSend.sendMessageUnaryWithLog(wrappers, log_msg);
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
                iD_134_TRANS_EVENT_REP.EventType = EventType.AdrPass ;
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

                if(!send_result.isSuccess)
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
                iD_136_TRANS_EVENT_REP.CSTID = Global_Param.MismatchID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
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
                iD_136_TRANS_EVENT_REP.CSTID = theVehicleInfo.CSTID;
                iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

                iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
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
            iD_136_TRANS_EVENT_REP.CSTID = CSTID;
            iD_136_TRANS_EVENT_REP.LOTID = theVehicleInfo.LOTID;

            iD_136_TRANS_EVENT_REP.SecDistance = theVehicleInfo.SecDistance;
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
                            setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                        case ActiveType.Scan:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                            setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
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
                            setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT, cMDCancelType);
                            break;
                        case ActiveType.Scan:
                            iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                            setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            break;
                    }
                }
                else if(operaType == EnumOperaType.MTL)
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
                }
                else
                {
                    switch (recent31Cmd.ActType)
                    {
                        case ActiveType.Move:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusMove;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Load:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoad;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT, cMDCancelType);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusInterlockError;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusInterlockError;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadForceFinish)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIddoubleStorage;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            break;
                        case ActiveType.Unload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusUnload;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            break;
                        case ActiveType.Loadunload:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusLoadunload;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadFailed)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdreadFailed;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdMismatch)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            else if (cMDCancelType == CMDCancelType.CmdCancelIdReadForceFinish)
                            {
                                //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIddoubleStorage;
                                setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                                iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.CarBoxID;
                            }
                            //else if (cMDCancelType == CMDCancelType.CmdCancelEmptyRetrieval)
                            //{
                            //    //iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdmisMatch;
                            //    iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusIdemptyRetrival;
                            //    setHaveCstBox132(iD_132_TRANS_COMPLETE_REPORT);
                            //    iD_132_TRANS_COMPLETE_REPORT.CarBoxID = "";
                            //}
                            break;
                        case ActiveType.Scan:
                            if (cMDCancelType == CMDCancelType.CmdNone)
                            {
                                iD_132_TRANS_COMPLETE_REPORT.CmpStatus = CompleteStatus.CmpStatusScan;
                                setEmptyCarrier132(iD_132_TRANS_COMPLETE_REPORT);
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

        private void setHaveCstBox132(ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT, CMDCancelType cMDCancelType = CMDCancelType.CmdNone)
        {
            iD_132_TRANS_COMPLETE_REPORT.HasBox = VhLoadCarrierStatus.Exist;
            iD_132_TRANS_COMPLETE_REPORT.CarBoxID = theVehicleInfo.BOXID;
            iD_132_TRANS_COMPLETE_REPORT.HasCst = VhLoadCarrierStatus.Exist;
            iD_132_TRANS_COMPLETE_REPORT.CarCstID = theVehicleInfo.CSTID;
        }

        private static void setEmptyCarrier132(ID_132_TRANS_COMPLETE_REPORT iD_132_TRANS_COMPLETE_REPORT)
        {
            iD_132_TRANS_COMPLETE_REPORT.HasBox = VhLoadCarrierStatus.NotExist;
            iD_132_TRANS_COMPLETE_REPORT.CarBoxID = "";
            iD_132_TRANS_COMPLETE_REPORT.HasCst = VhLoadCarrierStatus.NotExist;
            iD_132_TRANS_COMPLETE_REPORT.CarCstID = "";
        }

        private void setHaveCstBox144(ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP)
        {
            iD_144_STATUS_CHANGE_REP.HasBox = VhLoadCarrierStatus.Exist;
            iD_144_STATUS_CHANGE_REP.CarBoxID = theVehicleInfo.BOXID;
            iD_144_STATUS_CHANGE_REP.HasCst = VhLoadCarrierStatus.Exist;
            iD_144_STATUS_CHANGE_REP.CarCstID = theVehicleInfo.CSTID;
        }
        private static void setEmptyCstBox144(ID_144_STATUS_CHANGE_REP iD_144_STATUS_CHANGE_REP)
        {
            iD_144_STATUS_CHANGE_REP.HasBox = VhLoadCarrierStatus.NotExist;
            iD_144_STATUS_CHANGE_REP.CarBoxID = "";
            iD_144_STATUS_CHANGE_REP.HasCst = VhLoadCarrierStatus.NotExist;
            iD_144_STATUS_CHANGE_REP.CarCstID = "";
        }
        
        private (bool isSuccess, WrapperMessage message) SendCommandWrapper(WrapperMessage wrapper)
        {
            string rtnMsg = string.Empty;
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            (bool is_success, WrapperMessage rtn_msg) result = sendRecv(wrapper);
            if (!result.is_success) {
                theLoggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID" ,"OHTC Not Reply=>"+ msg));
            }

            return result;
        }

        private (bool isSuccess, WrapperMessage message) SendCommandWrapperWithLog(WrapperMessage wrapper, string log_msg)
        {
            string rtnMsg = string.Empty;
            string msg = $"[SEND] [ID = {wrapper.ID}][SeqNum = {wrapper.SeqNum}] " + wrapper.ToString();
            OnCmdSend?.Invoke(this, msg);
            (bool is_success, WrapperMessage rtn_msg) result = sendRecvWithLog(wrapper, log_msg);
            if (!result.is_success)
            {
                theLoggerAgent.LogMsg("Error", new LogFormat("Error", "1", GetType().Name + ":" + MethodBase.GetCurrentMethod().Name, "Device", "CarrierID", "OHTC Not Reply=>" + msg));
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
    }
}
