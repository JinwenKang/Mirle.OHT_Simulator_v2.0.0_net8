using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agvc.Simulator
{
    class CmdQueue
    {
        private Queue<Object> cmdQueue = new Queue<object>();
        private Object nowCmd = new object();
        public CmdQueue()
        {

        }

        public bool Initialize()
        {
            return true;
        }

        public bool AddCmdIn(object cmd_Data)
        {
            bool isSuccess = true;
            try
            {
                cmdQueue.Enqueue(cmd_Data);
            }
            catch
            {
                isSuccess = false;
            }
            return isSuccess;
        }

        public bool GetCmdOut (out object cmd_Data)
        {
            bool isSuccess = true;
            try
            {
                cmd_Data= cmdQueue.Dequeue();
            }
            catch
            {
                cmd_Data = "Defeat";
                isSuccess = false;
            }
            return isSuccess;
        }
    }
}
