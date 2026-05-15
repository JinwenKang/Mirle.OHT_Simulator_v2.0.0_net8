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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class PortMap
    {
        public String StationID { set; get; }
        public String StationAddress { set; get; }
        public String StationSection { set; get; }
        public String SEC_DIS_FROM_HEAD { set; get; }
        public String POS_X { set; get; }
        public String POS_Y { set; get; }
        public String PORT_TYPE { set; get; }
    }
}
