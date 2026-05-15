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
    public class Section
    {
        public String ID { set; get; }
        public String From_Adr { set; get; }
        public String To_Adr { set; get; }
        public String Distance { set; get; }
    }
}
