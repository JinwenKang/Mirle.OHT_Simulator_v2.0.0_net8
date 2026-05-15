using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.sc.ProtocolFormat.OHTMessage;
using Google.Protobuf.Collections;
using NLog;

namespace Mirle.Agvc.Simulator
{
    class Vehicle
    {
        private static LoggerAgent theLoggerAgent;
        private ID_144_STATUS_CHANGE_REP vehicle_Data = new ID_144_STATUS_CHANGE_REP();

        public ID_144_STATUS_CHANGE_REP Vehicle_Data
        {
            get => vehicle_Data;
        }
        public ID_144_STATUS_CHANGE_REP ChangeDataOfVehicle
        {
            set => vehicle_Data = value;
        }

        public bool Vehicle_Initialize()
        {
            theLoggerAgent = LoggerAgent.Instance;
            bool isSuccess = true;
            isSuccess = isSuccess && SetValueToVehicleData();
            return isSuccess;
        }

        private bool SetValueToVehicleData()
        {
            try
            {
                #region set the data into the vehicl class object
                //
                vehicle_Data.CurrentAdrID = "10005";
                vehicle_Data.CurrentSecID = "00105";
                vehicle_Data.ModeStatus = VHModeStatus.AutoRemote;
                vehicle_Data.ActionStatus = VHActionStatus.NoCommand;
                vehicle_Data.PowerStatus = VhPowerStatus.PowerOn;
                vehicle_Data.ObstacleStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.BlockingStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.HIDStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.PauseStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.ErrorStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.ReserveStatus = VhStopSingle.StopSingleOff;
                vehicle_Data.SecDistance = 0;
                vehicle_Data.ObstDistance = 0;
                vehicle_Data.ObstVehicleID = "";
                vehicle_Data.StoppedBlockID = "";
                vehicle_Data.StoppedHIDID = "";
                vehicle_Data.EarthquakePauseTatus = VhStopSingle.StopSingleOff;
                vehicle_Data.SafetyPauseStatus = VhStopSingle.StopSingleOff;
                #region vehicle_Data.ReserveInfos needs to be added by this type;
                ////
                FitReserveInfos(vehicle_Data.ReserveInfos, "");
                ////
                #endregion
                vehicle_Data.DrivingDirection = 0;
                vehicle_Data.Speed = 0;
                vehicle_Data.Angle = 0;
                vehicle_Data.XAxis = 0;
                vehicle_Data.YAxis = 0;
                vehicle_Data.CmdID = "";
                vehicle_Data.BOXID = "";
                vehicle_Data.CSTID = "";
                vehicle_Data.LOTID = "";
                vehicle_Data.HasBox = VhLoadCarrierStatus.NotExist;
                vehicle_Data.CarBoxID = "";
                vehicle_Data.HasBox = VhLoadCarrierStatus.NotExist;
                vehicle_Data.CarCstID = "";
                return true;
                //
                #endregion
            }
            catch (Exception ex)
            {
                LogFormat logFormat = new LogFormat("Error_", "1", "SetValueToVehicleData", "OHT01", "CarrierID_01", "Error initial : "+ ex.ToString());
                theLoggerAgent.LogMsg("Error_", logFormat);
                return false;
            }
        }

        private void FitReserveInfos(RepeatedField<ReserveInfo> reserveInfos, string sectionId)
        {
            reserveInfos.Clear();
            ReserveInfo reserveInfo = new ReserveInfo();
            reserveInfo.ReserveSectionID = sectionId;
            reserveInfo.DriveDirction = DriveDirction.DriveDirForward;
            reserveInfos.Add(reserveInfo);
        }
    }
}
