//
// System.Security.SecurityContext class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;

namespace System.Security {

	[MonoTODO ("need to determine internals")]
	[ComVisible (false)]
	public sealed class SecurityContext {

		static private bool _flowSuppressed;
		static private bool _windowsIdentityFlowSuppressed;

		private bool _usedForSetSecurityContext;
		private WindowsIdentity _winid;
		private CompressedStack _stack;

		internal SecurityContext ()
		{
			_usedForSetSecurityContext = false;
		}

		// copy constructor
		internal SecurityContext (SecurityContext sc)
		{
			_usedForSetSecurityContext = sc._usedForSetSecurityContext;
			_winid = sc._winid;
			_stack = sc._stack.CreateCopy ();
		}

		public SecurityContext CreateCopy ()
		{
			if (_usedForSetSecurityContext) {
				throw new InvalidOperationException (Locale.GetText (
					"SecurityContext used for SetSecurityContext"));
			}
			return new SecurityContext (this);
		}

// LAMESPEC: documented but not implemented (not shown by corcompare)
#if false
		public override bool Equals (object obj)
		{
			return false;
		}

		public override int GetHashCode ()
		{
			return 0;
		}

		public void Undo ()
		{
		}
#endif
		// static methods

		static public SecurityContext Capture ()
		{
			return new SecurityContext ();
		}

		static public bool IsFlowSuppressed ()
		{
			return _flowSuppressed;
		} 

		static public bool IsWindowsIdentityFlowSuppressed ()
		{
			return _windowsIdentityFlowSuppressed;
		}

		static public void RestoreFlow ()
		{
			_flowSuppressed = false;
		}

		static public void Run (SecurityContext securityContext, ContextCallback callBack, object state)
		{
			if (securityContext == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Null SecurityContext"));
			}
		}

		static public SecurityContextSwitcher SetSecurityContext (SecurityContext securityContext)
		{
			if (securityContext == null) {
				throw new InvalidOperationException (Locale.GetText (
					"Null SecurityContext"));
			}

			securityContext._usedForSetSecurityContext = true;
			return new SecurityContextSwitcher ();
		}

		static public AsyncFlowControl SuppressFlow ()
		{
			_flowSuppressed = true;
			return new AsyncFlowControl ();
		}

		static public AsyncFlowControl SuppressFlowWindowsIdentity ()
		{
			_windowsIdentityFlowSuppressed = true;
			return new AsyncFlowControl ();
		}
	}
}

#endif
