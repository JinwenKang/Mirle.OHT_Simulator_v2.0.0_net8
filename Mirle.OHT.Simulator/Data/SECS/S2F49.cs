using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using com.mirle.ibg3k0.stc.Common;
using com.mirle.ibg3k0.stc.Data.SecsData;

namespace com.mirle.ibg3k0.sc.Data.SECS
{
    public class S2F49 : SXFY
    {
        [SecsElement(Index = 1, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_4_BYTE_UNSIGNED_INTEGER, Length = 1)]
        public string DATAID;
        [SecsElement(Index = 2, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string OBJSPEC;
        [SecsElement(Index = 3, ListSpreadOut = true, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
        public string RCMD;
        [SecsElement(Index = 4)]
        public REPITEM REPITEMS;

        public S2F49()
        {
            StreamFunction = "S2F49";
            W_Bit = 1;
        }

        public class REPITEM : SXFY
        {
            [SecsElement(Index = 1)]
            public COMM COMMINFO;
            [SecsElement(Index = 2)]
            public TRAN TRANINFO;
            //[SecsElement(Index = 3)]
            //public CST CSTINFO;
            //[SecsElement(Index = 4)]
            //public STAGEIDLSIT STAGEIDLIST;

            public class COMM : SXFY
            {
                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string COMMANDINFO;
                [SecsElement(Index = 2)]
                public COMMA COMMAINFO;

                public class COMMA : SXFY
                {
                    [SecsElement(Index = 1)]
                    public COMMANDID COMMANDIDINFO;
                    [SecsElement(Index = 2)]
                    public PRIO PRIORITY;
                    [SecsElement(Index = 3)]
                    public PRIO REPLACE;

                    public class COMMANDID : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string COMMANDID1;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)] //Modify Length 1 > 10 By Kevin
                        public string CommandID;
                    }

                    public class PRIO : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string PRIORITY;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_2_BYTE_UNSIGNED_INTEGER, Length = 1)]
                        public string CommandID;
                    }
                }
            }
            public class TRAN : SXFY
            {
                [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string TRANSFERINFO;
                [SecsElement(Index = 2)]
                public CARR CARRINFO;

                public class CARR : SXFY
                {
                    [SecsElement(Index = 1)]
                    public CARRIERID CARRIERIDINFO;
                    [SecsElement(Index = 2)]
                    public SOU SOUINFO;
                    [SecsElement(Index = 3)]
                    public DEST DESTINFO;

                    public class CARRIERID : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string CARRIERID1;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)] //Modify Length 1 > 10 By Kevin
                        public string CarrierID;
                    }

                    public class SOU : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string SOURCE;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]//Modify Length 1 > 40 By Kevin
                        public string Source;
                    }

                    public class DEST : SXFY
                    {
                        [SecsElement(Index = 1, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string DEST1;
                        [SecsElement(Index = 2, Type = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]//Modify Length 1 > 40 By Kevin
                        public string Dest;
                    }
                }
            }
            public class CST : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                public string TRANSFERINFO_CST;
                [SecsElement(Index = 2)]
                public CARR CARRINFO;

                public class CARR : SXFY
                {
                    [SecsElement(Index = 1)]
                    public CASSETTE_SIZE CARRIERIDINFO;
                    [SecsElement(Index = 2)]
                    public EMPTYCARRIER SOUINFO;

                    public class CASSETTE_SIZE : SXFY
                    {
                        [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string CASSETTE_SIZE_NAME;
                        [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)] //Modify Length 1 > 10 By Kevin
                        public string CASSETTE_SIZE_VALUE;
                    }

                    public class EMPTYCARRIER : SXFY
                    {
                        [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 40)]
                        public string GLASS_DATA_NAME;
                        [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 64)]//Modify Length 1 > 40 By Kevin
                        public string GLASS_DATA_VALUE;
                    }
                }
            }

            public class STAGEIDLSIT : SXFY
            {
                [SecsElement(Index = 1, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, Length = 12)]
                public string STAGE_ID_LIST_NAME;
                [SecsElement(Index = 2, ListElementType = SecsElement.SecsElementType.TYPE_ASCII, ListElementLength = 10)]
                public string[] STAGE_ID_LIST;
            }


        }
    }
}
