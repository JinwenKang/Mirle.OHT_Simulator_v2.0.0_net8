using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using TcpIpClientSample;

namespace Mirle.Agvc.Simulator
{
    [Serializable]
    public class LoadunloadCmd
    {
        public string CmdText { get; set; } = "Empty";
        public EnumAddress StartAddress { get; set; } = EnumAddress.A;
        public EnumAddress EndAddress { get; set; } = EnumAddress.B;
        //public Dictionary<string, string> CommandInfoPairs { get; set; } = new Dictionary<string, string>();

        public string CmdId { get; set; } = "Cmd001";
        public string CstId { get; set; } = "CA0070";
        //public ActiveType ActType { get; set; } = ActiveType.Loadunload;
        public List<string> GuideSectionsStartToLoad { get; set; } = new List<string>();
        public List<string> GuideAddressesStartToLoad { get; set; } = new List<string>();
        public string LoadAdr { get; set; }
        public List<string> GuideSectionsToDestination { get; set; } = new List<string>();
        public List<string> GuideAddressesToDestination { get; set; } = new List<string>();
        public string DestinationAdr { get; set; }
        public uint SecDistance { get; set; } = 100;

        //public void SetupCommandInfoPairs()
        //{
        //    CommandInfoPairs.Add("CmdID", CmdId);
        //    CommandInfoPairs.Add("CSTID", CstId);
        //    CommandInfoPairs.Add("ActType", ActType.ToString());
        //    CommandInfoPairs.Add("GuideSectionsStartToLoad", SetupCommandInfoPairFromBuffer(GuideSectionsStartToLoad));
        //    CommandInfoPairs.Add("GuideAddressesStartToLoad", SetupCommandInfoPairFromBuffer(GuideAddressesStartToLoad));
        //    CommandInfoPairs.Add("LoadAdr", LoadAdr);
        //    CommandInfoPairs.Add("GuideSectionsToDestination", SetupCommandInfoPairFromBuffer(GuideSectionsToDestination));
        //    CommandInfoPairs.Add("GuideAddressesToDestination", SetupCommandInfoPairFromBuffer(GuideAddressesToDestination));
        //    CommandInfoPairs.Add("DestinationAdr", DestinationAdr);
        //    CommandInfoPairs.Add("SecDistance", SecDistance.ToString());
        //}

        //public string SetupCommandInfoPairFromBuffer(List<string> strbuffer)
        //{
        //    string v = "[";
        //    for (int i = 0; i < strbuffer.Count - 1; i++)
        //    {
        //        v += strbuffer[i] + ",";
        //    }
        //    v += strbuffer[strbuffer.Count - 1] + "]";
        //    return v;
        //}
    }
}
