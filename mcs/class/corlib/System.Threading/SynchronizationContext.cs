// 
// System.Threading.SynchronizationContext.cs
//
// Author:
//   Lluis Sanchez (lluis@novell.com)
//
// Copyright (C) Novell, Inc., 2004
//

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

#if NET_2_0

using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace System.Threading 
{
	[ComVisibleAttribute (false)]
	public class SynchronizationContext
	{
		[ThreadStatic]
		static SynchronizationContext currentContext;
		
		public SynchronizationContext ()
		{
		}

		internal SynchronizationContext (SynchronizationContext context)
		{
			currentContext = context;
		}
		
		public static SynchronizationContext Current
		{
			get { return currentContext; }
		}

		public virtual SynchronizationContext CreateCopy ()
		{
			return new SynchronizationContext (this);
		}
		
		public virtual void Post (SendOrPostCallback d, object state)
		{
			d.BeginInvoke (state, null, null);
		}
		
		public virtual void Send (SendOrPostCallback d, object state)
		{
			d (state);
		}
		
		public virtual void SendOrPost (SendOrPostCallback d, object state)
		{
			Send (d, state);
		}

		[MonoTODO]
		public virtual void SendOrPost (SendOrPostCallback d, object state, ExecutionContext ec)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static SynchronizationContextSwitcher SetSynchronizationContext (SynchronizationContext syncContext)
		{
			throw new NotImplementedException ();
		}

		[CLSCompliant (false)]
		public virtual int Wait (IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			return WaitHelper (waitHandles, waitAll, millisecondsTimeout);
		}

		[MonoTODO]
		[CLSCompliant (false)]
		[ReliabilityContract (Consistency.WillNotCorruptState, CER.MayFail)]
		protected static int WaitHelper (IntPtr[] waitHandles, bool waitAll, int millisecondsTimeout)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
