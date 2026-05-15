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
    public class PortMapDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public PortMap loadPortMapByID(string station_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["PORTMAP"];
                PortMap rtnPortMap = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("StationID").Trim() == station_id.Trim()
                            select new PortMap
                            {
                                StationID = c.Field<string>("StationID"),
                                StationAddress = c.Field<string>("StationAddress"),
                                StationSection = c.Field<string>("StationSection"),
                                SEC_DIS_FROM_HEAD = c.Field<string>("SEC_DIS_FROM_HEAD"),
                                POS_X = c.Field<string>("POS_X"),
                                POS_Y = c.Field<string>("POS_Y"),
                                PORT_TYPE = c.Field<string>("PORT_TYPE")
                            };

                rtnPortMap = query.FirstOrDefault();

                return rtnPortMap;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<PortMap> loadPortMapBySecID(string sec_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["PORTMAP"];
                List<PortMap> rtnPortMapLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("StationSection").Trim() == sec_id.Trim()
                            select new PortMap
                            {
                                StationID = c.Field<string>("StationID"),
                                StationAddress = c.Field<string>("StationAddress"),
                                StationSection = c.Field<string>("StationSection"),
                                SEC_DIS_FROM_HEAD = c.Field<string>("SEC_DIS_FROM_HEAD"),
                                POS_X = c.Field<string>("POS_X"),
                                POS_Y = c.Field<string>("POS_Y"),
                                PORT_TYPE = c.Field<string>("PORT_TYPE")
                            };

                rtnPortMapLst = query.ToList();

                return rtnPortMapLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
