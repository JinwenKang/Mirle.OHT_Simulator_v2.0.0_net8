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
    public class VehicleMap
    {
        public String ID { set; get; }
        public String GRPC_Local_IP { set; get; }
        public String GRPC_Lcoal_PORT { set; get; }
        public String GRPC_Remote_IP { set; get; }
        public String GRPC_Remote_PORT { set; get; }
    }
}
