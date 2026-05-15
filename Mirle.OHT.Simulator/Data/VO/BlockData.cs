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
    public class BlockData
    {
        public String Block_ID { set; get; }
        public String Start_Section { set; get; }
        public String End_Section { set; get; }
    }
}
