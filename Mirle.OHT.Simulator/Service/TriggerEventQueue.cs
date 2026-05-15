using System;
using System.Collections.Generic;
using System.Threading;
using static com.mirle.ibg3k0.sc.Service.gRPC.VehicleMessageRceive;

namespace com.mirle.ibg3k0.sc.Service.gRPC
{
    internal class TriggerEventQueue
    {
        NLog.Logger logger = NLog.LogManager.GetLogger("gRCPWithVh");
        private readonly object _eventSyncRoot = new object();
        private readonly Queue<WaitCallback> _eventDelegateQueue = new Queue<WaitCallback>();
        private String name = string.Empty;
        private WorkKeyFlag flag = new WorkKeyFlag();
        private Object _lock = new Object();
        public TriggerEventQueue(string name/*, ILicenseKey licenseKey*/)
        {
            this.name = name;
            //            this.licenseKey = licenseKey;
            handlerEvent += handler;
        }

        public void onNotify(ChangeEventStruct _eventStruct)
        {
            WaitCallback eventDelegate = delegate (object obj)
            {
                ChangeEventStruct eventStruct = _eventStruct;
                if (eventStruct.EventHandler != null)
                {
                    string eventTraceInfo = string.Format("Begin Trigger Event Handler By Queue-Pack ID:{0},Seq Num:{1},name:{2},[Thread ID:{3}]"
                                                        , eventStruct.EventArgs.PacketID
                                                        , eventStruct.EventArgs.iSeqNum.ToString()
                                                        , this.name
                                                        , Thread.CurrentThread.ManagedThreadId);
                    logger.Info(eventTraceInfo);
                    DateTime startTime = DateTime.Now;
                    eventStruct.EventHandler(this, eventStruct.EventArgs);
                    double totalMS = DateTime.Now.Subtract(startTime).TotalMilliseconds;
                    eventTraceInfo = string.Format("Complete Trigger Event Handler By Queue-Pack ID:{0},Seq Num:{1},name:{2},[Thread ID:{3}]"
                                                        , eventStruct.EventArgs.PacketID
                                                        , eventStruct.EventArgs.iSeqNum.ToString()
                                                        , this.name
                                                        , Thread.CurrentThread.ManagedThreadId);
                    logger.Info(eventTraceInfo);
                }
            };
            lock (_eventDelegateQueue)
            {
                _eventDelegateQueue.Enqueue(eventDelegate);
            }
            if (flag.Compare(WorkKeyFlag.UNSET))
            {
                ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(doWork), null);
            }
        }

        private void doWork(Object wKey)
        {
            if (flag.CompareAndSet(WorkKeyFlag.UNSET, WorkKeyFlag.SET))
            {
                logger.Info("TriggerEventQueue doWork is Running.[Thread ID:{0}]", Thread.CurrentThread.ManagedThreadId);
                while (true)
                {
                    try
                    {
                        WaitCallback eventDelegate = null;
                        lock (_eventDelegateQueue)
                        {
                            if (_eventDelegateQueue.Count <= 0)
                            {
                                break;
                            }
                            eventDelegate = _eventDelegateQueue.Dequeue();
                        }
                        HandlerDelgate tmpHandlerEvent = handlerEvent;
                        if (tmpHandlerEvent != null && eventDelegate != null)
                        {

                            tmpHandlerEvent(eventDelegate);

                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                    }
                }
                flag.CompareAndSet(WorkKeyFlag.SET, WorkKeyFlag.UNSET);
                logger.Info("TriggerEventQueue doWork is Finish.[Thread ID:{0}]", Thread.CurrentThread.ManagedThreadId);
            }
            else
            {

            }
        }

        public delegate void HandlerDelgate(WaitCallback eventDelegate);
        public event HandlerDelgate handlerEvent;
        private void handler(WaitCallback eventDelegate)
        {
            eventDelegate(null);
        }

    }

    internal class WorkKeyFlag
    {
        public static readonly int SET = 1;
        public static readonly int UNSET = 0;

        private int eventPoint;
        public WorkKeyFlag()
        {
            eventPoint = UNSET;
        }

        public int Exchange(int update)
        {
            return Interlocked.Exchange(ref eventPoint, update);
        }

        public bool Compare(int expect)
        {
            return Interlocked.CompareExchange(ref eventPoint, eventPoint, expect) == expect;
        }

        public bool CompareAndSet(int expect, int update)
        {
            return Interlocked.CompareExchange(ref eventPoint, update, expect) == expect;
        }
    }
}
