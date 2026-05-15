using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agvc.Simulator.App
{
    public class SCAppConstants
    {
        public static readonly string TimestampFormat_12 = "yyMMddHHmmss";

        public class AddressType
        {
            public const int Normal = 0;

            public const int Control = 1;
        }

        public class Direction
        {
            public const int Right = 0;

            public const int Up = 1;

            public const int Down = 2;

            public const int Left = 3;
        }

        public class CurrentAction
        {
            public const int None = 0;

            public const int Initial = 1;

            public const int Move = 2;

            public const int MoveToLoad = 3;

            public const int Load = 4;

            public const int MoveToUnload = 5;

            public const int Unload = 6;
        }

    }
}
