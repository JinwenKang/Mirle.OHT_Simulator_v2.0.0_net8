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
using System.Windows.Forms;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class GuideData
    {
        public Address TargetAdr { set; get; }
        public Section TargetSec { set; get; }
        public int Distance { set; get; }
        public String TargetPort { set; get; }
        public Boolean IsOneSection { set; get; } = false;
        public List<String> GuideSection { set; get; }
        public List<String> GuideAddress { set; get; }
    }
}
