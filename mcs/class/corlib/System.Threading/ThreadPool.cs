//
// System.Threading.ThreadPool
//
// Author:
//   Patrik Torstensson (patrik.torstensson@labs2.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
// (C) Patrik Torstensson
//
using System;
using System.Collections;

namespace System.Threading
{
   /// <summary> (Patrik T notes)
   /// This threadpool is focused on saving resources not giving max performance. 
   /// 
   /// Note, this class is not perfect but it works. ;-) Should also replace
   /// the queue with an internal one (performance)
   /// 
   /// This class should also use a specialized queue to increase performance..
   /// </summary>
   /// 
   public sealed class ThreadPool {
      internal struct ThreadPoolWorkItem {
         public WaitCallback _CallBack;
         public object _Context;
      }

      private int _ThreadTimeout;

      private long _MaxThreads;
      private long _CurrentThreads;
      private long _ThreadsInUse;
      private long _RequestInQueue;
      private long _ThreadCreateTriggerRequests;

      private Thread _MonitorThread;
      private Queue _RequestQueue;

      private ArrayList _Threads;
      private ManualResetEvent _DataInQueue; 

      static ThreadPool _Threadpool;

      static ThreadPool() {
         _Threadpool = new ThreadPool();
      }

      private ThreadPool() {
         // 30 sec timeout default
         _ThreadTimeout = 30 * 1000; 

         // Used to signal that there is data in the queue
         _DataInQueue = new ManualResetEvent(false);
         
         _Threads = ArrayList.Synchronized(new ArrayList());

         // Holds requests..
         _RequestQueue = Queue.Synchronized(new Queue(128));

         _MaxThreads = 64;
         _CurrentThreads = 0;
         _RequestInQueue = 0;
         _ThreadsInUse = 0;
         _ThreadCreateTriggerRequests = 5;

         // Keeps track of requests in the queue and increases the number of threads if needed
         _MonitorThread = new Thread(new ThreadStart(MonitorThread));
         _MonitorThread.Start();
      }

      internal void RemoveThread() {
         Interlocked.Decrement(ref _CurrentThreads);
         _Threads.Remove(Thread.CurrentThread);
      }

      internal void CheckIfStartThread() {
         bool bCreateThread = false;

         if (_CurrentThreads == 0) {
            bCreateThread = true;
         }

         if ((_MaxThreads == -1 || _CurrentThreads < _MaxThreads) && _ThreadsInUse > 0 && _RequestInQueue > _ThreadCreateTriggerRequests) {
            bCreateThread = true;
         }

         if (bCreateThread) {
            Interlocked.Increment(ref _CurrentThreads);
      
            Thread Start = new Thread(new ThreadStart(WorkerThread));
            Start.IsThreadPoolThreadInternal = true;
            Start.Start();
            
            _Threads.Add(Start);
         }
      }

      internal void AddItem(ref ThreadPoolWorkItem Item) {
         if (Interlocked.Increment(ref _RequestInQueue) == 1) {
            _DataInQueue.Set();
         }

         _RequestQueue.Enqueue(Item);
      }

      // Work Thread main function
      internal void WorkerThread() {
         bool bWaitForData = true;

         while (true) {
            if (bWaitForData) {
               if (!_DataInQueue.WaitOne(_ThreadTimeout, false)) {
                  // timeout
                  RemoveThread();
                  return;
               }
            }

            Interlocked.Increment(ref _ThreadsInUse);

            try {
               ThreadPoolWorkItem oItem = (ThreadPoolWorkItem) _RequestQueue.Dequeue();

               if (Interlocked.Decrement(ref _RequestInQueue) == 0) {
                  _DataInQueue.Reset();
               }

               oItem._CallBack(oItem._Context);
            }
            catch (InvalidOperationException) {
               // Queue empty
               bWaitForData = true;
            }
            catch (ThreadAbortException) {
               // We will leave here.. (thread abort can't be handled)
               RemoveThread();
            }
            finally {
               Interlocked.Decrement(ref _ThreadsInUse);
            }
         }
      }

      internal void MonitorThread() {
         while (true) {
            if (_DataInQueue.WaitOne ()) {
		    CheckIfStartThread();
            }

            Thread.Sleep(500);
         }
      }

      internal bool QueueUserWorkItemInternal(WaitCallback callback) {
         return QueueUserWorkItem(callback, null);
      }

      internal bool QueueUserWorkItemInternal(WaitCallback callback, object context) {
         ThreadPoolWorkItem Item = new ThreadPoolWorkItem();

         Item._CallBack = callback;
         Item._Context = context;

         AddItem(ref Item);

         // LAMESPEC: Return value? should use exception here if anything goes wrong
         return true;
      }

      public static bool BindHandle(IntPtr osHandle) {
         throw new NotSupportedException("This is a win32 specific method, not supported Mono");
		}

		public static bool QueueUserWorkItem(WaitCallback callback) {
         return _Threadpool.QueueUserWorkItemInternal(callback);
		}

		public static bool QueueUserWorkItem(WaitCallback callback, object state) {
         return _Threadpool.QueueUserWorkItemInternal(callback, state);
		}

      public static bool UnsafeQueueUserWorkItem(WaitCallback callback, object state) {
         return _Threadpool.QueueUserWorkItemInternal(callback, state);
      }

      [MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce) {
			if (millisecondsTimeOutInterval < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}

         throw new NotImplementedException();
      }

		[MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce) {
			if (millisecondsTimeOutInterval < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}
		
         throw new NotImplementedException();
      }

		[MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce) {
			// LAMESPEC: I assume it means "timeout" when it says "millisecondsTimeOutInterval"
			if (timeout.Milliseconds < -1) {
				throw new ArgumentOutOfRangeException("timeout < -1");
			}
			if (timeout.Milliseconds > Int32.MaxValue) {
				throw new NotSupportedException("timeout too large");
			}

         throw new NotImplementedException();
      }

      [CLSCompliant(false)][MonoTODO]
		public static RegisteredWaitHandle RegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce) {
         throw new NotImplementedException();
      }

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, int millisecondsTimeOutInterval, bool executeOnlyOnce) {
         throw new NotImplementedException();
      }

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, long millisecondsTimeOutInterval, bool executeOnlyOnce) {
         throw new NotImplementedException();
		}

		[MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, TimeSpan timeout, bool executeOnlyOnce) {
         throw new NotImplementedException();
      }

		[CLSCompliant(false)][MonoTODO]
		public static RegisteredWaitHandle UnsafeRegisterWaitForSingleObject(WaitHandle waitObject, WaitOrTimerCallback callback, object state, uint millisecondsTimeOutInterval, bool executeOnlyOnce) {
         throw new NotImplementedException();
      }
	}
}
