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
    public class SecsCmdSetDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public List<SecsCmdSet> loadSecsCmdSet()
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["SECSCMDSET"];
                List<SecsCmdSet> rtnSecsCmdsetLst = null;

                var query = from c in dt.AsEnumerable()
                            select new SecsCmdSet
                            {
                                Carrier = c.Field<string>("Carrier"),
                                Source = c.Field<string>("Source"),
                                Target = c.Field<string>("Target")
                            };

                rtnSecsCmdsetLst = query.ToList();

                return rtnSecsCmdsetLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
