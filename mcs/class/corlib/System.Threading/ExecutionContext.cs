// 
// System.Threading.ExecutionContext.cs
//
// Authors:
//	Lluis Sanchez (lluis@novell.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;
#if !DISABLE_SECURITY
using System.Security.Permissions;
#endif

namespace System.Threading {

	[Serializable]
#if NET_2_0
	public sealed class ExecutionContext : ISerializable {
#else
	internal sealed class ExecutionContext : ISerializable {
#endif
#if (!NET_2_1 || MONOTOUCH) && !DISABLE_SECURITY
		private SecurityContext _sc;
#endif
		private bool _suppressFlow;
		private bool _capture;

		internal ExecutionContext ()
		{
		}

		internal ExecutionContext (ExecutionContext ec)
		{
#if (!NET_2_1 || MONOTOUCH) && !DISABLE_SECURITY
			if (ec._sc != null)
				_sc = new SecurityContext (ec._sc);
#endif
			_suppressFlow = ec._suppressFlow;
			_capture = true;
		}
		
		[MonoTODO]
		internal ExecutionContext (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		public static ExecutionContext Capture ()
		{
			ExecutionContext ec = Thread.CurrentThread.ExecutionContext;
			if (ec.FlowSuppressed)
				return null;

			ExecutionContext capture = new ExecutionContext (ec);
#if (!NET_2_1 || MONOTOUCH) && !DISABLE_SECURITY
			if (SecurityManager.SecurityEnabled)
				capture.SecurityContext = SecurityContext.Capture ();
#endif
			return capture;
		}
		
		public ExecutionContext CreateCopy ()
		{
			if (!_capture)
				throw new InvalidOperationException ();

			return new ExecutionContext (this);
		}

		[MonoTODO]
		#if !DISABLE_SECURITY
		[ReflectionPermission (SecurityAction.Demand, MemberAccess = true)]
		#endif
		public void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");
			throw new NotImplementedException ();
		}
		
		// internal stuff
#if (!NET_2_1 || MONOTOUCH) && !DISABLE_SECURITY
		internal SecurityContext SecurityContext {
			get {
				if (_sc == null)
					_sc = new SecurityContext ();
				return _sc;
			}
			set { _sc = value; }
		}
#endif
		internal bool FlowSuppressed {
			get { return _suppressFlow; }
			set { _suppressFlow = value; }
		}

		// Note: Previous to version 2.0 only the CompressedStack and (sometimes!) the WindowsIdentity
		// were propagated to new threads. This is why ExecutionContext is internal in before NET_2_0.
		// It also means that all newer context classes should be here (i.e. inside the #if NET_2_0).

		public static bool IsFlowSuppressed ()
		{
			return Thread.CurrentThread.ExecutionContext.FlowSuppressed;
		}

		public static void RestoreFlow ()
		{
			ExecutionContext ec = Thread.CurrentThread.ExecutionContext;
			if (!ec.FlowSuppressed)
				throw new InvalidOperationException ();

			ec.FlowSuppressed = false;
		}
#if NET_2_0 && (!NET_2_1 || MONOTOUCH) && !MICRO_LIB
		[MonoTODO ("only the SecurityContext is considered")]
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		public static void Run (ExecutionContext executionContext, ContextCallback callback, object state)
		{
			if (executionContext == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Null ExecutionContext"));
			}

			// FIXME: supporting more than one context (the SecurityContext)
			// will requires a rewrite of this method

			SecurityContext.Run (executionContext.SecurityContext, callback, state);
		}
#endif
#if !NET_2_1 || MONOTOUCH
		public static AsyncFlowControl SuppressFlow ()
		{
			Thread t = Thread.CurrentThread;
			t.ExecutionContext.FlowSuppressed = true;
			return new AsyncFlowControl (t, AsyncFlowControlType.Execution);
		}
#endif
	}
}
