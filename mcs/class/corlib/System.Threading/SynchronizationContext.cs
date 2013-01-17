// 
// System.Threading.SynchronizationContext.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading 
{
	public class SynchronizationContext
	{
		bool notification_required;

		[ThreadStatic]
		static SynchronizationContext currentContext;
		
		public SynchronizationContext ()
		{
		}

		internal SynchronizationContext (SynchronizationContext context)
		{
			currentContext = context;
		}
		
		public static SynchronizationContext Current {
			get {
#if MONODROID
				if (currentContext == null)
					currentContext = AndroidPlatform.GetDefaultSyncContext ();
#endif
				return currentContext;
			}
		}

		public virtual SynchronizationContext CreateCopy ()
		{
			return new SynchronizationContext (this);
		}

		public bool IsWaitNotificationRequired ()
		{
			return notification_required;
		}

		public virtual void OperationCompleted ()
		{
		}

		public virtual void OperationStarted ()
		{
		}
		
		public virtual void Post (SendOrPostCallback d, object state)
		{
			ThreadPool.QueueUserWorkItem (new WaitCallback (d), state);
		}
		
		public virtual void Send (SendOrPostCallback d, object state)
		{
			d (state);
		}
		
		public static void SetSynchronizationContext (SynchronizationContext syncContext)
		{
			currentContext = syncContext;
		}

#if NET_2_1
		[Obsolete]
		public static void SetThreadStaticContext (SynchronizationContext syncContext)
		{
			currentContext = syncContext;
		}
#endif

		[MonoTODO]
		protected void SetWaitNotificationRequired ()
		{
			notification_required = true;
			throw new NotImplementedException ();
		}

		[CLSCompliant (false)]
		[PrePrepareMethod ()]
		public virtual int Wait (IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return WaitHelper (waitHandles, waitAll, millisecondsTimeout);
		}

		[MonoTODO]
		[CLSCompliant (false)]
		[ReliabilityContract (Consistency.WillNotCorruptState, Cer.MayFail)]
		[PrePrepareMethod ()]
		protected static int WaitHelper (IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}
	}
}
