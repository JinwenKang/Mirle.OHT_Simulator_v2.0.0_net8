//*********************************************************************************
//      MiddlerConfigs.cs
//*********************************************************************************
// File Name: MiddlerConfigs.cs
// Description: MiddlerConfigs
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agvc.Simulator
{
    public class MiddlerConfigs
    {
        public string VhID { get; set; }
        public int ClientNum { get; set; }
        public string ClientName { get; set; }
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public string LocalIp { get; set; }
        public int LocalPort { get; set; }
        public int RecvTimeoutMs { get; set; }
        public int SendTimeoutMs { get; set; }
        public int MaxReadSize { get; set; }
        public int ReconnectionIntervalMs { get; set; }
        public int MaxReconnectionCount { get; set; }
        public int RetryCount { get; set; }
        public int SleepTime { get; set; }
        public bool IsServer { get; set; }
        public int CycleRunIntervalMs { get; set; } = 4000;
        
        public string CurrentAdrID { get; set; }
        public string CurrentSecID { get; set; }

        // ===== In-Section Load Assignment =====
        /// <summary>
        /// 開啟時：收到 ID_36 NextAction.ActType=Load 時，將 Move 命令就地升級為 Load。
        /// 關閉時：靜默忽略 NextAction，OHT 繼續原 Move 命令走到終點。
        /// </summary>
        public bool InSectionLoadAssignment { get; set; } = true;
        // ===== 新增結束 =====
    }
}
