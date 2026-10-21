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

        private static string SafeField(DataRow row, string columnName)
        {
            if (row == null || row.Table == null || !row.Table.Columns.Contains(columnName))
            {
                return string.Empty;
            }

            var value = row[columnName];
            return value == null || value == DBNull.Value ? string.Empty : value.ToString();
        }

        public List<Address> loadAddress()
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["AADDRESS"];
                if (dt == null)
                {
                    return new List<Address>();
                }

                var query = from c in dt.AsEnumerable()
                            select new Address
                            {
                                Adr_ID = SafeField(c, "Id"),
                                POS_X = SafeField(c, "PositionX"),
                                POS_Y = SafeField(c, "PositionY"),
                                Adr_Type = int.TryParse(SafeField(c, "AdrType"), out int adrType) ? adrType : 0,
                                Sec_ID = SafeField(c, "SecID"),
                                Distance = SafeField(c, "Distance"),
                            };

                return query.ToList();
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
                if (dt == null || string.IsNullOrWhiteSpace(adr_id))
                {
                    return null;
                }

                var query = from c in dt.AsEnumerable()
                            where SafeField(c, "Id").Trim() == adr_id.Trim()
                            select new Address
                            {
                                Adr_ID = SafeField(c, "Id"),
                                POS_X = SafeField(c, "PositionX"),
                                POS_Y = SafeField(c, "PositionY"),
                                Adr_Type = int.TryParse(SafeField(c, "AdrType"), out int adrType) ? adrType : 0,
                                Sec_ID = SafeField(c, "SecID"),
                                Distance = SafeField(c, "Distance"),
                            };

                return query.FirstOrDefault();
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
                if (dt == null)
                {
                    return new List<Address>();
                }

                var query = from c in dt.AsEnumerable()
                            where SafeField(c, "SecID").Trim() == sec_id.Trim() && SafeField(c, "AdrType").Trim() == "1"
                            select new Address
                            {
                                Adr_ID = SafeField(c, "Id"),
                                POS_X = SafeField(c, "PositionX"),
                                POS_Y = SafeField(c, "PositionY"),
                                Adr_Type = int.TryParse(SafeField(c, "AdrType"), out int adrType) ? adrType : 0,
                                Sec_ID = SafeField(c, "SecID"),
                                Distance = SafeField(c, "Distance"),
                            };

                return query.ToList();
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
