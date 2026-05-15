//*********************************************************************************
//      TeachingData.cs
//*********************************************************************************
// File Name: TeachingData.cs
// Description: TeachingData
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using System;

namespace com.mirle.ibg3k0.sc.Data.VO
{
    public class TeachingData
    {
        public String AddressID { set; get; }
        public String Barcode { set; get; }
        public String GrabY { set; get; }
        public String GrabT { set; get; }
        public String GrabZ { set; get; }
        public String NEEDPIO { set; get; }
        public String PIOID { set; get; }
        public String PIOCHANNEL { set; get; }
        public String PIOPORTNUMBER { set; get; }
        public String StationNumber { set; get; }
        public String StationSection { set; get; }
        public String POSX { set; get; }
        public String POSY { set; get; }
        public String SECDIS { set; get; }
        public String PORTTYPE { set; get; }
        public String AREATYPE { set; get; }
        public String TA1TIMEOUT { set; get; }
    }
}
