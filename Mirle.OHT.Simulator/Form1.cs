//*********************************************************************************
//      Form1.cs
//*********************************************************************************
// File Name: Form1.cs
// Description: Form1
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using com.mirle.iibg3k0.ttc.Common;
using com.mirle.iibg3k0.ttc.Common.TCPIP;
using Google.Protobuf.Collections;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System.Configuration;
using com.mirle.ibg3k0.sc.ProtocolFormat;
using com.mirle.ibg3k0.sc.App;
using System.Net;
using NLog;
using com.mirle.ibg3k0.sc.Data.VO;
using static System.Collections.Specialized.BitVector32;
using SystemParameter = com.mirle.ibg3k0.sc.App.SystemParameter;
using System.Threading;

namespace Mirle.Agvc.Simulator
{
    public partial class Form1 : Form
    {

        public ConfigHandler configHandler;
        public MiddlerConfigs middlerConfigs;
        public int RichTextBoxMaxLines;
        public XmlHandler AXmlHandler;
        public Dictionary<EnumAddress, LoadunloadCmd> CmdsFromA = new Dictionary<EnumAddress, LoadunloadCmd>();
        public Dictionary<EnumAddress, LoadunloadCmd> CmdsFromB = new Dictionary<EnumAddress, LoadunloadCmd>();
        public Dictionary<EnumAddress, LoadunloadCmd> CmdsFromG = new Dictionary<EnumAddress, LoadunloadCmd>();
        public EnumAddress CurAddr { get; set; } = EnumAddress.A;
        public EnumAddress TempAddr { get; set; } = EnumAddress.A;
        public Random random = new Random();

        private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsInSitu { get; set; } = false;
        public Stopwatch sw = new Stopwatch();

        SCApplication scApp = null;

        private System.Windows.Forms.Timer timerThreadMonitor;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadConfigs();
            scApp = SCApplication.getInstance();
            ConfigToUI();

            LoadXmls();



            scApp.Start();

            toolStripStatusLabel2.Text = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString();

