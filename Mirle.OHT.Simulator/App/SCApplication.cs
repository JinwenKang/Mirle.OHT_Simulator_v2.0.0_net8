//*********************************************************************************
//      SCApplication.cs
//*********************************************************************************
// File Name: SCApplication.cs
// Description: SCApplication
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using com.mirle.ibg3k0.sc.BLL;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using com.mirle.ibg3k0.stc.Common.SECS;
using com.mirle.iibg3k0.ttc;
using GenericParsing;
using Mirle.Agvc.Simulator;
using Mirle.Agvc.Simulator.App;
using Nest;
using NLog;
using OverhaedHoistTransporter_Simulator.VO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.App
{
    public class SCApplication
    {
        private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static NLog.Logger logger_roadcontrol = LogManager.GetLogger("RoadControl");

        //private static NLog.Logger logger_seccmd = LogManager.GetLogger("SECSCmd");

        private static Object _lock = new Object();
        private static SCApplication application;

        //config DAO
        private BlockDataDao blockDataDao = null;
        public BlockDataDao BlockDataDao { get { return blockDataDao; } }
        private PortMapDao portMapDao = null;
        public PortMapDao PortMapDao { get { return portMapDao; } }
        private AddressDao addressDao = null;
        public AddressDao AddressDao { get { return addressDao; } }
        private VehicleMapDao vehicleMaoDao = null;
        public VehicleMapDao VehicleMapDao { get { return vehicleMaoDao; } }
        private SectionDao sectionDao = null;
        public SectionDao SectionDao { get { return sectionDao; } }
        private SecsCmdSetDao secsCmdSetDao = null;
        public SecsCmdSetDao SecsCmdSetDao { get { return secsCmdSetDao; } }

        //BLL
        private ConfigDataBLL configDataBLL = null;
        public ConfigDataBLL ConfigDataBLL { get { return configDataBLL; } }

        private DataSet configData = null;
        public DataSet ConfigData { get { return configData; } }

        private List<Address> adrLst;

        public Dictionary<string, MiddleAgent> AgentDic = new Dictionary<string, MiddleAgent>();
        public List<AddressCheck> VhAddressLst = new List<AddressCheck>();  //<VhNo, Address>
        public List<VirAddress> virAdrLst = new List<VirAddress>();

        private LoggerAgent theLoggerAgent;
        public MCSManager mcsManager { get; private set; }

        private bool isJunp = false;

        private bool isDisableLocChk = false;

        private System.Timers.Timer secs_test_timer;
        private List<SecsCmdSet> secsCmdLst = new List<SecsCmdSet>();
        private Dictionary<string, List<CrrCmdData>> crrCmdDic = new Dictionary<string, List<CrrCmdData>>(); 
        private int currIndex = 0;
        private int secCutLength = 0;
        private int vhLength = 0;
        private int vhSensorLength = 0;
        private bool precisePark = false;

        //判斷 Section 是否為直線時，From 與 To 的 X 軸或 Y 軸，相差 10 內都可以判定為直線
        private int isSecStraightBuffer = 100;

        private SCApplication()
        {
            init();
            //initMCSManager();
        }

        public static SCApplication getInstance()
        {
            lock (_lock)
            {
                if (application == null)
                {
                    application = new SCApplication();
                }

                return application;
            }
        }
        private void initMCSManager()
        {
            //string localIP = "192.168.70.99";
            //string remoteIP = "192.168.70.134";
            //int localPort = 17000;
            //int remotePort = 7000;
            // var localipAdr = System.Net.Dns.GetHostAddresses("localhost");
            //string remoteIP = "mcs.ha.ohxc.mirle.com.tw";
            string remoteIP = SystemParameter.SECSRemoteIP;
            string localIP = SystemParameter.SECSLocalIP;
            int localPort = SystemParameter.SECSLocalPort;
            int remotePort = SystemParameter.SECSRemotePort;
            int t3Timeout = 30;
            int t5Timeout = 5;
            int t6Timeout = 50;
            int t7Timeout = 5;
            int t8Timeout = 5;
            SECSAgent.SECS_AGENT_COMM_MODE connectMode = SECSAgent.SECS_AGENT_COMM_MODE.HSMS_ACTIVE_MODE;
            int retryCount = 0;
            string TX_LOG_PATH = @"N,C:\LogFiles\SECSLog\";
            mcsManager = new MCSManager("MCS", 11, localIP, localPort, remoteIP, remotePort, connectMode,
                t3Timeout, t5Timeout, t6Timeout, t7Timeout, t8Timeout, 10, TX_LOG_PATH,
                retryCount, SECSAgent.PROCESS_RECEIVE_ORDER.Normal, null, null);
        }

        public string getLocalIP()
        {
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress addr in localIPs)
            {
                if (addr.AddressFamily == AddressFamily.InterNetwork)
                {
                    return addr.ToString();
                }
            }
            return string.Empty;
        }

        private void init()
        {
            try
            {
                loadBlockDataConfig();

                blockDataDao = new BlockDataDao();
                portMapDao = new PortMapDao();
                addressDao = new AddressDao();
                vehicleMaoDao = new VehicleMapDao();
                sectionDao = new SectionDao();
                secsCmdSetDao = new SecsCmdSetDao();
                configDataBLL = new ConfigDataBLL();

                configDataBLL.start(this);

                theLoggerAgent = LoggerAgent.Instance;

                string localIP = getLocalIP();

                SystemParameter.setMoveStepTime(getInt("Move_Step_Time", 1000));
                SystemParameter.setBCRReadTime(getInt("BCRRead_Time", 10000));
                SystemParameter.setLoadArrivalToLoadingTime(getInt("LoadArrival_To_Loading_Time", 2000));
                SystemParameter.setLoadingToLoadCompleteTime(getInt("Loading_To_LoadComplete_Time", 12000));
                SystemParameter.setUnloadArrivalToUnloadingTime(getInt("UnloadArrival_To_Unloading_Time", 2000));
                SystemParameter.setUnloadingToUnloadCompleteTime(getInt("Unloading_To_UnloadComplete_Time", 12000));
                SystemParameter.setReport144Time(getInt("Report_144_Time", 60000));
                SystemParameter.setReport134Time(getInt("Report_134_Time", 1000));
                SystemParameter.setReport136Time(getInt("Report_136_Step_Time", 1000));
                SystemParameter.setSECSCmdCnt(getInt("Command_Count", 10));
                SystemParameter.setSECSCmdInterval(getInt("Command_Intrerval", 30));
                SystemParameter.setSECSLocalIP(getString("SECSLocalIP", localIP));
                SystemParameter.setSECSRemoteIP(getString("SECSRemoteIP", localIP));
                SystemParameter.setSECSLocalPort(getInt("SECSLocalPort", 5011));
                SystemParameter.setSECSRemotePort(getInt("SECSRemotePort", 5011));
                SystemParameter.setGrpcSendTimeoutTime(getInt("GrpcTimeoutTime", 30));

                secCutLength = getInt("SecCutLength", 1000);
                vhLength = getInt("VehicleLength", 800) + 100;
                isSecStraightBuffer = getInt("SecStraightBuffer", 100);

                vhSensorLength = getInt("VehicleSensorLength", 1000);

                precisePark = getBoolean("PrecisePark", false);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

        }

        public void Start()
        {
            adrLst = configDataBLL.loadAddress();

            if(precisePark)
            {
                createVirtualAddressWithAllLocation();
            }
            else
            {
                createVirtualAddress();
            }
        }

        public void loadBlockDataConfig()
        {
            configData = new DataSet();
            loadCSVToDataTable(configData, "BLOCKDATA");
            loadCSVToDataTable(configData, "PORTMAP");
            loadCSVToDataTable(configData, "AADDRESS");
            loadCSVToDataTable(configData, "VEHICLEMAP");
            loadCSVToDataTable(configData, "ASECTION");
            loadCSVToDataTable(configData, "SECSCMDSET");
        }

        private void loadCSVToDataTable(DataSet ds, string tableName)
        {
            using (GenericParser parser = new GenericParser())
            {
                parser.SetDataSource(Environment.CurrentDirectory + this.getString("CsvConfig", "\\Config\\") + tableName + ".csv");

                parser.ColumnDelimiter = ',';
                parser.FirstRowHasHeader = true;
                parser.MaxBufferSize = 1024;

                DataTable dt = new System.Data.DataTable(tableName);

                bool isfirst = true;
                while (parser.Read())
                {

                    int cs = parser.ColumnCount;
                    if (isfirst)
                    {
                        for (int i = 0; i < cs; i++)
                        {
                            dt.Columns.Add(parser.GetColumnName(i), typeof(string));
                        }
                        isfirst = false;
                    }


                    DataRow dr = dt.NewRow();

                    // 以 dt.Columns.Count 為上限，避免 CSV 資料列尾端多餘欄位
                    // 造成 IndexOutOfRangeException（例如 PORTMAP.csv 某些列有 8 欄但標頭只有 7 欄）
                    int writeCols = Math.Min(cs, dt.Columns.Count);
                    for (int i = 0; i < writeCols; i++)
                    {
                        string val = parser[i];
                        dr[i] = val;
                    }
                    dt.Rows.Add(dr);
                }
                ds.Tables.Add(dt);
            }
        }

        /// <summary>
        /// 讀取 TeachingData CSV 檔案並回傳 List<TeachingData>
        /// </summary>
        /// <param name="filePath">CSV 檔案路徑</param>
        /// <returns>TeachingData 資料列表</returns>
        public List<TeachingData> ReadTeachingDataFromCsv(string filePath)
        {
            List<TeachingData> teachingDataList = new List<TeachingData>();

            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    logger.Error($"檔案不存在: {filePath}");
                    return teachingDataList;
                }

                using (GenericParser parser = new GenericParser())
                {
                    parser.SetDataSource(filePath);
                    parser.ColumnDelimiter = ',';
                    parser.FirstRowHasHeader = true;
                    parser.SkipStartingDataRows = 0;
                    parser.MaxBufferSize = 4096;
                    parser.MaxRows = 500000;
                    parser.TextQualifier = '\"';

                    while (parser.Read())
                    {
                        TeachingData data = new TeachingData
                        {
                            AddressID = parser["AddressID"],
                            Barcode = parser["Barcode"],
                            GrabY = parser["GrabY"],
                            GrabT = parser["GrabT"],
                            GrabZ = parser["GrabZ"],
                            NEEDPIO = parser["NEEDPIO"],
                            PIOID = parser["PIOID"],
                            PIOCHANNEL = parser["PIOCHANNEL"],
                            PIOPORTNUMBER = parser["PIOPORTNUMBER"],
                            StationNumber = parser["StationNumber"],
                            StationSection = parser["StationSection"],
                            POSX = parser["POSX"],
                            POSY = parser["POSY"],
                            SECDIS = parser["SECDIS"],
                            PORTTYPE = parser["PORTTYPE"],
                            AREATYPE = parser["AREATYPE"],
                            TA1TIMEOUT = parser["TA1TIMEOUT"]
                        };

                        teachingDataList.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Exception:");
            }

            return teachingDataList;
        }

        object _updateLock = new object();
        public void updateVhAddress(string vh_id, string address)
        {
            lock (_updateLock)
            {
                try
                {
                    AddressCheck vhAdr = VhAddressLst.Where(v => v.Vh_ID.Trim() == vh_id.Trim()).FirstOrDefault();
                    if (vhAdr == null)
                    {
                        vhAdr = new AddressCheck();
                        vhAdr.Vh_ID = vh_id;
                        vhAdr.Address = address;
                        VhAddressLst.Add(vhAdr);

                        theLoggerAgent.LogMsg("Info", new LogFormat("Info", "1", "Vh:" + "OHT" + vh_id, "Device", "CarrierID", "Pass Addr:" + address));
                    }
                    else
                    {
                        vhAdr.Address = address;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }
        }

        object _checkLock = new object();
        public bool checkVhAddress(string vh_id, uint curr_dis, VirAddress chk_adr, List<string> guideSection, string curr_stage, int dis = int.MaxValue)
        {
            string guideSec = string.Empty;
            int curr_dis_int = int.Parse(curr_dis.ToString());
            if(guideSection != null && guideSection.Count > 0)
            {
                guideSec = string.Join(",", guideSection.ToArray());
            }

            lock (_checkLock)
            {
                try
                {
                    string source_address = string.Empty;
                    AddressCheck vhAdr = VhAddressLst.Where(v => v.Vh_ID.Trim() == vh_id.Trim()).FirstOrDefault();
                    if (vhAdr != null)
                    {
                        source_address = vhAdr.Address;
                    }

                    if(isMatche(chk_adr.Loc_ID, "10101"))
                    {
                        return checkFor10101(vh_id, source_address, curr_stage, guideSec);
                    }

                    //如果檢查的Address已經超過目的地，就不用再檢查上面是否有車
                    //if (chk_adr.Distance > dis && (chk_adr.Distance - dis) > vhLength)
                    //{
                    //    return true;
                    //}

                    //檢查目標位置是否有車
                    if (!chkAdrIsEmpty(chk_adr.Loc_ID, vh_id, source_address, curr_stage, guideSec))
                    {
                        return false;
                    }

                    //檢查後續一個車長+Sensor範圍內是否有車
                    int cur_distance = 0;
                    int nxt_distance = 0;
                    List<string> adrLst = new List<string>();

                    //找出一個車長+Sensor範圍內的Address
                    //掃描所在Section內的Address
                    List<VirAddress> secAdrLst = virAdrLst.Where(v => v.Sec_ID.Trim() == chk_adr.Sec_ID.Trim()).OrderBy(v => v.Seq_No).ToList();
                    int currAdrIndex = secAdrLst.FindIndex(v => v.Seq_No == chk_adr.Seq_No);
                    if(currAdrIndex < secAdrLst.Count - 1)
                    {
                        for (int i = currAdrIndex + 1; i < secAdrLst.Count; i++)
                        {
                            int check_distance = secAdrLst[i].Distance;
                            if(i == (secAdrLst.Count - 1))
                            {
                                Section sec = configDataBLL.getSection(chk_adr.Sec_ID);
                                if (sec != null)
                                {
                                    check_distance = int.Parse(sec.Distance);
                                }
                            }

                            cur_distance = (check_distance - curr_dis_int);
                            if (cur_distance < 0) cur_distance = 0; // 防止跨Section時殘留SecDistance導致負數
                            
                            if (cur_distance < vhSensorLength)
                            {
                                adrLst.Add(secAdrLst[i].Loc_ID);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        // chk_adr 是本 Section 的最後一個地址，直接計算剩餘距離到 Section 末端
                        Section sec = configDataBLL.getSection(chk_adr.Sec_ID);
                        if (sec != null)
                        {
                            cur_distance = int.Parse(sec.Distance) - curr_dis_int;
                            if (cur_distance < 0) cur_distance = 0;
                        }
                    }

                    //掃描下一個Section內的Address
                    if(cur_distance < vhSensorLength)
                    {
                        int currSecIndex = guideSection.FindIndex(s => s.Trim() == chk_adr.Sec_ID.Trim());
                        if(currSecIndex < guideSection.Count - 1)
                        {
                            for(int i = currSecIndex + 1; i < guideSection.Count; i++)
                            {
                                secAdrLst = virAdrLst.Where(v => v.Sec_ID.Trim() == guideSection[i].Trim()).OrderBy(v => v.Seq_No).ToList();
                                bool over_distance = false;
                                // j 從 1 開始，跳過 From-address（Distance=0），
                                // 因為該地址與上一 Section 的最後地址是同一個實體位置，已加入 adrLst
                                for (int j = 1; j < secAdrLst.Count; j++)
                                {
                                    int check_distance = secAdrLst[j].Distance;
                                    if (j == (secAdrLst.Count - 1))
                                    {
                                        Section sec = configDataBLL.getSection(guideSection[i]);
                                        if (sec != null)
                                        {
                                            check_distance = int.Parse(sec.Distance);
                                        }
                                    }

                                    nxt_distance = check_distance + cur_distance;
                                    
                                    if (nxt_distance < vhSensorLength)
                                    {
                                        adrLst.Add(secAdrLst[j].Loc_ID);
                                    }
                                    else
                                    {
                                        over_distance = true;
                                        break;
                                    }

                                    if (j == (secAdrLst.Count - 1))
                                    {
                                        cur_distance += check_distance;
                                    }
                                }

                                if(over_distance)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    //檢查找出的Address有沒有車在上面
                    if(adrLst.Count > 0)
                    {
                        logger_roadcontrol.Info($"Vh:[{vh_id}] at Address:[{source_address}] check Start Address:[{chk_adr.Loc_ID}] check vh length Addrress:[{string.Join(",", adrLst)}] Guide Section:[{guideSec}]");
                        foreach (string adr in adrLst)
                        {
                            if (!chkAdrIsEmpty(adr, vh_id, source_address, curr_stage, guideSec))
                            {
                                return false;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }

            return true;
        }

        private bool chkAdrIsEmpty(string address, string vh_id, string source_address, string curr_stage, string guideSec)
        {
            List<AddressCheck> vhLst = VhAddressLst.Where(v => v.Vh_ID.Trim() != vh_id.Trim() && v.Address.Trim() == address.Trim()).ToList();
            if (vhLst != null && vhLst.Count > 0)
            {
                logger_roadcontrol.Info($"Vh:[{vh_id}] at Address:[{source_address}] is block by Vh:[{vhLst[0].Vh_ID}] at Address:[{address}] Current Stage:[{curr_stage}] Guide Section:[{guideSec}]");
                return false;
            }

            return true;
        }

        private bool checkFor10101(string vh_id, string source_address, string curr_stage, string guideSec)
        {
            List<VirAddress> adrLst = virAdrLst.Where(v => v.Sec_ID.Trim() == "32602").OrderBy(v => v.Seq_No).ToList();
            foreach(VirAddress adr in adrLst)
            {
                if(!chkAdrIsEmpty(adr.Loc_ID, vh_id, source_address, curr_stage, guideSec))
                {
                    return false;
                }
            }

            return true;
        }

        object _deleteLock = new object();
        public void deleteVhAddress(string vh_no)
        {
            lock (_deleteLock)
            {
                try
                {
                    AddressCheck vhAdr = VhAddressLst.Where(v => v.Vh_ID.Trim() == vh_no.Trim()).FirstOrDefault();
                    if (vhAdr != null)
                    {
                        VhAddressLst.Remove(vhAdr);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Exception:");
                }
            }
        }

        private void createVirtualAddress()
        {
            List<Section> secLst = configDataBLL.loadSection();
            List<Address> adrLst = configDataBLL.loadAddress();

            foreach (Section sec in secLst)
            {
                Address from = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == sec.From_Adr.Trim());
                Address to = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == sec.To_Adr.Trim());

                int distance = int.Parse(sec.Distance);
                bool is_x_straight;
                bool is_straight = isStraight(from, to, out is_x_straight);

                //直線Section
                if(is_straight)
                {
                    //Section切割長度小於車長
                    if(secCutLength < vhLength)
                    {
                        setVirtualAddress(sec, from, to, is_x_straight, distance, true);
                    }
                    //Section切割長度大於車長
                    else
                    {
                        if (distance > (secCutLength * 2))
                        {
                            setVirtualAddress(sec, from, to, is_x_straight, distance, false);
                        }
                        else if (distance <= (vhLength + 25))
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                        }
                        else if (distance > (vhLength + 25) && distance < (secCutLength * 2))
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst, true);
                        }
                        else
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                        }
                    }
                }
                //彎道Section
                else
                {
                    setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                }

                logAddress(sec.ID);
            }
        }

        private void setbuffer_25(bool is_straight, bool is_x_straight, Address from, Address to, Section sec, List<Section> secLst, bool is_for_short_sec = false)
        {
            int seq = 1;

            //From Address
            VirAddress virAdrFrom = new VirAddress();
            virAdrFrom.Loc_ID = from.Adr_ID;
            virAdrFrom.Sec_ID = sec.ID;
            virAdrFrom.Is_Virtual = false;
            virAdrFrom.Seq_No = seq;
            virAdrFrom.POS_X = from.POS_X;
            virAdrFrom.POS_Y = from.POS_Y;
            virAdrFrom.Distance = 0;

            seq++;

            //Virtual Address
            VirAddress virAdr = new VirAddress();
            virAdr.Loc_ID = sec.ID + "_001";
            virAdr.Sec_ID = sec.ID;
            virAdr.Is_Virtual = true;
            virAdr.Seq_No = seq;
            virAdr.Distance = 25;

            seq++;

            if (is_straight)
            {
                setVirAdrXY(is_x_straight, from, to, ref virAdr);
            }
            else
            {
                Section pre_sec = secLst.Where(s => s.To_Adr.Trim() == sec.From_Adr.Trim()).FirstOrDefault();
                bool has_set = false;
                if (pre_sec != null)
                {
                    Address pre_from = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == pre_sec.From_Adr.Trim());
                    Address pre_to = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == pre_sec.To_Adr.Trim());

                    is_straight = isStraight(pre_from, pre_to, out is_x_straight);
                    if (is_straight)
                    {
                        setVirAdrXY(is_x_straight, from, to, ref virAdr);
                        has_set = true;
                    }
                }

                if (!has_set)
                {
                    setVirAdrXY(true, from, to, ref virAdr);
                }
            }
            
            if (is_for_short_sec)
            {
                setbufferForShortSection(is_straight, is_x_straight, from, to, sec, secLst, ref seq);
            }

            //To Address
            VirAddress virAdrTo = new VirAddress();
            virAdrTo.Loc_ID = to.Adr_ID;
            virAdrTo.Sec_ID = sec.ID;
            virAdrTo.Is_Virtual = false;
            virAdrTo.Seq_No = seq;
            virAdrTo.POS_X = to.POS_X;
            virAdrTo.POS_Y = to.POS_Y;
            virAdrTo.Distance = int.Parse(sec.Distance); // TO address 距離應為 section 總長，而非 0

            virAdrLst.Add(virAdrFrom);
            virAdrLst.Add(virAdr);
            virAdrLst.Add(virAdrTo);
        }

        private void setbufferForShortSection(bool is_straight, bool is_x_straight, Address from, Address to, Section sec, List<Section> secLst, ref int seq)
        {
            //Virtual Address
            VirAddress virAdrForShortSec = new VirAddress();
            virAdrForShortSec.Loc_ID = sec.ID + "_002";
            virAdrForShortSec.Sec_ID = sec.ID;
            virAdrForShortSec.Is_Virtual = true;
            virAdrForShortSec.Seq_No = seq;
            virAdrForShortSec.Distance = int.Parse(sec.Distance) - vhLength;

            seq++;

            if (is_straight)
            {
                setVirAdrXYForShort(is_x_straight, from, to, ref virAdrForShortSec);
            }
            else
            {
                Section pre_sec = secLst.Where(s => s.To_Adr.Trim() == sec.From_Adr.Trim()).FirstOrDefault();
                bool has_set = false;
                if (pre_sec != null)
                {
                    Address pre_from = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == pre_sec.From_Adr.Trim());
                    Address pre_to = adrLst.FirstOrDefault(a => a.Adr_ID.Trim() == pre_sec.To_Adr.Trim());

                    is_straight = isStraight(pre_from, pre_to, out is_x_straight);
                    if (is_straight)
                    {
                        setVirAdrXYForShort(is_x_straight, from, to, ref virAdrForShortSec);
                        has_set = true;
                    }
                }
                if (!has_set)
                {
                    setVirAdrXYForShort(true, from, to, ref virAdrForShortSec);
                }
            }
            virAdrLst.Add(virAdrForShortSec);
        }
        private void setVirAdrXYForShort(bool is_x_straight, Address from, Address to, ref VirAddress virAdr) {
            int from_axis = 0;
            int to_axis = 0;
            if (is_x_straight)
            {
                from_axis = int.Parse(from.POS_Y);
                to_axis = int.Parse(to.POS_Y);
            }
            else
            {
                from_axis = int.Parse(from.POS_X);
                to_axis = int.Parse(to.POS_X);
            }

            int axis = 0;
            if (to_axis > from_axis)
            {
                axis = to_axis - vhLength;
            }
            else
            {
                axis = to_axis + vhLength;
            }

            if (is_x_straight)
            {
                virAdr.POS_X = from.POS_X;
                virAdr.POS_Y = axis.ToString();
            }
            else
            {
                virAdr.POS_Y = from.POS_Y;
                virAdr.POS_X = axis.ToString();
            }
        }

        

        private void setVirAdrXY(bool is_x_straight, Address from, Address to, ref VirAddress virAdr)
        {
            int from_axis = 0;
            int to_axis = 0;
            if (is_x_straight)
            {
                from_axis = int.Parse(from.POS_Y);
                to_axis = int.Parse(to.POS_Y);
            }
            else
            {
                from_axis = int.Parse(from.POS_X);
                to_axis = int.Parse(to.POS_X);
            }

            int axis = 0;
            if (to_axis > from_axis)
            {
                axis = from_axis + 25;
            }
            else
            {
                axis = from_axis - 25;
            }

            if (is_x_straight)
            {
                virAdr.POS_X = from.POS_X;
                virAdr.POS_Y = axis.ToString();
            }
            else
            {
                virAdr.POS_Y = from.POS_Y;
                virAdr.POS_X = axis.ToString();
            }
        }

        private void logAddress(string sec)
        {
            List<VirAddress> tmpLst = virAdrLst.Where(v => v.Sec_ID.Trim() == sec.Trim()).OrderBy(v => v.Seq_No).ToList();

            StringBuilder sb = new StringBuilder();
            bool is_first = true;
            foreach (VirAddress adr in tmpLst)
            {
                if (is_first)
                {
                    sb.Append($"Address:[{adr.Loc_ID}] Distance:[{adr.Distance.ToString()}] X,Y:[{adr.POS_X},{adr.POS_Y}]");
                    is_first = false;
                }
                else
                {
                    sb.Append($" -> Address:[{adr.Loc_ID}] Distance:[{adr.Distance.ToString()}] X,Y:[{adr.POS_X},{adr.POS_Y}]");
                }
            }

            logger_roadcontrol.Info($"Sec:[{sec}] {sb.ToString()}");
        }

        private void setVirtualAddress(Section sec, Address from, Address to, bool is_x_straight, int distance, bool is_sort_cut_length)
        {
            int index = 1;
            int from_axis = 0;
            int to_axis = 0;
            if (is_x_straight)
            {
                from_axis = int.Parse(from.POS_Y);
                to_axis = int.Parse(to.POS_Y);
            }
            else
            {
                from_axis = int.Parse(from.POS_X);
                to_axis = int.Parse(to.POS_X);
            }

            VirAddress virAdrFrom = new VirAddress();
            virAdrFrom.Loc_ID = from.Adr_ID;
            virAdrFrom.Sec_ID = sec.ID;
            virAdrFrom.Is_Virtual = false;
            virAdrFrom.Seq_No = index;
            virAdrFrom.POS_X = from.POS_X;
            virAdrFrom.POS_Y = from.POS_Y;
            virAdrFrom.Distance = 0;
            virAdrLst.Add(virAdrFrom);
            index++;

            //int vir_adr_cnt = (distance / secCutLength) - 1;
            int vir_adr_cnt = (distance / secCutLength);

            for (int i = 0; i < vir_adr_cnt; i++)
            {
                VirAddress virAdr = new VirAddress();
                virAdr.Loc_ID = sec.ID + "_" + i.ToString().PadLeft(3, '0');
                virAdr.Sec_ID = sec.ID;
                virAdr.Is_Virtual = true;
                virAdr.Seq_No = index;

                int virDistance = i * secCutLength;
                if(i == 0)
                {
                    virDistance = 25;
                }
                else if(i == (vir_adr_cnt - 1))
                {
                    if(!is_sort_cut_length)
                    {
                        virDistance = distance - vhLength;
                    }
                }

                virAdr.Distance = virDistance;

                int axis = 0;
                if (to_axis > from_axis)
                {
                    axis = from_axis + virDistance;
                }
                else
                {
                    axis = from_axis - virDistance;
                }

                if (is_x_straight)
                {
                    virAdr.POS_X = from.POS_X;
                    virAdr.POS_Y = axis.ToString();
                }
                else
                {
                    virAdr.POS_Y = from.POS_Y;
                    virAdr.POS_X = axis.ToString();
                }

                virAdrLst.Add(virAdr);
                index++;
            }

            VirAddress virAdrTo = new VirAddress();
            virAdrTo.Loc_ID = to.Adr_ID;
            virAdrTo.Sec_ID = sec.ID;
            virAdrTo.Is_Virtual = false;
            virAdrTo.Seq_No = index;
            virAdrTo.POS_X = to.POS_X;
            virAdrTo.POS_Y = to.POS_Y;
            virAdrTo.Distance = 0;
            virAdrLst.Add(virAdrTo);
        }

        public bool isStraight(Address from, Address to, out bool is_x_straight)
        {
            is_x_straight = false;

            int from_x = int.Parse(from.POS_X);
            int from_y = int.Parse(from.POS_Y);
            int to_x = int.Parse(to.POS_X);
            int to_y = int.Parse(to.POS_Y);

            //if (from_x == to_x)
            if(Math.Abs(from_x - to_x) < isSecStraightBuffer)
            {
                is_x_straight = true;

                return true;
            }
            //else if (from_y == to_y)
            else if(Math.Abs(from_y - to_y) < isSecStraightBuffer)
            {
                return true;
            }

            return false;
        }

        private void createVirtualAddressWithAllLocation()
        {
            List<Section> secLst = configDataBLL.loadSection();
            
            setVirtualAddress(secLst);

            List<VirAddress> virAdrLstTmp = new List<VirAddress>();

            foreach (Section sec in secLst)
            {
                List<Address> adrLst = configDataBLL.loadParkAddressBySec(sec.ID);
                List<PortMap> portMapLst = configDataBLL.loadPortMapBySecID(sec.ID);
                List<VirAddress> secVirAdrLst = virAdrLst.Where(v => v.Sec_ID.Trim() == sec.ID.Trim()).OrderBy(v => v.Seq_No).ToList();

                // 取得 secVirAdrLst 裡面的最後一個物件
                VirAddress lastVirAdr = secVirAdrLst.LastOrDefault();

                // 如果存在，則將其從 List 中移除
                if (lastVirAdr != null)
                {
                    secVirAdrLst.Remove(lastVirAdr);
                }

                //加入每一個Control Address
                if(adrLst != null && adrLst.Count > 0)
                {
                    foreach (Address adr in adrLst)
                    {
                        VirAddress virAdr = new VirAddress();
                        virAdr.Loc_ID = adr.Adr_ID;
                        virAdr.Sec_ID = sec.ID;
                        virAdr.Is_Virtual = false;
                        virAdr.Seq_No = 0;
                        virAdr.POS_X = adr.POS_X;
                        virAdr.POS_Y = adr.POS_Y;
                        virAdr.Distance = int.Parse(adr.Distance);

                        secVirAdrLst.Add(virAdr);
                    }
                }
                
                //加入每一個EQ Port、OHB Port
                if(portMapLst != null && portMapLst.Count > 0)
                {
                    foreach (PortMap port in portMapLst)
                    {
                        VirAddress virAdr = new VirAddress();
                        virAdr.Loc_ID = port.StationID;
                        virAdr.Sec_ID = sec.ID;
                        virAdr.Is_Virtual = false;
                        virAdr.Seq_No = 0;
                        virAdr.POS_X = port.POS_X;
                        virAdr.POS_Y = port.POS_Y;
                        virAdr.Distance = int.Parse(port.SEC_DIS_FROM_HEAD);

                        secVirAdrLst.Add(virAdr);
                    }
                }

                //重新排序Seq_No
                secVirAdrLst = secVirAdrLst.OrderBy(v => v.Distance).ToList();
                int index = 0;
                foreach(VirAddress virAdr in secVirAdrLst)
                {
                    virAdr.Seq_No = index;
                    index++;
                }

                //加入最後一個Address
                lastVirAdr.Seq_No = index;
                secVirAdrLst.Add(lastVirAdr);

                //合併回暫存總清單
                virAdrLstTmp.AddRange(secVirAdrLst);
            }

            //設定回全域清單
            virAdrLst = virAdrLstTmp;

            //記錄Log
            foreach (Section sec in secLst)
            {
                logAddress(sec.ID);
            }  
        }

        private void setVirtualAddress(List<Section> secLst)
        {
            if (secLst == null || secLst.Count == 0)
            {
                logger.Warn("setVirtualAddress: secLst is null or empty. Skip virtual address generation.");
                return;
            }

            List<Address> adrLst = configDataBLL.loadAddress() ?? new List<Address>();

            foreach (Section sec in secLst)
            {
                if (sec == null) continue;

                Address from = adrLst.FirstOrDefault(a => a != null && a.Adr_ID.Trim() == sec.From_Adr.Trim());
                Address to = adrLst.FirstOrDefault(a => a != null && a.Adr_ID.Trim() == sec.To_Adr.Trim());

                if (from == null || to == null)
                {
                    logger.Warn($"setVirtualAddress: Section [{sec.ID}] From=[{sec.From_Adr}] To=[{sec.To_Adr}] — address not found in AADDRESS, skip.");
                    continue;
                }

                int distance = int.Parse(sec.Distance);
                bool is_x_straight;
                bool is_straight = isStraight(from, to, out is_x_straight);

                //直線Section
                if (is_straight)
                {
                    //Section切割長度小於車長
                    if (secCutLength < vhLength)
                    {
                        setVirtualAddress(sec, from, to, is_x_straight, distance, true);
                    }
                    //Section切割長度大於車長
                    else
                    {
                        if (distance > (secCutLength * 2))
                        {
                            setVirtualAddress(sec, from, to, is_x_straight, distance, false);
                        }
                        else if (distance <= (vhLength + 25))
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                        }
                        else if (distance > (vhLength + 25) && distance < (secCutLength * 2))
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst, true);
                        }
                        else
                        {
                            setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                        }
                    }
                }
                //彎道Section
                else
                {
                    setbuffer_25(is_straight, is_x_straight, from, to, sec, secLst);
                }
            }
        }

        #region 舊方法
        //private void setVirtualAddress(Section sec, Address from, Address to, bool is_x_straight, int distance, int vhLength)
        //{
        //    int index = 1;
        //    int from_axis = 0;
        //    int to_axis = 0;
        //    if (is_x_straight)
        //    {
        //        from_axis = int.Parse(from.POS_Y);
        //        to_axis = int.Parse(to.POS_Y);
        //    }
        //    else
        //    {
        //        from_axis = int.Parse(from.POS_X);
        //        to_axis = int.Parse(to.POS_X);
        //    }

        //    List<VirAddress> portParkAdrLst = setSecPortPark(sec.ID);

        //    VirAddress virAdrFrom = new VirAddress();
        //    virAdrFrom.Loc_ID = from.Adr_ID;
        //    virAdrFrom.Sec_ID = sec.ID;
        //    virAdrFrom.Is_Virtual = false;
        //    virAdrFrom.Seq_No = index;
        //    virAdrFrom.POS_X = from.POS_X;
        //    virAdrFrom.POS_Y = from.POS_Y;
        //    virAdrFrom.Distance = 0;
        //    virAdrLst.Add(virAdrFrom);
        //    index++;

        //    if (portParkAdrLst == null || portParkAdrLst.Count == 0)
        //    {
        //        int vir_adr_cnt = (distance / vhLength) - 1;

        //        for (int i = 0; i < vir_adr_cnt; i++)
        //        {
        //            VirAddress virAdr = new VirAddress();
        //            virAdr.Loc_ID = sec.ID + "_" + i.ToString().PadLeft(3, '0');
        //            virAdr.Sec_ID = sec.ID;
        //            virAdr.Is_Virtual = true;
        //            virAdr.Seq_No = index;

        //            int virDistance = (i + 1) * vhLength;
        //            virAdr.Distance = virDistance;

        //            int axis = 0;
        //            if (to_axis > from_axis)
        //            {
        //                axis = from_axis + virDistance;
        //            }
        //            else
        //            {
        //                axis = from_axis - virDistance;
        //            }

        //            if (is_x_straight)
        //            {
        //                virAdr.POS_X = from.POS_X;
        //                virAdr.POS_Y = axis.ToString();
        //            }
        //            else
        //            {
        //                virAdr.POS_Y = from.POS_Y;
        //                virAdr.POS_X = axis.ToString();
        //            }

        //            virAdrLst.Add(virAdr);
        //            index++;
        //        }
        //    }
        //    else
        //    {
        //        portParkAdrLst = portParkAdrLst.OrderBy(p => p.Distance).ToList();
        //        int first_dis = portParkAdrLst[0].Distance;
        //        int virtual_index = 0;
        //        if (first_dis > (vhLength * 2))
        //        {
        //            int vir_adr_cnt = (first_dis / vhLength) - 1;
        //            for (int i = 0; i < vir_adr_cnt; i++)
        //            {
        //                VirAddress virAdr = new VirAddress();
        //                virAdr.Loc_ID = sec.ID + "_" + virtual_index.ToString().PadLeft(3, '0');
        //                virAdr.Sec_ID = sec.ID;
        //                virAdr.Is_Virtual = true;
        //                virAdr.Seq_No = index;

        //                int virDistance = (i + 1) * vhLength;
        //                virAdr.Distance = virDistance;

        //                int axis = 0;
        //                if (to_axis > from_axis)
        //                {
        //                    axis = from_axis + virDistance;
        //                }
        //                else
        //                {
        //                    axis = from_axis - virDistance;
        //                }

        //                if (is_x_straight)
        //                {
        //                    virAdr.POS_X = from.POS_X;
        //                    virAdr.POS_Y = axis.ToString();
        //                }
        //                else
        //                {
        //                    virAdr.POS_Y = from.POS_Y;
        //                    virAdr.POS_X = axis.ToString();
        //                }

        //                virAdrLst.Add(virAdr);
        //                index++;
        //                virtual_index++;
        //            }
        //        }

        //        VirAddress lastAdr = new VirAddress();
        //        for (int i = 0; i < portParkAdrLst.Count; i++)
        //        {
        //            portParkAdrLst[i].Seq_No = index;
        //            virAdrLst.Add(portParkAdrLst[i]);
        //            index++;

        //            lastAdr = portParkAdrLst[i];
        //        }

        //        if ((distance - lastAdr.Distance) > (vhLength * 2))
        //        {
        //            int vir_adr_cnt = (((distance - lastAdr.Distance) / vhLength) - 1);
        //            for (int i = 0; i < vir_adr_cnt; i++)
        //            {
        //                VirAddress virAdr = new VirAddress();
        //                virAdr.Loc_ID = sec.ID + "_" + virtual_index.ToString().PadLeft(3, '0');
        //                virAdr.Sec_ID = sec.ID;
        //                virAdr.Is_Virtual = true;
        //                virAdr.Seq_No = index;

        //                int virDistance = (i + 1) * vhLength;
        //                virAdr.Distance = lastAdr.Distance + virDistance;

        //                int axis = 0;
        //                if (to_axis > from_axis)
        //                {
        //                    axis = from_axis + lastAdr.Distance + virDistance;
        //                }
        //                else
        //                {
        //                    axis = from_axis - lastAdr.Distance - virDistance;
        //                }

        //                if (is_x_straight)
        //                {
        //                    virAdr.POS_X = from.POS_X;
        //                    virAdr.POS_Y = axis.ToString();
        //                }
        //                else
        //                {
        //                    virAdr.POS_Y = from.POS_Y;
        //                    virAdr.POS_X = axis.ToString();
        //                }

        //                virAdrLst.Add(virAdr);
        //                index++;
        //                virtual_index++;
        //            }
        //        }

        //    }

        //    VirAddress virAdrTo = new VirAddress();
        //    virAdrTo.Loc_ID = to.Adr_ID;
        //    virAdrTo.Sec_ID = sec.ID;
        //    virAdrTo.Is_Virtual = false;
        //    virAdrTo.Seq_No = index;
        //    virAdrTo.POS_X = to.POS_X;
        //    virAdrTo.POS_Y = to.POS_Y;
        //    virAdrTo.Distance = int.Parse(sec.Distance);
        //    virAdrLst.Add(virAdrTo);
        //}

        //private List<VirAddress> setSecPortPark(string sec_id)
        //{
        //    List<PortMap> portMapLst = configDataBLL.loadPortMapBySecID(sec_id);
        //    List<Address> adrLst = configDataBLL.loadParkAddressBySec(sec_id);
        //    List<VirAddress> virAdrLst = new List<VirAddress>();

        //    if (portMapLst != null && portMapLst.Count > 0)
        //    {
        //        for (int i = 0; i < portMapLst.Count; i++)
        //        {
        //            VirAddress virAdr = new VirAddress();
        //            virAdr.Loc_ID = portMapLst[i].StationID;
        //            virAdr.Sec_ID = sec_id;
        //            virAdr.Is_Virtual = false;
        //            virAdr.Seq_No = 0;
        //            virAdr.POS_X = portMapLst[i].POS_X;
        //            virAdr.POS_Y = portMapLst[i].POS_Y;
        //            virAdr.Distance = int.Parse(portMapLst[i].SEC_DIS_FROM_HEAD);
        //            virAdrLst.Add(virAdr);
        //        }
        //    }

        //    if (adrLst != null && adrLst.Count > 0)
        //    {
        //        for (int i = 0; i < adrLst.Count; i++)
        //        {
        //            VirAddress virAdr = new VirAddress();
        //            virAdr.Loc_ID = adrLst[i].Adr_ID;
        //            virAdr.Sec_ID = sec_id;
        //            virAdr.Is_Virtual = false;
        //            virAdr.Seq_No = 0;
        //            virAdr.POS_X = adrLst[i].POS_X;
        //            virAdr.POS_Y = adrLst[i].POS_Y;
        //            virAdr.Distance = int.Parse(adrLst[i].Distance);
        //            virAdrLst.Add(virAdr);
        //        }
        //    }

        //    return virAdrLst;
        //}
        #endregion

        public Address getAdrByID(string id)
        {
            return adrLst.Where(a => a.Adr_ID.Trim() == id.Trim()).FirstOrDefault();
        }

        public void setIsJumpToTarget(bool set)
        {
            isJunp = set;
        }

        public bool getIsJumpToTarget()
        {
            return isJunp;
        }

        public void setDisableLocCheck(bool set)
        {
            isDisableLocChk = set;
        }

        public bool getDisableLocCheck()
        {
            return isDisableLocChk;
        }

        public void setOHTCReplyDelayTime(int set)
        {
            SystemParameter.setOHTCReplyDelayTime(set);
        }

        public void setSecsTestTimer(bool start)
        {
            if(start)
            {
                if (secs_test_timer != null)
                {
                    secs_test_timer.Stop();
                    secs_test_timer = null;
                }

                //secsCmdLst = configDataBLL.loadSecsCmdSet();
                setSecsCmdInitialData();
                currIndex = 0;

                // 故意 fire-and-forget：啟動第一次發送，後續由 timer 觸發。
                _ = sendCommand();

                secs_test_timer = new System.Timers.Timer(SystemParameter.SECSCmdInterval);
                secs_test_timer.Elapsed += new System.Timers.ElapsedEventHandler(secsTestTimer_Elapsed);
                secs_test_timer.Start();
            }
            else
            {
                if (secs_test_timer != null)
                {
                    secs_test_timer.Stop();
                    secs_test_timer = null;
                }
            }
        }

        private void setSecsCmdInitialData()
        {
            try
            {
                secsCmdLst = configDataBLL.loadSecsCmdSet();

                foreach (SecsCmdSet cmd in secsCmdLst)
                {
                    CrrCmdData data = new CrrCmdData();
                    data.CurSend = false;
                    data.Carrier = cmd.Carrier;
                    data.Source = cmd.Source;
                    data.Target = cmd.Target;
                    data.RetryCnt = 0;

                    if (crrCmdDic.Keys.Contains(cmd.Carrier))
                    {
                        crrCmdDic[cmd.Carrier].Add(data);
                    }
                    else
                    {
                        data.CurSend = true;
                        crrCmdDic.Add(cmd.Carrier, new List<CrrCmdData>() { data });
                    }
                }

                //foreach (var kvp in crrCmdDic)
                //{
                //    StringBuilder sb = new StringBuilder();
                //    sb.Append($"Carrier: {kvp.Key} ");
                //    foreach (CrrCmdData data in kvp.Value)
                //    {
                //        sb.Append($"(Source: {data.Source}, Destination: {data.Target}) ");
                //    }

                //    logger_seccmd.Info(sb.ToString());
                //}
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public CrrCmdData getNextCmd(string crr_id)
        {
            CrrCmdData rtn_data = null;

            try
            {
                if (crrCmdDic.Keys.Contains(crr_id))
                {
                    foreach (CrrCmdData data in crrCmdDic[crr_id])
                    {
                        if (data.CurSend)
                        {
                            rtn_data = data;
                            break;
                        }
                    }

                    if (rtn_data == null)
                    {
                        crrCmdDic[crr_id][0].CurSend = true;
                        rtn_data = crrCmdDic[crr_id][0];
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            //if(rtn_data != null)
            //{
            //    logger_seccmd.Info($"getNextCmd Crr ID:{crr_id} Next Source:{rtn_data.Source} Target:{rtn_data.Target}");
            //}
            //else
            //{
            //    logger_seccmd.Info($"getNextCmd Crr ID:{crr_id} Next is Null");
            //}

            return rtn_data;
        }

        public void setCmdSendFail(string crr_id)
        {
            try
            {
                if (crrCmdDic.Keys.Contains(crr_id))
                {
                    bool is_found = false;
                    foreach (CrrCmdData data in crrCmdDic[crr_id])
                    {
                        if (data.CurSend)
                        {
                            is_found = true;

                            if (data.RetryCnt > 2)
                            {
                                //logger_seccmd.Info($"setCmdSendFail Crr ID:{crr_id} Retry Count:{data.RetryCnt.ToString()} > 2, Set Next");
                                setNextCmd(crr_id);
                            }
                            else
                            {
                                data.RetryCnt++;
                                //logger_seccmd.Info($"setCmdSendFail Crr ID:{crr_id} Retry Count:{data.RetryCnt.ToString()}");
                            }

                            break;
                        }
                    }

                    if(!is_found)
                    {
                        //logger_seccmd.Info($"setCmdSendFail Crr ID:{crr_id} Not Found, Set Next");
                        setNextCmd(crr_id);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        public void setNextCmd(string crr_id)
        {
            try
            {
                if (crrCmdDic.Keys.Contains(crr_id))
                {
                    bool is_found = false;
                    for(int i = 0; i < crrCmdDic[crr_id].Count; i++)
                    {
                        if (crrCmdDic[crr_id][i].CurSend)
                        {
                            crrCmdDic[crr_id][i].CurSend = false;
                            crrCmdDic[crr_id][i].RetryCnt = 0;

                            if(i == (crrCmdDic[crr_id].Count - 1))
                            {
                                crrCmdDic[crr_id][0].CurSend = true;

                                //logger_seccmd.Info($"setNextCmd Crr ID:{crr_id} Set Next Source:{crrCmdDic[crr_id][0].Source} Target:{crrCmdDic[crr_id][0].Target}");
                            }
                            else
                            {
                                crrCmdDic[crr_id][i + 1].CurSend = true;

                                //logger_seccmd.Info($"setNextCmd Crr ID:{crr_id} Set Next Source:{crrCmdDic[crr_id][i + 1].Source} Target:{crrCmdDic[crr_id][i + 1].Target}");
                            }

                            is_found = true;
                            break;
                        }
                    }

                    if(!is_found)
                    {
                        crrCmdDic[crr_id][0].CurSend = true;
                        crrCmdDic[crr_id][0].RetryCnt = 0;

                        //logger_seccmd.Info($"setNextCmd Crr ID:{crr_id} Not Found, Set Next Source:{crrCmdDic[crr_id][0].Source} Target:{crrCmdDic[crr_id][0].Target}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void secsTestTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // 故意 fire-and-forget：Timer callback 不適合 await，於背景執行即可。
            _ = sendCommand();
        }

        private async Task sendCommand()
        {
            try
            {
                List<com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER> s2f49Lst = new List<com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER>();

                for (int i = 0; i < SystemParameter.SECSCmdCnt && currIndex < secsCmdLst.Count; i++)
                {
                    string cmd_id = "S2F49Cycle" + DateTime.Now.ToString(SCAppConstants.TimestampFormat_12) + i;

                    //s2f49Lst.Add(setS2F49(cmd_id, secsCmdLst[currIndex].Carrier, secsCmdLst[currIndex].Source, secsCmdLst[currIndex].Target));
                    CrrCmdData data = getNextCmd(secsCmdLst[currIndex].Carrier);
                    if (data != null)
                    {
                        s2f49Lst.Add(setS2F49(cmd_id, data.Carrier, data.Source, data.Target));
                    }
                    else
                    {
                        s2f49Lst.Add(setS2F49(cmd_id, secsCmdLst[currIndex].Carrier, secsCmdLst[currIndex].Source, secsCmdLst[currIndex].Target));
                    }

                    currIndex++;
                }

                List<Task> tasks = new List<Task>();
                for (int i = 0; i < s2f49Lst.Count; i++)
                {
                    int capturedIndex = i; // 捕獲當前的索引
                    //tasks.Add(Task.Run(() => mcsManager.sendTranCmd(s2f49Lst[capturedIndex])));
                    tasks.Add(mcsManager.sendTranCmdAsync(s2f49Lst[capturedIndex])); // 修改呼叫
                }

                if (currIndex >= secsCmdLst.Count)
                {
                    currIndex = 0;
                }

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }
        private com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER setS2F49(string cmd_id, string crr_id, string source_port, string dest_port)
        {
            com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER s2f49 = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER()
            {
                DATAID = "0",
                RCMD = "TRANSFER",
                REPITEMS = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM()
                {
                    COMMINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.COMM()
                    {
                        COMMANDINFONAME = "COMMANDINFO",
                        COMMAINFOVALUE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.COMM.COMMVALUE()
                        {
                            COMMANDID = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "COMMANDID",
                                CPVAL = cmd_id
                            },
                            PRIORITY = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_U2()
                            {
                                CPNAME = "PRIORITY",
                                CPVAL = "50"
                            },
                            REPLACE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_U2()
                            {
                                CPNAME = "REPLACE",
                                CPVAL = "0"
                            },
                        }
                    },
                    TRANINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.TRAN()
                    {
                        TRANSFERINFONAME = "TRANSFERINFO",
                        TRANSFERINFOVALUE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.TRAN.TRANVALUE()
                        {
                            CARRIERIDINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "CARRIERID",
                                CPVAL = crr_id
                            },
                            TRAYSLOTNUM = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "TRAYSLOTNUM",
                                CPVAL = 10.ToString()
                            },
                            TRAYCOLOR = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "TRAYCOLOR",
                                CPVAL = "RED"
                            },
                            SOUINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "SOURCE",
                                CPVAL = source_port
                            },                            
                            DESTINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "DEST",
                                CPVAL = dest_port
                            },
                        }
                    },
                    TRAN_CSTINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.TRAN_CST()
                    {
                        TRAN_CSTNAME = "TRANSFERINFO_CST",
                        TRAN_CSTVALUE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.TRAN_CST_VALUE()
                        {
                            LOTINFOVALUE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "LOT_ID",
                                CPVAL = ""
                            },
                            CARRIERTYPE = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.CP_ASCII()
                            {
                                CPNAME = "CARRIER_TYPE",
                                CPVAL = ""
                            }
                        }
                    },
                    STAGEINFO = new com.mirle.ibg3k0.sc.Data.SECS.OHTC.VISIONOX.S2F49_TRANSFER.REPITEM.STAGE()
                    {
                        STAGEIDLIST = "STAGEIDLIST",
                        STAGEINFOVALUE = new string[]
                            {
                            }
                    }
                }
            };

            return s2f49;
        }


        private int getInt(string key, int defaultValue)
        {
            int rtn = defaultValue;
            try
            {
                string setting_value = ConfigurationManager.AppSettings.Get(key);
                if (!isEmpty(setting_value))
                {
                    rtn = Convert.ToInt32(setting_value);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private long getLong(string key, long defaultValue)
        {
            long rtn = defaultValue;
            try
            {
                string setting_value = ConfigurationManager.AppSettings.Get(key);
                if (!isEmpty(setting_value))
                {
                    rtn = long.Parse(setting_value);
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        private string getString(string key, string defaultValue)
        {
            string rtn = defaultValue;
            try
            {
                rtn = ConfigurationManager.AppSettings.Get(key);
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        public Boolean getBoolean(string key, Boolean defaultValue)
        {
            Boolean rtn = defaultValue;
            try
            {
                string val = ConfigurationManager.AppSettings.Get(key);
                if (val != null)
                {
                    if (isMatche(val, "Y"))
                    {
                        rtn = true;
                    }
                    else
                    {
                        rtn = false;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Warn("Get Config error[key:{0}][Exception:{1}]", key, e);
            }
            return rtn;
        }

        /// <summary>
        /// 表示兩個物件內容是否相同
        /// </summary>
        /// <param name="obj1">The obj1.</param>
        /// <param name="obj2">The obj2.</param>
        /// <returns>Boolean.</returns>
        public Boolean isMatche(Object obj1, Object obj2)
        {
            try
            {
                if (obj1 == obj2) return true;
                if (obj1 == null || obj2 == null) return false;
                if (obj1.GetType().BaseType == typeof(Array))
                {
                    return isMatche((Array)obj1, (Array)obj2);
                }
                if (obj1.GetType().BaseType == typeof(Enum))
                {
                    return (Enum.Equals(obj1, obj2));
                }
                return isMatche(obj1.ToString(), obj2.ToString());
            }
            catch (Exception e)
            {
                logger.Warn("isMatche Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// 比較兩個陣列內容是否相同
        /// </summary>
        /// <param name="ary1">The ary1.</param>
        /// <param name="ary2">The ary2.</param>
        /// <returns>Boolean.</returns>
        public static Boolean isMatche(Array ary1, Array ary2)
        {
            try
            {
                if (ary1.Length != ary2.Length) return false;

                if (ary1.GetType() == typeof(UInt16[]))
                {
                    UInt16[] uAry1;
                    UInt16[] uAry2;
                    uAry1 = (UInt16[])ary1;
                    uAry2 = (UInt16[])ary2;
                    for (int i = 0; i < uAry1.Length; i++)
                    {
                        if (uAry1[i] != uAry2[i]) return false;
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                logger.Warn("isMatche(Array) Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// 比較兩個字串內容是否相同
        /// </summary>
        /// <param name="str1">The STR1.</param>
        /// <param name="str2">The STR2.</param>
        /// <returns>Boolean.</returns>
        public static Boolean isMatche(String str1, String str2)
        {
            try
            {
                if (str1 == str2) return true;
                if (str1 == null || str2 == null) return false;
                return str1.Trim().Equals(str2.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                logger.Warn("isMatche Has Exception:{0}", e);
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified object is empty.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>Boolean.</returns>
        public Boolean isEmpty(Object obj)
        {
            if (obj == null)
            {
                return true;
            }
            if (obj is String)
            {
                if ((obj as String).Trim().Length <= 0)
                {
                    return true;
                }
            }
            if (obj.GetType().BaseType == typeof(Array))
            {
                if ((obj as Array).Length <= 0)
                {
                    return true;
                }
            }

            return false;
        }
       

    }

    public class SystemParameter
    {
        public static int MoveStepTime { get; private set; } = 1000;
        public static int BCRReadTime { get; private set; } = 10000;
        public static int LoadArrivalToLoadingTime { get; private set; } = 2000;
        public static int LoadingToLoadCompleteTime { get; private set; } = 12000;
        public static int UnloadArrivalToUnloadingTime { get; private set; } = 2000;
        public static int UnloadingToUnloadCompleteTime { get; private set; } = 12000;
        public static int Report144Time { get; private set; } = 60000;
        public static int Report134Time { get; private set; } = 1000;
        public static int Report136Time { get; private set; } = 1000;
        public static int SECSCmdCnt { get; private set; } = 10;
        public static int SECSCmdInterval { get; private set; } = 30;
        public static string SECSLocalIP { get; private set; } = "127.0.0.1";
        public static string SECSRemoteIP { get; private set; } = "127.0.0.1";
        public static int SECSLocalPort { get; private set; } = 5011;
        public static int SECSRemotePort { get; private set; } = 5011;
        public static int OHTCReplyDelayTime { get; private set; } = 0;
        public static int GrpcSendTimeoutTime { get; private set; } = 30;
        public static int GrpcSendTimeoutTimeMs { get { return GrpcSendTimeoutTime * 1000; } }

        public static void setMoveStepTime(int time)
        {
            MoveStepTime = time;
        }

        public static void setBCRReadTime(int time)
        {
            BCRReadTime = time;
        }

        public static void setLoadArrivalToLoadingTime(int time)
        {
            LoadArrivalToLoadingTime = time;
        }

        public static void setLoadingToLoadCompleteTime(int time)
        {
            LoadingToLoadCompleteTime = time;
        }

        public static void setUnloadArrivalToUnloadingTime(int time)
        {
            UnloadArrivalToUnloadingTime = time;
        }

        public static void setUnloadingToUnloadCompleteTime(int time)
        {
            UnloadingToUnloadCompleteTime = time;
        }

        public static void setReport144Time(int time)
        {
            Report144Time = time;
        }

        public static void setReport134Time(int time)
        {
            Report134Time = time;
        }

        public static void setReport136Time(int time)
        {
            Report136Time = time;
        }

        public static void setSECSCmdCnt(int cnt)
        {
            SECSCmdCnt = cnt;
        }

        public static void setSECSCmdInterval(int interval)
        {
            SECSCmdInterval = interval;
        }

        public static void setSECSLocalIP(string ip)
        {
            SECSLocalIP = ip;
        }

        public static void setSECSRemoteIP(string ip)
        {
            SECSRemoteIP = ip;
        }

        public static void setSECSLocalPort(int port)
        {
            SECSLocalPort = port;
        }

        public static void setSECSRemotePort(int port)
        {
            SECSRemotePort = port;
        }

        public static void setOHTCReplyDelayTime(int time)
        {
            OHTCReplyDelayTime = time;
        }

        public static void setGrpcSendTimeoutTime(int time)
        {
            GrpcSendTimeoutTime = time;
        }

        

    }

}
