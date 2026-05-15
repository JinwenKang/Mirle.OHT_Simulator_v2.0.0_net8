//*********************************************************************************
//      BlockDataDao.cs
//*********************************************************************************
// File Name: BlockDataDao.cs
// Description: BlockDataDao
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using System.IO;
using System.Reflection;
using NLog;
using com.mirle.ibg3k0.sc.Data.VO;

namespace com.mirle.ibg3k0.sc.Data.DAO
{
    public class VehicleMapDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<VehicleMap> loadVehicle()
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["VEHICLEMAP"];
                List<VehicleMap> rtnVehicleLst = null;

                var query = from c in dt.AsEnumerable()
                            select new VehicleMap
                            {
                                ID = c.Field<string>("ID").Trim(),
                                GRPC_Local_IP = c.Field<string>("GRPC_LOCAL_IP").Trim(),
                                GRPC_Lcoal_PORT = c.Field<string>("GRPC_LOCAL_PORT").Trim(),
                                GRPC_Remote_IP = c.Field<string>("GRPC_REMOTE_IP").Trim(),
                                GRPC_Remote_PORT = c.Field<string>("GRPC_REMOTE_PORT").Trim()
                            };

                rtnVehicleLst = query.ToList();

                return rtnVehicleLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
