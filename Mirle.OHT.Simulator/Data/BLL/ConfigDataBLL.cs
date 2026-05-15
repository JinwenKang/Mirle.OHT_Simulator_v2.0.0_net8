//*********************************************************************************
//      BlockDataBLL.cs
//*********************************************************************************
// File Name: BlockDataBLL.cs
// Description: 業務邏輯：Block Data
//
//(c) Copyright 2014, MIRLE Automation Corporation
//
// Date          Author         Request No.    Tag     Description
// ------------- -------------  -------------  ------  -----------------------------
//**********************************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.App;
using com.mirle.ibg3k0.sc.Data.DAO;
using com.mirle.ibg3k0.sc.Data.VO;
using Mirle.Agvc.Simulator.App;
using Nest;
using NLog;
using static Nest.JoinField;

namespace com.mirle.ibg3k0.sc.BLL
{
    public class ConfigDataBLL
    {
        private SCApplication scApp = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private Logger logger_systemProcess = LogManager.GetLogger("SystemProcess");

        private BlockDataDao blockDataDao = null;
        private PortMapDao portMapDao = null;
        private AddressDao addressDao = null;
        private VehicleMapDao vehicleMapDao = null;
        private SectionDao sectionDao = null;
        private SecsCmdSetDao secsCmdSetDao = null;

        public ConfigDataBLL()
        {

        }

        public void start(SCApplication scApp)
        {
            this.scApp = scApp;
            blockDataDao = this.scApp.BlockDataDao;
            portMapDao = this.scApp.PortMapDao;
            addressDao = this.scApp.AddressDao;
            vehicleMapDao = this.scApp.VehicleMapDao;
            sectionDao = this.scApp.SectionDao;
            secsCmdSetDao = this.scApp.SecsCmdSetDao;
        }

