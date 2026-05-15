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
    public class Address
    {
        public String Adr_ID { set; get; }
        public String POS_X { set; get; }
        public String POS_Y { set; get; }
        public int Adr_Type { set; get; }
        public String Sec_ID { set; get; }
        public String Distance { set; get; }
    }
}
