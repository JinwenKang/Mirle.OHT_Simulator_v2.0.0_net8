//*********************************************************************************
//      BlockData.cs
//*********************************************************************************
// File Name: BlockData.cs
// Description: BlockData
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class RouteChangeData
    {
        public ActiveType ActType { set; get; }
        public String TargetPort { set; get; }
        public String CstID { set; get; }
        public uint CstType { set; get; }
    }
}
