//*********************************************************************************
//      Section.cs
//*********************************************************************************
// File Name: Section.cs
// Description: Section
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
    public class CrrCmdData
    {
        public bool CurSend { get; set; }
        public String Carrier { set; get; }
        public String Source { set; get; }
        public String Target { set; get; }
        public int RetryCnt { set; get; }
    }
}
