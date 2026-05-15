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
    public class BlockDataDao
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public BlockData loadBlockByID(string block_id)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["BLOCKDATA"];
                BlockData rtnBlockData = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("BLOCK_ID").Trim() == block_id.Trim()
                            select new BlockData
                            {
                                Block_ID = c.Field<string>("BLOCK_ID"),
                                Start_Section = c.Field<string>("START_SECTION"),
                                End_Section = c.Field<string>("END_SECTION")
                            };

                rtnBlockData = query.FirstOrDefault();

                return rtnBlockData;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<BlockData> loadBlockByStartSection(string start_adr) 
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["BLOCKDATA"];
                List<BlockData> rtnBlockDataLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("START_SECTION").Trim() == start_adr.Trim()
                            select new BlockData
                            {
                                Block_ID = c.Field<string>("BLOCK_ID"),
                                Start_Section = c.Field<string>("START_SECTION"),
                                End_Section = c.Field<string>("END_SECTION")
                            };

                rtnBlockDataLst = query.ToList();

                return rtnBlockDataLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

        public List<BlockData> loadBlockByEndSection(string end_adr)
        {
            try
            {
                DataTable dt = SCApplication.getInstance().ConfigData.Tables["BLOCKDATA"];
                List<BlockData> rtnBlockDataLst = null;

                var query = from c in dt.AsEnumerable()
                            where c.Field<string>("END_SECTION").Trim() == end_adr.Trim()
                            select new BlockData
                            {
                                Block_ID = c.Field<string>("BLOCK_ID"),
                                Start_Section = c.Field<string>("START_SECTION"),
                                End_Section = c.Field<string>("END_SECTION")
                            };

                rtnBlockDataLst = query.ToList();

                return rtnBlockDataLst;
            }
            catch (Exception ex)
            {
                logger.Warn(ex);
                throw;
            }
        }

    }
}
