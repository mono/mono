//
// System.Security.SecurityContext class
//
// Author:
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
using System.Security.Permissions;
using System.Security.Principal;
using System.Threading;

namespace System.Security {

	public sealed class SecurityContext
#if NET_4_0
		: IDisposable
#endif
	{
		private bool _capture;
		private IntPtr _winid;
		private CompressedStack _stack;
		private bool _suppressFlowWindowsIdentity;
		private bool _suppressFlow;

		internal SecurityContext ()
		{
		}

		// copy constructor
		internal SecurityContext (SecurityContext sc)
		{
			_capture = true;
#if !MOBILE
			_winid = sc._winid;
			if (sc._stack != null)
				_stack = sc._stack.CreateCopy ();
#endif
		}

		public SecurityContext CreateCopy ()
		{
			if (!_capture)
				throw new InvalidOperationException ();

			return new SecurityContext (this);
		}

		// static methods

		static public SecurityContext Capture ()
		{
			SecurityContext sc = Thread.CurrentThread.ExecutionContext.SecurityContext;
			if (sc.FlowSuppressed)
				return null;

			SecurityContext capture = new SecurityContext ();
			capture._capture = true;
#if !MOBILE
			capture._winid = WindowsIdentity.GetCurrentToken ();
			capture._stack = CompressedStack.Capture ();
#endif
			return capture;
		}
		
#if NET_4_0
		public void Dispose ()
		{
		}
#endif

		// internal stuff

		internal bool FlowSuppressed {
			get { return _suppressFlow; }
			set { _suppressFlow = value; }
		}

		internal bool WindowsIdentityFlowSuppressed {
			get { return _suppressFlowWindowsIdentity; }
			set { _suppressFlowWindowsIdentity = value; }
		}

		internal CompressedStack CompressedStack {
			get { return _stack; }
			set { _stack = value; }
		}

		internal IntPtr IdentityToken {
			get { return _winid; }
			set { _winid = value; }
		}

		// Suppressing the SecurityContext flow wasn't required before 2.0

		static public bool IsFlowSuppressed ()
		{
			return Thread.CurrentThread.ExecutionContext.SecurityContext.FlowSuppressed;
		} 

		static public bool IsWindowsIdentityFlowSuppressed ()
		{
			return Thread.CurrentThread.ExecutionContext.SecurityContext.WindowsIdentityFlowSuppressed;
		}

		static public void RestoreFlow ()
		{
			SecurityContext sc = Thread.CurrentThread.ExecutionContext.SecurityContext;
			// if nothing is suppressed then throw
			if (!sc.FlowSuppressed && !sc.WindowsIdentityFlowSuppressed)
				throw new InvalidOperationException ();

			sc.FlowSuppressed = false;
			sc.WindowsIdentityFlowSuppressed = false;
		}

		// if you got the context then you can use it
		[SecurityPermission (SecurityAction.Assert, ControlPrincipal = true)]
		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		static public void Run (SecurityContext securityContext, ContextCallback callback, object state)
		{
			if (securityContext == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Null SecurityContext"));
			}
#if MOBILE
			callback (state);
#else
			SecurityContext sc = Thread.CurrentThread.ExecutionContext.SecurityContext;
			IPrincipal original = Thread.CurrentPrincipal;
			try {
				if (sc.IdentityToken != IntPtr.Zero) {
					Thread.CurrentPrincipal = new WindowsPrincipal (new WindowsIdentity (sc.IdentityToken));
				}

				// FIXME: is the security manager isn't active then we may not have
				// a compressed stack (bug #78652)
				if (securityContext.CompressedStack != null)
					CompressedStack.Run (securityContext.CompressedStack, callback, state);
				else
					callback (state);
			}
			finally {
				if ((original != null) && (sc.IdentityToken != IntPtr.Zero))
					Thread.CurrentPrincipal = original;
			}
#endif
		}

		[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
		static public AsyncFlowControl SuppressFlow ()
		{
			Thread t = Thread.CurrentThread;
			// suppress both flows
			t.ExecutionContext.SecurityContext.FlowSuppressed = true;
			t.ExecutionContext.SecurityContext.WindowsIdentityFlowSuppressed = true;
			return new AsyncFlowControl (t, AsyncFlowControlType.Security);
		}

		static public AsyncFlowControl SuppressFlowWindowsIdentity ()
		{
			Thread t = Thread.CurrentThread;
			t.ExecutionContext.SecurityContext.WindowsIdentityFlowSuppressed = true;
			return new AsyncFlowControl (t, AsyncFlowControlType.Security);
		}
	}
}