        public BlockData loadBlockByID(string block_id)
        {
            BlockData blockData = null;

            try
            {
                blockData = blockDataDao.loadBlockByID(block_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return blockData;
        }

        public List<BlockData> loadBlockByStartSection(string start_sec)
        {
            List<BlockData> lstBlockData = null;

            try
            {
                lstBlockData = blockDataDao.loadBlockByStartSection(start_sec);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return lstBlockData;
        }

        public List<BlockData> loadBlockByEndSection(string end_sec)
        {
            List<BlockData> lstBlockData = null;

            try
            {
                lstBlockData = blockDataDao.loadBlockByEndSection(end_sec);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return lstBlockData;
        }

        public PortMap loadPortMapByID(string station_id)
        {
            PortMap portMap = null;

            try
            {
                portMap = portMapDao.loadPortMapByID(station_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return portMap;
        }

        public List<PortMap> loadPortMapBySecID(string sec_id)
        {
            List<PortMap> portMapLst = null;

            try
            {
                portMapLst = portMapDao.loadPortMapBySecID(sec_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return portMapLst;
        }

        public string getPortMapToAddress(string station_id, string vh_id, string cmd_id)
        {
            string adr = string.Empty;

            try
            {
                logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getPortMapToAddress get port");

                PortMap portMap = portMapDao.loadPortMapByID(station_id);
                Section sec = sectionDao.getSection(portMap.StationSection);

                if (sec != null)
                {
                    adr = sec.From_Adr;

                    logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getPortMapToAddress found section by port, Section:[{sec.ID}] Section From Address:[{adr}]");
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adr;
        }

        public string getPortMapFromAddress(string station_id)
        {
            string adr = string.Empty;

            try
            {
                Address address = addressDao.getAddress(station_id);
                if(address != null)
                {
                    Section sec = sectionDao.getSection(address.Sec_ID);

                    if (sec != null)
                    {
                        adr = sec.To_Adr;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adr;
        }

        public string getMoveToAddress(string id, string vh_id, string cmd_id)
        {
            string adr = id;

            try
            {
                logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress check is address or port");

                Address address = addressDao.getAddress(id);
                if(address != null)
                {
                    logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress check is address");

                    if (address.Adr_Type == SCAppConstants.AddressType.Normal)
                    {
                        adr = id;

                        logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress address type is normal, To Address:[{adr}]");
                    }
                    else
                    {
                        logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress address type is control, Address Section:[{address.Sec_ID}]");

                        Section sec = sectionDao.getSection(address.Sec_ID);
                        if (sec != null)
                        {
                            adr = sec.From_Adr;

                            logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress found section, Section From Address:[{adr}]");
                        }
                    }
                }
                else
                {
                    logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress check is port");

                    PortMap portMap = portMapDao.loadPortMapByID(id);
                    Section sec = sectionDao.getSection(portMap.StationSection);

                    if (sec != null)
                    {
                        adr = sec.From_Adr;

                        logger_systemProcess.Info($"Vh:[{vh_id}] Cmd:[{cmd_id}], getMoveToAddress found section by port, Section:[{sec.ID}] Section From Address:[{adr}]");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adr;
        }

        public Section getMoveToSection(string id, string sec_id, out int distance)
        {
            Section sec = null;
            distance = 0;

            try
            {
                Address address = addressDao.getAddress(id);
                if (address != null)
                {
                    if (address.Adr_Type == SCAppConstants.AddressType.Normal)
                    {
                        sec = sectionDao.getSectionByToAdress(id).Where(s => s.ID.Trim() == sec_id.Trim()).FirstOrDefault();
                        if(sec == null)
                        {
                            sec = sectionDao.getSectionByToAdress(id).FirstOrDefault();
                            distance = int.Parse(sec.Distance);
                        }
                    }
                    else
                    {
                        sec = sectionDao.getSection(address.Sec_ID);
                        distance = int.Parse(address.Distance);
                    }
                }
                else
                {
                    PortMap portMap = portMapDao.loadPortMapByID(id);
                    sec = sectionDao.getSection(portMap.StationSection);
                    distance = int.Parse(portMap.SEC_DIS_FROM_HEAD);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return sec;
        }

        public string getMoveFromAddress(string adr_id, string sec_id, uint distance)
        {
            string adr = adr_id;

            try
            {
                if(distance > 0)
                {
                    Section sec = sectionDao.getSection(sec_id);
                    if(sec != null)
                    {
                        adr = sec.To_Adr;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adr;
        }

        public List<Address> loadAddress()
        {
            List<Address> adrLst = null;

            try
            {
                adrLst = addressDao.loadAddress();
                foreach (Address adr in adrLst)
                {
                    if (adr.Adr_Type == 1)
                    {
                        Section sec = sectionDao.getSection(adr.Sec_ID);
                        Address from = addressDao.getAddress(sec.From_Adr);
                        Address to = addressDao.getAddress(sec.To_Adr);
                        if (sec != null) { 
                            if (sec == null || from == null || to == null || double.Parse(sec.Distance) == 0) continue;                            
                            double sectionrealdis = double.Parse(sec.Distance); //實際車踩的距離
                            double unitvector_x = Convert.ToDouble(int.Parse(to.POS_X) - int.Parse(from.POS_X)) / sectionrealdis; //單位向量x
                            double unitvector_y = Convert.ToDouble(int.Parse(to.POS_Y) - int.Parse(from.POS_Y)) / sectionrealdis; //單位向量y
                            adr.POS_X = (int.Parse(from.POS_X) + Convert.ToInt32(unitvector_x * double.Parse(adr.Distance))).ToString();
                            adr.POS_Y = (int.Parse(from.POS_Y) + Convert.ToInt32(unitvector_y * double.Parse(adr.Distance))).ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adrLst;
        }

        public Address getAddress(string adr_id)
        {
            Address rtnAdr = null;

            try
            {
                rtnAdr = addressDao.getAddress(adr_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return rtnAdr;
        }

        public List<Address> loadParkAddressBySec(string sec_id)
        {
            List<Address> adrLst = null;

            try
            {
                adrLst = addressDao.loadParkAddressBySec(sec_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return adrLst;
        }

        public List<Section> loadSection()
        {
            List<Section> secLst = null;

            try
            {
                secLst = sectionDao.loadSection();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return secLst;
        }

        public Section getSection(string sec_id)
        {
            Section rtnSec = null;

            try
            {
                rtnSec = sectionDao.getSection(sec_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return rtnSec;
        }

        public Section getSectionByAdress(string adr_id)
        {
            Section rtnSec = null;

            try
            {
                rtnSec = sectionDao.getSectionByAdress(adr_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return rtnSec;
        }

        public List<Section> getSectionByFromAdress(string adr_id)
        {
            List<Section> rtnSec = null;

            try
            {
                rtnSec = sectionDao.getSectionByFromAdress(adr_id);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return rtnSec;
        }

        public List<VehicleMap> loadVehicleMap()
        {
            List<VehicleMap> vehicleLst = null;

            try
            {
                vehicleLst = vehicleMapDao.loadVehicle();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return vehicleLst;
        }

        public List<SecsCmdSet> loadSecsCmdSet()
        {
            List<SecsCmdSet> secsCmdLst = null;

            try
            {
                secsCmdLst = secsCmdSetDao.loadSecsCmdSet();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return secsCmdLst;
        }

    }
}
