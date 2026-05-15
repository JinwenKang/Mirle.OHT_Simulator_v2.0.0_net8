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
    public class AddressDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<Address> loadAddress()
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["AADDRESS"];
                List<Address> rtnAdrLst = null;

                var query = from c in dt.AsEnumerable()
                            select new Address
                            {
                                Adr_ID = c.Field<string>("Id"),
                                POS_X = c.Field<string>("PositionX"),
                                POS_Y = c.Field<string>("PositionY"),
                                Adr_Type = int.Parse(c.Field<string>("AdrType")),
                                Sec_ID = c.Field<string>("SecID"),
                                Distance = c.Field<string>("Distance"),
                            };

                rtnAdrLst = query.ToList();

                return rtnAdrLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public Address getAddress(string adr_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["AADDRESS"];
                Address rtnAdr = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("Id").Trim() == adr_id.Trim()
                            select new Address
                            {
                                Adr_ID = c.Field<string>("Id"),
                                POS_X = c.Field<string>("PositionX"),
                                POS_Y = c.Field<string>("PositionY"),
                                Adr_Type = int.Parse(c.Field<string>("AdrType")),
                                Sec_ID = c.Field<string>("SecID"),
                                Distance = c.Field<string>("Distance"),
                            };

                rtnAdr = query.FirstOrDefault();

                return rtnAdr;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<Address> loadParkAddressBySec(string sec_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["AADDRESS"];
                List<Address> rtnAdrLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("SecID").Trim() == sec_id.Trim() && c.Field<string>("AdrType").Trim() == "1"
                            select new Address
                            {
                                Adr_ID = c.Field<string>("Id"),
                                POS_X = c.Field<string>("PositionX"),
                                POS_Y = c.Field<string>("PositionY"),
                                Adr_Type = int.Parse(c.Field<string>("AdrType")),
                                Sec_ID = c.Field<string>("SecID"),
                                Distance = c.Field<string>("Distance"),
                            };

                rtnAdrLst = query.ToList();

                return rtnAdrLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
