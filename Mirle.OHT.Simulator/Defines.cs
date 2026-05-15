using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agvc.Simulator
{
    public enum EnumCmdNums
    {
        Cmd000_EmptyCommand = 0,
        Cmd31_TransferRequest = 31,
        Cmd32_TransferCompleteResponse = 32,
        Cmd33_ControlZoneCancelRequest = 33,
        Cmd35_CarrierIdRenameRequest = 35,
        Cmd36_TransferEventResponse = 36,
        Cmd37_TransferCancelRequest = 37,
        Cmd39_PauseRequest = 39,
        Cmd41_ModeChange = 41,
        Cmd43_StatusRequest = 43,
        Cmd44_StatusRequest = 44,
        Cmd45_PowerOnoffRequest = 45,
        Cmd51_AvoidRequest = 51,
        Cmd52_AvoidCompleteResponse = 52,
        Cmd71_RangeTeachRequest = 71,
        Cmd72_RangeTeachCompleteResponse = 72,
        Cmd74_AddressTeachResponse = 74,
        Cmd91_AlarmResetRequest = 91,
        Cmd94_AlarmResponse = 94,
        Cmd131_TransferResponse = 131,
        Cmd132_TransferCompleteReport = 132,
        Cmd133_ControlZoneCancelResponse = 133,
        Cmd134_TransferEventReport = 134,
        Cmd135_CarrierIdRenameResponse = 135,
        Cmd136_TransferEventReport = 136,
        Cmd137_TransferCancelResponse = 137,
        Cmd139_PauseResponse = 139,
        Cmd141_ModeChangeResponse = 141,
        Cmd143_StatusResponse = 143,
        Cmd144_StatusReport = 144,
        Cmd145_PowerOnoffResponse = 145,
        Cmd151_AvoidResponse = 151,
        Cmd152_AvoidCompleteReport = 152,
        Cmd171_RangeTeachResponse = 171,
        Cmd172_RangeTeachCompleteReport = 172,
        Cmd174_AddressTeachReport = 174,
        Cmd191_AlarmResetResponse = 191,
        Cmd194_AlarmReport = 194,
    }

    public enum EnumAddress
    {
        A,
        B,
        G
    }

    public enum EnumOperaType
    {
        NormalComplete = 0,
        CancelComplete = 1,
        AbortComplete = 2,
        ErrorComplete = 3,
        Abnormal_BcrReadFail = 4,
        Abnormal_BcrMismatch = 5,
        Abnormal_BcrDuplicate = 6,
        Abnormal_DoubleStorage = 7,
        Abnormal_EmptyRetrieval = 8,
        InterlockError = 9,
        MTL = 10,
        SystemOut = 11,
        ScanMismatche = 12,
        ScanFail = 13,
        InterlockErrorUnload = 14,
        RejectCommand = 15,
        RejectModeChange = 16
    }

    /// <summary>
    /// 命令Queue狀態枚舉
    /// </summary>
    public enum CmdQueueStatus
    {
        NoCommand,      // 無命令
        OneExecuting,   // 一個執行中
        OneQueueing     // 一個執行中+一個Queue
    }
}
