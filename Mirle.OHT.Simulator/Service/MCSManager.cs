using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.stc.Common.SECS;
using OverhaedHoistTransporter_Simulator.Action;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OverhaedHoistTransporter_Simulator.VO
{
    public class MCSManager
    {
        MCSManagerAction action;
        SCApplication scApp;
        public SECSAgent agent { get; private set; }
        public MCSManager(string name, int deviceID, string localIP, int localPort, string remoteIP, int remotePort,
            SECSAgent.SECS_AGENT_COMM_MODE connectMode,
            int t3Timeout, int t5Timeout, int t6Timeout, int t7Timeout, int t8Timeout, int linkTestTimer, string TX_LOG_PATH, int retryCount,
            SECSAgent.PROCESS_RECEIVE_ORDER procReceiveOrder, List<string> streamFunctionProcByReceiveQueue, SECSAgent.SFTypeCheck typeCheckFunction)
        {
            agent = new SECSAgent(name, deviceID, localIP, localPort, remoteIP, remotePort, connectMode,
                t3Timeout, t5Timeout, t6Timeout, t7Timeout, t8Timeout, 30, TX_LOG_PATH,
                retryCount, SECSAgent.PROCESS_RECEIVE_ORDER.Normal, null, null, checkDataValue);
            Task.Run
                (() =>
                {
                    agent.start();
                });
            action = new MCSManagerAction(this);
        }

        public void addSECSConnectedHandler(EventHandler<com.mirle.ibg3k0.stc.Common.SECSEventArgs> handler)
        {
            agent.addSECSConnectedHandler(handler);
        }
        public void removeSECSConnectedHandler(EventHandler<com.mirle.ibg3k0.stc.Common.SECSEventArgs> handler)
        {
            agent.removeSECSConnectedHandler(handler);
        }
        public void addSECSDisconnectedHandler(EventHandler<com.mirle.ibg3k0.stc.Common.SECSEventArgs> handler)
        {
            agent.addSECSDisconnectedHandler(handler);
        }
        public void removeSECSDisconnectedHandler(EventHandler<com.mirle.ibg3k0.stc.Common.SECSEventArgs> handler)
        {
            agent.removeSECSDisconnectedHandler(handler);
        }
        public bool isConnection { get { return agent.IsSelected; } }

        /// <summary>
        /// 停止 MCSManager 連接
        /// </summary>
        public void Stop()
        {
            try
            {
                if (agent != null)
                {
                    agent.stop();
                }
            }
            catch (Exception ex)
            {
                // 記錄異常但不拋出，避免影響整體流程
                System.Console.WriteLine($"停止 MCSManager 時發生異常: {ex.Message}");
            }
        }

        object tranCmdSync = new object();
        public void sendTranCmd(string source_port, string destn_port)
        {
            lock (tranCmdSync)
            {
                string cmdType = string.Concat(source_port, "To", destn_port);
                string cmdID = getTranCmdID(cmdType);
                action.sendS2F49(cmdID, "CST01",
                    source_port, destn_port);
            }
        }

        public void sendTranCmd(com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER s2f49)
        {
            bool return_code = action.sendS2F49GREE(s2f49);

            if(scApp == null)
            {
               scApp = SCApplication.getInstance();
            }

            if (return_code)
            {
                scApp.setNextCmd(s2f49.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL);
            }
            else
            {
                scApp.setCmdSendFail(s2f49.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL);
            }
        }
        public async Task sendTranCmdAsync(com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER s2f49)
        {
            bool return_code = await action.sendS2F49GREEAsync(s2f49);

            if (scApp == null)
            {
                scApp = SCApplication.getInstance();
            }

            if (return_code)
            {
                scApp.setNextCmd(s2f49.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL);
            }
            else
            {
                scApp.setCmdSendFail(s2f49.REPITEMS.TRANINFO.TRANSFERINFOVALUE.CARRIERIDINFO.CPVAL);
            }
        }
        public void sendCmdCancleAbort(string cmd_id, string rcmd)
        {
            action.sendS2F41(cmd_id, rcmd);
        }
        public void sendCmdRename(string old_name, string new_name, string loc)
        {
            action.sendS2F41_Rename(old_name, new_name, loc);
        }

        Dictionary<string, int> dicTranCmdIndex = new Dictionary<string, int>();
        public string getTranCmdID(string cmdType)
        {
            //System.Threading.SpinWait.SpinUntil(() => false, 500);
            Thread.Sleep(500);
            string cmd_id = string.Empty;
            //if (!dicTranCmdIndex.Keys.Contains(cmdType))
            //{
            //    dicTranCmdIndex.Add(cmdType, 1);
            //    cmd_id = string.Concat("1", cmdType);
            //}
            //else
            //{

            //    cmd_id = string.Concat(++dicTranCmdIndex[cmdType], cmdType);

            //}
            cmd_id = DateTime.Now.ToString("yyyyMMddHHmmssfffff");
            return cmd_id;

        }


        public com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT checkDataValue(string name, string value)
        {
            com.mirle.ibg3k0.stc.Common.SECS.SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT result =
                SECSAgent.SECS_STREAM_FUNCTION_CHECK_RESULT.Recognize;



            return result;
        }
    }
}
