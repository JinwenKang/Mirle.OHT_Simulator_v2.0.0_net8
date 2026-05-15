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
    public class VirAddress
    {
        public String Loc_ID { set; get; } = string.Empty;
        public String Sec_ID { set; get; } = string.Empty;
        public bool Is_Virtual { set; get; }
        public int Seq_No { set; get; }
        public String POS_X { set; get; }
        public String POS_Y { set; get; }
        public int Distance { set; get; }
    }
}