            timerThreadMonitor = new System.Windows.Forms.Timer();
            timerThreadMonitor.Enabled = true;
            timerThreadMonitor.Interval = 10000;
            timerThreadMonitor.Tick += new System.EventHandler(this.Timer_Tick);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel3.Text = "系統時間: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.f");
        }

        private void LoadXmls()
        {
            AXmlHandler = new XmlHandler();
            LoadunloadCmd aCmd = new LoadunloadCmd();
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("AtoA.xml");
            CmdsFromA.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("AtoB.xml");
            CmdsFromA.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("AtoG.xml");
            CmdsFromA.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("BtoB.xml");
            CmdsFromB.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("BtoA.xml");
            CmdsFromB.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("BtoG.xml");
            CmdsFromB.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("GtoG.xml");
            CmdsFromG.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("GtoA.xml");
            CmdsFromG.Add(aCmd.EndAddress, aCmd);
            aCmd = AXmlHandler.ReadXml<LoadunloadCmd>("GtoB.xml");
            CmdsFromG.Add(aCmd.EndAddress, aCmd);
        }

        public delegate void ToolStripStatusLabelTextChangeCallback(ToolStripStatusLabel toolStripStatusLabel, string msg);
        public void ToolStripStatusLabelTextChange(ToolStripStatusLabel toolStripStatusLabel, string msg)
        {
            if (this.InvokeRequired)
            {
                ToolStripStatusLabelTextChangeCallback mydel = new ToolStripStatusLabelTextChangeCallback(ToolStripStatusLabelTextChange);
                this.Invoke(mydel, new object[] { toolStripStatusLabel, msg });
            }
            else
            {
                toolStripStatusLabel.Text = msg;
            }
        }
        private void MiddleAgent_OnDisConnected(object sender, string e)
        {
            ToolStripStatusLabelTextChange(toolStripStatusLabel1, e);
        }
        private void MiddleAgent_OnConnected(object sender, string e)
        {
            ToolStripStatusLabelTextChange(toolStripStatusLabel1, e);
        }

        private void ConfigToUI()
        {
            //txtIp.Text = "192.168.74.82";
            txtIp.Text = SystemParameter.SECSRemoteIP;
            txtPort.Text = "11001";
            txtLocalPort.Text = "10101";
            tbx_init_add_id.Text = "10238";
            tbx_init_sec_id.Text = "49001";
            //txtLocalIP.Text = "192.168.74.82";
            txtLocalIP.Text = SystemParameter.SECSLocalIP;

            // ===== In-Section Load Assignment =====
            chbInSectionLoad.Checked = middlerConfigs.InSectionLoadAssignment;
            // ===== 新增結束 =====
        }

        // 熱路徑 log：每筆命令收/送都會呼叫。原本用 GetType().Name + MethodBase.GetCurrentMethod()
        // 是反射操作，每次呼叫都查 stack frame，300 台車場景下成本可觀。改成字串常數。
        private const string CTX_MiddleAgent_OnCmdSendOrReceive = "Form1:MiddleAgent_OnCmdSendOrReceive";

        private void MiddleAgent_OnCmdSendOrReceive(object sender, string msg)
        {
            LoggerAgent.Instance.LogMsg("Debug", new LogFormat("Debug", "5", CTX_MiddleAgent_OnCmdSendOrReceive, "Device", "CarrierID", msg));
        }

        private void LoadConfigs()
        {
            string iniPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
            configHandler = new ConfigHandler(iniPath);

            LoggerAgent.LogConfigPath = configHandler.GetString("MainFlow", "LogConfigPath", "Log.ini");
            int.TryParse(configHandler.GetString("MainFlow", "RichTextBoxMaxLines", "15"), out int tempRichTextBoxMaxLines);
            RichTextBoxMaxLines = tempRichTextBoxMaxLines;

            middlerConfigs = new MiddlerConfigs();

            int.TryParse(configHandler.GetString("Middler", "ClientNum", "1"), out int tempClientNum);
            middlerConfigs.ClientNum = tempClientNum;

            middlerConfigs.ClientName = configHandler.GetString("Middler", "ClientName", "AGV01");

            middlerConfigs.RemoteIp = configHandler.GetString("Middler", "RemoteIp", "127.0.0.1");

            int.TryParse(configHandler.GetString("Middler", "RemotePort", "10001"), out int tempRemotePort);
            middlerConfigs.RemotePort = tempRemotePort;

            middlerConfigs.LocalIp = configHandler.GetString("Middler", "LocalIp", "127.0.0.1");

            int.TryParse(configHandler.GetString("Middler", "LocalPort", "5002"), out int tempPort);
            middlerConfigs.LocalPort = tempPort;

            int.TryParse(configHandler.GetString("Middler", "RecvTimeoutMs", "10000"), out int tempRecvTimeoutMs);
            middlerConfigs.RecvTimeoutMs = tempRecvTimeoutMs;

            int.TryParse(configHandler.GetString("Middler", "SendTimeoutMs", "0"), out int tempSendTimeoutMs);
            middlerConfigs.SendTimeoutMs = tempSendTimeoutMs;

            int.TryParse(configHandler.GetString("Middler", "MaxReadSize", "0"), out int tempMaxReadSize);
            middlerConfigs.MaxReadSize = tempMaxReadSize;

            int.TryParse(configHandler.GetString("Middler", "ReconnectionIntervalMs", "10000"), out int tempReconnectionIntervalMs);
            middlerConfigs.ReconnectionIntervalMs = tempReconnectionIntervalMs;

            int.TryParse(configHandler.GetString("Middler", "MaxReconnectionCount", "10"), out int tempMaxReconnectionCount);
            middlerConfigs.MaxReconnectionCount = tempMaxReconnectionCount;

            int.TryParse(configHandler.GetString("Middler", "RetryCount", "2"), out int tempRetryCount);
            middlerConfigs.RetryCount = tempRetryCount;

            int.TryParse(configHandler.GetString("Middler", "SleepTime", "10"), out int tempSleepTime);
            middlerConfigs.SleepTime = tempSleepTime;

            middlerConfigs.IsServer = bool.Parse(configHandler.GetString("Middler", "IsServer ", "false"));

            middlerConfigs.CycleRunIntervalMs = int.Parse(configHandler.GetString("Middler", "CycleRunIntervalMs ", "4000"));

            // ===== In-Section Load Assignment =====
            middlerConfigs.InSectionLoadAssignment = bool.Parse(configHandler.GetString("Middler", "InSectionLoadAssignment", "false"));
            // ===== 新增結束 =====
        }

        private void btnAlarmSet_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            if (txtAlarmCode.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please Input Alarm Code.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].SetAlarmToOHBC(txtAlarmCode.Text.Trim(), ErrorStatus.ErrSet);
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtIp.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Remote IP.");
                    return;
                }
                if (txtLocalIP.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Local IP.");
                    return;
                }
                if (txtPort.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Remote Port.");
                    return;
                }

                if (txtLocalPort.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Local Port.");
                    return;
                }

                if (txtVhID.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Vehicle ID.");
                    return;
                }

                string vh_id = txtVhID.Text.Trim();
                if (scApp.AgentDic.ContainsKey(vh_id))
                {
                    MessageBox.Show("Vehicle ID Already Exist.");
                    return;
                }

                MiddlerConfigs newConfig = new MiddlerConfigs();

                newConfig.ClientNum = middlerConfigs.ClientNum;
                newConfig.ClientName = middlerConfigs.ClientName;
                //newConfig.LocalIp = middlerConfigs.LocalIp;
                newConfig.RecvTimeoutMs = middlerConfigs.RecvTimeoutMs;
                newConfig.SendTimeoutMs = middlerConfigs.SendTimeoutMs;
                newConfig.MaxReadSize = middlerConfigs.MaxReadSize;
                newConfig.ReconnectionIntervalMs = middlerConfigs.ReconnectionIntervalMs;
                newConfig.MaxReconnectionCount = middlerConfigs.MaxReconnectionCount;
                newConfig.RetryCount = middlerConfigs.RetryCount;
                newConfig.SleepTime = middlerConfigs.SleepTime;
                newConfig.IsServer = middlerConfigs.IsServer;
                newConfig.CycleRunIntervalMs = middlerConfigs.CycleRunIntervalMs;

                newConfig.RemoteIp = txtIp.Text;
                //newConfig.RemoteIp = txtLocalIP.Text;
                newConfig.RemotePort = int.Parse(txtPort.Text);
                newConfig.LocalPort = int.Parse(txtLocalPort.Text);
                newConfig.LocalIp = txtLocalIP.Text.Trim();
                newConfig.VhID = txtVhID.Text.Trim();

                newConfig.CurrentAdrID = tbx_init_add_id.Text;
                newConfig.CurrentSecID = tbx_init_sec_id.Text;

                MiddleAgent middleAgent = new MiddleAgent(newConfig);
                middleAgent.OnCmdSend += MiddleAgent_OnCmdSendOrReceive;
                middleAgent.OnCmdReceive += MiddleAgent_OnCmdSendOrReceive;
                //middleAgent.OnConnected += MiddleAgent_OnConnected;
                //middleAgent.OnDisConnected += MiddleAgent_OnDisConnected;
                middleAgent.AForm = this;

                //SaveMiddlerConfigs();

                scApp.AgentDic.Add(vh_id, middleAgent);
                updateDGV();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void SaveMiddlerConfigs()
        {
            string iniPath = Path.Combine(Environment.CurrentDirectory, "Configs.ini");
            configHandler = new ConfigHandler(iniPath);

            configHandler.SetString("Middler", "RemoteIp", middlerConfigs.RemoteIp);
            configHandler.SetString("Middler", "RemotePort", middlerConfigs.RemotePort.ToString());
            configHandler.SetString("Middler", "IsServer", middlerConfigs.IsServer.ToString());

        }

        private void btnDisConnect_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_id = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_id))
            {

                scApp.AgentDic[vh_id].Dispose();
                scApp.AgentDic.Remove(vh_id);
                updateDGV();
            }

            scApp.deleteVhAddress(vh_id);
        }

        public Dictionary<string, string> SetupCommandInfoPairs(LoadunloadCmd loadunloadCmd)
        {
            Dictionary<string, string> pairs = new Dictionary<string, string>();

            try
            {
                pairs.Add("CmdID", loadunloadCmd.CmdId);
                pairs.Add("CSTID", loadunloadCmd.CstId);
                pairs.Add("GuideSectionsStartToLoad", SetupCommandInfoPairFromBuffer(loadunloadCmd.GuideSectionsStartToLoad));
                pairs.Add("GuideAddressesStartToLoad", SetupCommandInfoPairFromBuffer(loadunloadCmd.GuideAddressesStartToLoad));
                pairs.Add("LoadAdr", loadunloadCmd.LoadAdr);
                pairs.Add("GuideSectionsToDestination", SetupCommandInfoPairFromBuffer(loadunloadCmd.GuideSectionsToDestination));
                pairs.Add("GuideAddressesToDestination", SetupCommandInfoPairFromBuffer(loadunloadCmd.GuideAddressesToDestination));
                pairs.Add("DestinationAdr", loadunloadCmd.DestinationAdr);
                pairs.Add("SecDistance", loadunloadCmd.SecDistance.ToString());
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            return pairs;
        }

        public string SetupCommandInfoPairFromBuffer(List<string> strbuffer)
        {
            if (strbuffer.Count == 0)
            {
                return "[]";
            }

            string v = "[";
            for (int i = 0; i < strbuffer.Count - 1; i++)
            {
                v += strbuffer[i] + ",";
            }
            v += strbuffer[strbuffer.Count - 1] + "]";
            return v;
        }

        private EnumAddress EnumAddressParse(string v)
        {
            try
            {
                v = v.Trim();

                return (EnumAddress)Enum.Parse(typeof(EnumAddress), v);
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
                return EnumAddress.A;
            }
        }

        public EnumAddress GetEnumAddressFormRadioButton(RadioButton radioButton)
        {
            return EnumAddressParse(radioButton.Text);
        }

        private void btnCleanRichTextBox_Click(object sender, EventArgs e)
        {
            // 原 DebugLogMsg 已移除（dead code），此按鈕保留為 UI 相容性
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                if (radioButton_Normal.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.NormalComplete);
                }
                else if (radioButton_Abnormal_BcrReadFail.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.Abnormal_BcrReadFail);
                }
                else if (radioButton_Abnormal_BcrMisMatch.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.Abnormal_BcrMismatch);
                }
                else if (radioButton_Abnormal_BcrDuplicate.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.Abnormal_BcrDuplicate);
                }
                else if (radioButton_Abnormal_DoubleStorage.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.Abnormal_DoubleStorage);
                }
                else if (radioButton_Abnormal_EmptyRetrieval.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.Abnormal_EmptyRetrieval);
                }
                else if (radioButton_Cancel.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.CancelComplete);
                }
                else if (radioButton_Abort.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.AbortComplete);
                }
                else if (radioButton_InterlockError.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.InterlockError);
                }
                else if (radioButton_InterlockErrorUnload.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.InterlockErrorUnload);
                }
                else if (radioButton_Reject31.Checked == true)
                {
                    scApp.AgentDic[vh_no].OperatypeSet(EnumOperaType.RejectCommand);
                }
            }
        }

        private void updateDGV()
        {
            // 一次性更新避免每加一 row 都觸發 layout / autosize / repaint
            // 300 台車場景單一次 updateDGV 可省下大量 UI 重繪
            dgvOht.SuspendLayout();
            try
            {
                dgvOht.Rows.Clear();

                List<MiddleAgent> agentLst = scApp.AgentDic.Values.ToList();

                // 用 AddRange 一次塞所有列，比逐筆 Add 快
                var rows = new List<DataGridViewRow>(agentLst.Count);
                foreach (MiddleAgent agent in agentLst)
                {
                    var row = new DataGridViewRow();
                    row.CreateCells(dgvOht, agent.RemoteIP(), agent.VhID());
                    rows.Add(row);
                }
                dgvOht.Rows.AddRange(rows.ToArray());
            }
            finally
            {
                dgvOht.ResumeLayout();
            }
        }

        private void set_button_Click(object sender, EventArgs e)
        {
            Global_Param.MismatchID = Mismatch_ID.Text;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            IPHostEntry host = Dns.GetHostEntry("localhost");

            Console.WriteLine($"GetHostEntry({"ohtc01"}) returns:");

            foreach (IPAddress address in host.AddressList)
            {
                Console.WriteLine($"    {address}");
            }
        }

        private void RadioBtn_Server_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void stopSet_button_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].SetStopSignal();
            }
        }

        private void stopClear_button_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].ClearStopSignal();
            }
        }

        private void btnAlarmClear_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            if (txtAlarmCode.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please Input Alarm Code.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].SetAlarmToOHBC(txtAlarmCode.Text.Trim(), ErrorStatus.ErrReset);
            }
        }

        private void btnCstSet_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            if (txtCstID.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please Input Cst ID.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].setCstID(txtCstID.Text.Trim());
            }
        }

        private void btnCstClear_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].setCstID(string.Empty);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            autoOnlineVehicle();
            //try
            //{
            //    if (txtIp.Text.Trim().Equals(string.Empty))
            //    {
            //        MessageBox.Show("Please Input Remote IP.");
            //        return;
            //    }
            //    if (txtPort.Text.Trim().Equals(string.Empty))
            //    {
            //        MessageBox.Show("Please Input Remote Port.");
            //        return;
            //    }

            //    //讀取config
            //    //迴圈設定每台車的連線
            //    List<VehicleMap> lstVehicleMap = scApp.ConfigDataBLL.loadVehicleMap();// (recent31Cmd.UnloadPortID);loadVehicleMap
            //    List<com.mirle.ibg3k0.sc.Data.VO.Section> lstSection = scApp.ConfigDataBLL.loadSection();
            //    lstSection = lstSection.Where(section => Convert.ToInt32(section.Distance) > 2500).ToList();
            //    int i = 0;
            //    foreach (VehicleMap v in lstVehicleMap)
            //    {
            //        try
            //        {
            //            MiddlerConfigs newConfig = new MiddlerConfigs();

            //            newConfig.ClientNum = middlerConfigs.ClientNum;
            //            newConfig.ClientName = middlerConfigs.ClientName;
            //            //newConfig.LocalIp = middlerConfigs.LocalIp;
            //            newConfig.RecvTimeoutMs = middlerConfigs.RecvTimeoutMs;
            //            newConfig.SendTimeoutMs = middlerConfigs.SendTimeoutMs;
            //            newConfig.MaxReadSize = middlerConfigs.MaxReadSize;
            //            newConfig.ReconnectionIntervalMs = middlerConfigs.ReconnectionIntervalMs;
            //            newConfig.MaxReconnectionCount = middlerConfigs.MaxReconnectionCount;
            //            newConfig.RetryCount = middlerConfigs.RetryCount;
            //            newConfig.SleepTime = middlerConfigs.SleepTime;
            //            newConfig.IsServer = middlerConfigs.IsServer;
            //            newConfig.CycleRunIntervalMs = middlerConfigs.CycleRunIntervalMs;

            //            newConfig.RemoteIp = txtIp.Text;
            //            //newConfig.RemoteIp = txtLocalIP.Text;
            //            newConfig.RemotePort = int.Parse(txtPort.Text);
            //            //newConfig.LocalPort = int.Parse(txtLocalPort.Text);
            //            //newConfig.LocalIp = txtLocalIP.Text.Trim();
            //            newConfig.LocalPort = int.Parse(v.GRPC_Lcoal_PORT);
            //            newConfig.LocalIp = v.GRPC_Local_IP;
            //            newConfig.VhNo = v.NO;
            //            newConfig.VhID = v.ID;

            //            newConfig.CurrentAdrID = lstSection[i].From_Adr;
            //            newConfig.CurrentSecID = lstSection[i].ID;
            //            //TBD

            //            MiddleAgent middleAgent = new MiddleAgent(newConfig);
            //            middleAgent.OnCmdSend += MiddleAgent_OnCmdSendOrReceive;
            //            middleAgent.OnCmdReceive += MiddleAgent_OnCmdSendOrReceive;
            //            middleAgent.OnConnected += MiddleAgent_OnConnected;
            //            middleAgent.OnDisConnected += MiddleAgent_OnDisConnected;
            //            middleAgent.AForm = this;

            //            //SaveMiddlerConfigs();

            //            scApp.AgentDic.Add(v.NO, middleAgent);
            //            i += 1;
            //        }
            //        catch (Exception ex)
            //        {
            //            logger.Error(ex.ToString());
            //        }
            //    }
            //    updateDGV();
            //}
            //catch (Exception ex)
            //{
            //    logger.Error(ex.ToString());
            //}
        }

        private void btnOn_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            if (tbxMs.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please Input Time.");
                return;
            }

            int tick_time;
            if (!int.TryParse(tbxMs.Text, out tick_time))
            {
                MessageBox.Show("Please Input Correct Time.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].startTimer(tick_time);
            }
        }

        private void btnOff_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].stopTimer();
            }
        }

        private void btnSetJump_Click(object sender, EventArgs e)
        {
            if(chbSetFlyToTarget.Checked)
            {
                scApp.setIsJumpToTarget(true);
            }
            else
            {
                scApp.setIsJumpToTarget(false);
            }
        }

        private void btnDisableLocCheck_Click(object sender, EventArgs e)
        {
            if (chbDisableLocChk.Checked)
            {
                scApp.setDisableLocCheck(true);
            }
            else
            {
                scApp.setDisableLocCheck(false);
            }
        }

        private void Button_BD_Car_Set_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }
            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {

                scApp.AgentDic[vh_no].SetVehicleToManual(0);
            }
        }

        private void button3Button_BD_Car_ReSet_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }
            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                scApp.AgentDic[vh_no].SetVehicleToManual(1);
            }
        }

        private void btnSecsCmdStart_Click(object sender, EventArgs e)
        {
            scApp.setSecsTestTimer(true);
        }

        private void btnSecsCmdStop_Click(object sender, EventArgs e)
        {
            scApp.setSecsTestTimer(false);
        }

        bool discon_flag = false;
        private async void btn_DisCon_Click(object sender, EventArgs e)
        {
            //檢查是否有已經連線的車輛
            if (!scApp.AgentDic.Any())
            {
                MessageBox.Show("No Vehicle Connect.");
                return;
            }

            //檢查連線時間與斷線時間設定是否為正整數
            if (!int.TryParse(txBox_ConTime.Text, out int conTime))
            {
                MessageBox.Show("Connect Time is not integer");
                return;
            }
            else if (conTime < 0)
            {
                MessageBox.Show("Connect Time is not positive integer");
                return;
            }
            else
            {
                conTime = conTime * 1000;
            }

            if (!int.TryParse(txBox_DisConTime.Text, out int disconTime))
            {
                MessageBox.Show("Disconnect Time is not integer");
                return;
            }
            else if (disconTime < 0)
            {
                MessageBox.Show("Disconnect Time is not positive integer");
                return;
            }
            else
            {
                disconTime = disconTime * 1000;
            }

            //控制斷線測試的 flag
            discon_flag = true;

            while(discon_flag)
            {
                //將 Dictionary 中所有的 agent 資源釋放，並清乾淨 Dictionary
                foreach (var agent in scApp.AgentDic)
                {
                    agent.Value.Dispose();
                    string vh_no = agent.Key;
                    scApp.deleteVhAddress(vh_no);
                }
                scApp.AgentDic.Clear();
                updateDGV();
                await Task.Delay(disconTime);

                //重新建立連線
                autoOnlineVehicle();
                //若 IP 設定欄位是空的，就跳出流程
                if (txtIp.Text.Trim().Equals(string.Empty))
                {
                    discon_flag = false;
                    return;
                }
                if (txtPort.Text.Trim().Equals(string.Empty))
                {
                    discon_flag = false;
                    return;
                }

                await Task.Delay(conTime);
            }
        }

        private void btn_disConStop_Click(object sender, EventArgs e)
        {
            discon_flag = false;
        }

        private void autoOnlineVehicle()
        {
            try
            {
                if (txtIp.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Remote IP.");
                    return;
                }
                if (txtPort.Text.Trim().Equals(string.Empty))
                {
                    MessageBox.Show("Please Input Remote Port.");
                    return;
                }

                //讀取config
                //迴圈設定每台車的連線
                List<VehicleMap> lstVehicleMap = scApp.ConfigDataBLL.loadVehicleMap();// (recent31Cmd.UnloadPortID);loadVehicleMap
                List<com.mirle.ibg3k0.sc.Data.VO.Section> lstSection = scApp.ConfigDataBLL.loadSection();
                lstSection = lstSection.Where(section => Convert.ToInt32(section.Distance) > 4000).ToList();
                List<AutoSecAdr> autoSecAdrLst = new List<AutoSecAdr>();
                foreach(com.mirle.ibg3k0.sc.Data.VO.Section sec in lstSection)
                {
                    autoSecAdrLst.Add(new AutoSecAdr() { Sec = sec.ID, Address = sec.From_Adr});
                    autoSecAdrLst.Add(new AutoSecAdr() { Sec = sec.ID, Address = sec.To_Adr });
                }
                int i = 0;
                foreach (VehicleMap v in lstVehicleMap)
                {
                    try
                    {
                        MiddlerConfigs newConfig = new MiddlerConfigs();

                        newConfig.ClientNum = middlerConfigs.ClientNum;
                        newConfig.ClientName = middlerConfigs.ClientName;
                        //newConfig.LocalIp = middlerConfigs.LocalIp;
                        newConfig.RecvTimeoutMs = middlerConfigs.RecvTimeoutMs;
                        newConfig.SendTimeoutMs = middlerConfigs.SendTimeoutMs;
                        newConfig.MaxReadSize = middlerConfigs.MaxReadSize;
                        newConfig.ReconnectionIntervalMs = middlerConfigs.ReconnectionIntervalMs;
                        newConfig.MaxReconnectionCount = middlerConfigs.MaxReconnectionCount;
                        newConfig.RetryCount = middlerConfigs.RetryCount;
                        newConfig.SleepTime = middlerConfigs.SleepTime;
                        newConfig.IsServer = middlerConfigs.IsServer;
                        newConfig.CycleRunIntervalMs = middlerConfigs.CycleRunIntervalMs;

                        newConfig.RemoteIp = v.GRPC_Remote_IP;
                        //newConfig.RemoteIp = txtLocalIP.Text;
                        newConfig.RemotePort = int.Parse(v.GRPC_Remote_PORT);
                        //newConfig.LocalPort = int.Parse(txtLocalPort.Text);
                        //newConfig.LocalIp = txtLocalIP.Text.Trim();
                        newConfig.LocalPort = int.Parse(v.GRPC_Lcoal_PORT);
                        newConfig.LocalIp = v.GRPC_Local_IP;
                        newConfig.VhID = v.ID;

                        newConfig.CurrentAdrID = autoSecAdrLst[i].Address;
                        newConfig.CurrentSecID = autoSecAdrLst[i].Sec;
                        //TBD

                        MiddleAgent middleAgent = new MiddleAgent(newConfig);
                        middleAgent.OnCmdSend += MiddleAgent_OnCmdSendOrReceive;
                        middleAgent.OnCmdReceive += MiddleAgent_OnCmdSendOrReceive;
                        middleAgent.OnConnected += MiddleAgent_OnConnected;
                        middleAgent.OnDisConnected += MiddleAgent_OnDisConnected;
                        middleAgent.AForm = this;

                        //SaveMiddlerConfigs();

                        scApp.AgentDic.Add(v.ID, middleAgent);
                        i += 1;
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                    }
                }
                updateDGV();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        private void btSetDelayTime_Click(object sender, EventArgs e)
        {
            try
            {
                int time = 0;
                if(int.TryParse(tbxDelayTime.Text, out time))
                {
                    scApp.setOHTCReplyDelayTime(time);
                }
                else
                {
                    MessageBox.Show("Please Check Input Data.");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
        }

        
        private void btThreadStart_Click(object sender, EventArgs e)
        {
            timerThreadMonitor.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            MonitorThreadPool();
        }

        private int workerThreadsUsed = 0;
        private int completionPortThreadsUsed = 0;
        private void MonitorThreadPool()
        {
            int workerThreadsAvailable, completionPortThreadsAvailable;
            int workerThreadsMax, completionPortThreadsMax;

            // 取得可用的執行緒數
            ThreadPool.GetAvailableThreads(out workerThreadsAvailable, out completionPortThreadsAvailable);

            // 取得最大執行緒數
            ThreadPool.GetMaxThreads(out workerThreadsMax, out completionPortThreadsMax);

            // 計算目前使用的執行緒數
            if ((workerThreadsMax - workerThreadsAvailable) > workerThreadsUsed)
            {
                workerThreadsUsed = workerThreadsMax - workerThreadsAvailable;
            }

            if ((completionPortThreadsMax - completionPortThreadsAvailable) > completionPortThreadsUsed)
            {
                completionPortThreadsUsed = completionPortThreadsMax - completionPortThreadsAvailable;
            }

            // 更新 UI 元素必須在 UI 執行緒上進行；
            // 用 BeginInvoke 非同步排程，避免 monitor thread 被 UI 卡住、
            // 也避免 UI 忙時反過來阻塞這條診斷 thread
            if (!this.IsDisposed)
            {
                this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
                {
                    txtWorkThread.Text = workerThreadsUsed.ToString();
                    txtIOThread.Text = completionPortThreadsUsed.ToString();
                });
            }
        }

        private void bt_sendTeachData_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                // 開啟檔案選擇對話框
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Select Teaching Data CSV File";
                openFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.InitialDirectory = Path.Combine(Environment.CurrentDirectory, "Config");
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    
                    if (!File.Exists(selectedFilePath))
                    {
                        MessageBox.Show("Selected file does not exist.");
                        return;
                    }

                    try
                    {
                        scApp.AgentDic[vh_no].Send_Cmd122_TeachingData(selectedFilePath);
                        MessageBox.Show($"Teaching data sent successfully from:\n{selectedFilePath}");
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex.ToString());
                        MessageBox.Show($"Error sending teaching data:\n{ex.Message}");
                    }
                }
            }
        }

        private void bt_setTeachReply_Click(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                if (rb_teachOk.Checked == true)
                {
                    scApp.AgentDic[vh_no].TeachingReplySet(true);
                }
                else if (rb_teachNG.Checked == true)
                {
                    scApp.AgentDic[vh_no].TeachingReplySet(false);
                }
            }
        }

        private void radioButton_Reject41_CheckedChanged(object sender, EventArgs e)
        {
            if (dgvOht.SelectedRows.Count <= 0)
            {
                MessageBox.Show("Please Select OHT.");
                return;
            }

            string vh_no = dgvOht.SelectedRows[0].Cells[1].Value.ToString();
            if (scApp.AgentDic.ContainsKey(vh_no))
            {
                bool isReject = radioButton_Reject41.Checked;
                scApp.AgentDic[vh_no].Reject41Set(isReject);
            }
        }

        // ===== In-Section Load Assignment =====
        /// <summary>
        /// CheckBox 狀態變更：套用 In-Section Load Assignment 開關至所有已連線 OHT。
        /// 勾選（ON）：OHT 收到 ID_36 NextAction.ActType=Load（Reserve成功）時，將 Move 就地升級為 Load。
        /// 未勾選（OFF）：OHT 靜默忽略 NextAction，繼續原 Move 命令走到終點，以 CmpStatusMove 完成。
        /// </summary>
        private void chbInSectionLoad_CheckedChanged(object sender, EventArgs e)
        {
            bool enabled = chbInSectionLoad.Checked;
            middlerConfigs.InSectionLoadAssignment = enabled;

            // 套用至所有已連線的 OHT agent
            foreach (var kv in scApp.AgentDic)
            {
                kv.Value.SetInSectionLoadEnabled(enabled);
            }

            logger.Info($"[InSectionLoad] Global mode changed to: {(enabled ? "ON" : "OFF")}");
        }
        // ===== 新增結束 =====
    }
}
