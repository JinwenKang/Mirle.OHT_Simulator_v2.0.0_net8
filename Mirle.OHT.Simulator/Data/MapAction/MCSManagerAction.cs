using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.SECS;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Data.TcpIpData;
using Mirle.Agvc.Simulator.App;
using NLog;
using OverhaedHoistTransporter_Simulator.VO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OverhaedHoistTransporter_Simulator.Action
{
    class MCSManagerAction
    {
        MCSManager mcsManager = null;
        private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static NLog.Logger logger_seccmd = LogManager.GetLogger("SECSCmd");

        public MCSManagerAction( MCSManager _mcs_manager)
        {
            mcsManager = _mcs_manager;
            registeredEvent();
        }

        protected void s6f11Receive(object sender, SECSEventArgs e)
        {
            try
            {
                S6F11 s6f11 = ((S6F11)e.secsHandler.Parse<S6F11>(e));
                S6F12 s6f12 = new S6F12()
                {
                    SystemByte = s6f11.SystemByte,
                    ACKC6 = "0",
                    SECSAgentName = mcsManager.agent.Name
                };
                Task.Run(() =>
                {
                    mcsManager.agent.replySECSMsg(s6f12);
                });
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        protected async Task s6f11ReceiveAsync(object sender, SECSEventArgs e)
        {
            try
            {
                S6F11 s6f11 = ((S6F11)e.secsHandler.Parse<S6F11>(e));
                S6F12 s6f12 = new S6F12()
                {
                    SystemByte = s6f11.SystemByte,
                    ACKC6 = "0",
                    SECSAgentName = mcsManager.agent.Name
                };
                await Task.Run(() => mcsManager.agent.replySECSMsg(s6f12)); // 使用 Task.Run 避免堵塞
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error in s6f11ReceiveAsync");
            }
        }
        protected void s1f1Receive(object sender, SECSEventArgs e)
        {
            try
            {
                S1F1 s1f1 = ((S1F1)e.secsHandler.Parse<S1F1>(e));
                S1F2 s1f2 = new S1F2()
                {
                    MDLN = "",
                    SOFTREV = "",
                    SystemByte = s1f1.SystemByte,
                    SECSAgentName = mcsManager.agent.Name
                };
                mcsManager.agent.replySECSMsg(s1f2);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void sendS2F41(string cmd_id, string rcmd)
        {
            try
            {
                String rtnMsg = string.Empty;
                SXFY abortSecs = null;
                S2F42 s2f42 = null;

                S2F41 s2f41 = new S2F41()
                {
                    RCMD = rcmd,
                    REPITEMS = new S2F41.REPITEM[1]
                   {
                       new S2F41.REPITEM()
                       {
                           CPNAME = "COMMANDID",
                           CPVAL = cmd_id
                       }
                   }
                };
                TrxSECS.ReturnCode rtnCode = mcsManager.agent.TrxSECS.sendRecv<S2F42>(s2f41, out s2f42,
                        out abortSecs, out rtnMsg, null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
        public void sendS2F41_Rename(string old_carrier_id, string new_carrier_id, string carrier_loc)
        {
            try
            {
                String rtnMsg = string.Empty;
                SXFY abortSecs = null;
                S2F42 s2f42 = null;

                S2F41 s2f41 = new S2F41()
                {
                    RCMD = "RENAME",
                    REPITEMS = new S2F41.REPITEM[3]
                   {
                       new S2F41.REPITEM()
                       {
                           CPNAME = "CARRIERID",
                           CPVAL = old_carrier_id
                       },
                       new S2F41.REPITEM()
                       {
                           CPNAME = "NEW-CARRIERID",
                           CPVAL = new_carrier_id
                       },
                       new S2F41.REPITEM()
                       {
                           CPNAME = "CARRIERLOC",
                           CPVAL = carrier_loc
                       },
                   }
                };
                TrxSECS.ReturnCode rtnCode = mcsManager.agent.TrxSECS.sendRecv<S2F42>(s2f41, out s2f42,
                        out abortSecs, out rtnMsg, null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void sendS2F49(string cmd_id, string carrier_id, string source_port, string destn_port)
        {
            try
            {
                String rtnMsg = string.Empty;
                SXFY abortSecs = null;
                S2F50 s2f50 = null;

                S2F49 s2f49 = new S2F49()
                {
                    DATAID = "0",
                    OBJSPEC = "0",
                    RCMD = "TRANSFER",
                    REPITEMS = new S2F49.REPITEM()
                    {
                        COMMINFO = new S2F49.REPITEM.COMM()
                        {
                            COMMANDINFO = "COMMANDINFO",
                            COMMAINFO = new S2F49.REPITEM.COMM.COMMA()
                            {
                                COMMANDIDINFO = new S2F49.REPITEM.COMM.COMMA.COMMANDID()
                                {
                                    COMMANDID1 = "COMMANDID",
                                    CommandID = cmd_id
                                },
                                PRIORITY = new S2F49.REPITEM.COMM.COMMA.PRIO()
                                {
                                    PRIORITY = "PRIORITY",
                                    CommandID = "10"
                                },
                                REPLACE = new S2F49.REPITEM.COMM.COMMA.PRIO()
                                {
                                    PRIORITY = "REPLACE",
                                    CommandID = "0"
                                }
                            }
                        },
                        TRANINFO = new S2F49.REPITEM.TRAN()
                        {
                            TRANSFERINFO = "TRANSFERINFO",
                            CARRINFO = new S2F49.REPITEM.TRAN.CARR()
                            {
                                CARRIERIDINFO = new S2F49.REPITEM.TRAN.CARR.CARRIERID()
                                {
                                    CARRIERID1 = "CARRIERID",
                                    CarrierID = carrier_id
                                },
                                SOUINFO = new S2F49.REPITEM.TRAN.CARR.SOU()
                                {
                                    SOURCE = "SOURCEPORT",
                                    Source = source_port
                                },
                                DESTINFO = new S2F49.REPITEM.TRAN.CARR.DEST()
                                {
                                    DEST1 = "DESTPORT",
                                    Dest = destn_port
                                }
                            }
                        },
                        //CSTINFO = new S2F49.REPITEM.CST()
                        //{
                        //    TRANSFERINFO_CST = "TRANSFERINFO_CST",
                        //    CARRINFO = new S2F49.REPITEM.CST.CARR()
                        //    {
                        //        CARRIERIDINFO = new S2F49.REPITEM.CST.CARR.CASSETTE_SIZE()
                        //        {
                        //            CASSETTE_SIZE_NAME = "CASSETTE_SIZE",
                        //            CASSETTE_SIZE_VALUE = "0"
                        //        },
                        //        SOUINFO = new S2F49.REPITEM.CST.CARR.EMPTYCARRIER()
                        //        {
                        //            GLASS_DATA_NAME = "GLASS_DATA",
                        //            GLASS_DATA_VALUE = "0"
                        //        }
                        //    }
                        //},
                        //STAGEIDLIST = new S2F49.REPITEM.STAGEIDLSIT()
                        //{
                        //    STAGE_ID_LIST_NAME = "STAGE_ID_LIST",
                        //    STAGE_ID_LIST = new string[0]
                        //}
                    }
                };
                TrxSECS.ReturnCode rtnCode = mcsManager.agent.TrxSECS.sendRecv<S2F50>(s2f49, out s2f50,
                        out abortSecs, out rtnMsg, null);

            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }

        public bool sendS2F49GREE(com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER s2f49)
        {
            try
            {
                String rtnMsg = string.Empty;
                SXFY abortSecs = null;
                com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F50 s2f50 = null;

                TrxSECS.ReturnCode rtnCode = mcsManager.agent.TrxSECS.sendRecv<com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F50>(s2f49, out s2f50,
                        out abortSecs, out rtnMsg, null);

                logger_seccmd.Info($"S2F50 Result RtnCode:{rtnCode} S2F50 HCACK:{s2f50.HCACK}");
                if (rtnCode != TrxSECS.ReturnCode.Normal || s2f50 == null)
                {
                   return false;
                }
                else if(s2f50 != null)
                {
                    int hcack = Convert.ToInt32(s2f50.HCACK.Trim(), 16);
                    if(hcack != 4)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return true;
        }

        public async Task<bool> sendS2F49GREEAsync(com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER s2f49)
        {
            try
            {
                String rtnMsg = string.Empty;
                SXFY abortSecs = null;
                com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F50 s2f50 = null;

                // 使用 Task.Run 將 sendRecv 放入背景執行緒
                TrxSECS.ReturnCode rtnCode = await Task.Run(() =>
                    mcsManager.agent.TrxSECS.sendRecv<com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F50>(s2f49, out s2f50, out abortSecs, out rtnMsg, null));

                logger_seccmd.Info($"S2F50 Result RtnCode:{rtnCode} S2F50 HCACK:{s2f50.HCACK}");
                if (rtnCode != TrxSECS.ReturnCode.Normal || s2f50 == null)
                {
                    return false;
                }
                else if (s2f50 != null)
                {
                    int hcack = Convert.ToInt32(s2f50.HCACK.Trim(), 16);
                    if (hcack != 4)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }

            return true;
        }
        void registeredEvent()
        {
            mcsManager.agent.addSECSReceivedHandler("S6F11", (sender, e) => Task.Run(async () => await s6f11ReceiveAsync(sender, e)));
            mcsManager.agent.addSECSReceivedHandler("S1F1", s1f1Receive);
            mcsManager.agent.addSECSReceivedHandler("S1F13", s1f13Receive);

        }

        private void s1f13Receive(object sender, SECSEventArgs e)
        {
            try
            {
                S1F13 s1f13 = ((S1F13)e.secsHandler.Parse<S1F13>(e));
                S1F14 s1f14 = new S1F14()
                {
                    SystemByte = s1f13.SystemByte,
                    SECSAgentName = mcsManager.agent.Name
                };
                mcsManager.agent.replySECSMsg(s1f14);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }
        }
    }
}
