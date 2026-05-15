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
    public class SectionDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<Section> loadSection()
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["ASECTION"];
                List<Section> rtnSecLst = null;

                var query = from c in dt.AsEnumerable()
                            select new Section
                            {
                                ID = c.Field<string>("Id"),
                                From_Adr = c.Field<string>("FromAddress"),
                                To_Adr = c.Field<string>("ToAddress"),
                                Distance = c.Field<string>("SecDis"),
                            };

                rtnSecLst = query.ToList();

                return rtnSecLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public Section getSection(string sec_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["ASECTION"];
                Section rtnSec = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("Id").Trim() == sec_id.Trim()
                            select new Section
                            {
                                ID = c.Field<string>("Id"),
                                From_Adr = c.Field<string>("FromAddress"),
                                To_Adr = c.Field<string>("ToAddress"),
                                Distance = c.Field<string>("SecDis"),
                            };

                rtnSec = query.FirstOrDefault();

                return rtnSec;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public Section getSectionByAdress(string adr_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["ASECTION"];
                Section rtnSec = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("FromAddress").Trim() == adr_id.Trim()
                            select new Section
                            {
                                ID = c.Field<string>("Id"),
                                From_Adr = c.Field<string>("FromAddress"),
                                To_Adr = c.Field<string>("ToAddress"),
                                Distance = c.Field<string>("SecDis"),
                            };

                rtnSec = query.FirstOrDefault();

                return rtnSec;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<Section> getSectionByFromAdress(string adr_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["ASECTION"];
                List<Section> rtnSecLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("FromAddress").Trim() == adr_id.Trim()
                            select new Section
                            {
                                ID = c.Field<string>("Id"),
                                From_Adr = c.Field<string>("FromAddress"),
                                To_Adr = c.Field<string>("ToAddress"),
                                Distance = c.Field<string>("SecDis"),
                            };

                rtnSecLst = query.ToList();

                return rtnSecLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<Section> getSectionByToAdress(string adr_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["ASECTION"];
                List<Section> rtnSecLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("ToAddress").Trim() == adr_id.Trim()
                            select new Section
                            {
                                ID = c.Field<string>("Id"),
                                From_Adr = c.Field<string>("FromAddress"),
                                To_Adr = c.Field<string>("ToAddress"),
                                Distance = c.Field<string>("SecDis"),
                            };

                rtnSecLst = query.ToList();

                return rtnSecLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
